namespace Aristocrat.Monaco.Mgam.Services.Identification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Action;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Identification;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Cabinet.Contracts;
    using Common;
    using Common.Events;
    using Gaming.Contracts.InfoBar;
    using Hardware.Contracts.CardReader;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Monaco.Common;
    using Notification;
    using PlayerTracking;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Identification validator for the MGAM protocol
    /// </summary>
    public class IdentificationValidator : IIdentificationValidator, IService, IDisposable
    {
        private const string OperatorKeyName = "Operator";
        private const string EmployeeCardPrefix = "EMP";
        private const string PlayerCardPrefix = "PLY";

        private const string OperatorActionName = "ATIOperator";
        private const string TechnicianActionName = "ATITechnician";

        private readonly Guid _infoBarOwnershipKey = new Guid("{3BAF6CA2-567A-4ED6-AE99-A752C8BE980F}");
        private readonly ActionInfo _operatorAction;
        private readonly ActionInfo _technicianAction;

        private static readonly Guid OperatorMenuDisableKey = new Guid("{DB8B647F-5FC8-4BF8-B903-EA93D499C1AD}");

        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IEgm _egm;
        private readonly IEmployeeLogin _employeeLogin;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IHandlerConnector<IIdentificationValidator> _connector;
        private readonly IPlayerTracking _playerTracking;
        private readonly INotificationLift _notificationLift;
        private bool _ignoreKeySwitches;

        private IIdentification _identification;

        private bool _disposed;

        /// <summary>
        ///     The MGAM identification validator
        /// </summary>
        /// <param name="connector"><see cref="IIdentificationProvider"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        /// <param name="egm"><see cref="IEgm"/></param>
        /// <param name="employeeLogin"><see cref="IEmployeeLogin"/></param>
        /// <param name="operatorMenu"><see cref="IOperatorMenuLauncher"/></param>
        /// <param name="playerTracking"><see cref="IPlayerTracking"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/></param>
        public IdentificationValidator(
            IIdentificationProvider connector,
            ILogger<IdentificationValidator> logger,
            IEventBus eventBus,
            IEgm egm,
            IEmployeeLogin employeeLogin,
            IOperatorMenuLauncher operatorMenu,
            IPlayerTracking playerTracking,
            INotificationLift notificationLift)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _employeeLogin = employeeLogin ?? throw new ArgumentNullException(nameof(employeeLogin));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _playerTracking = playerTracking ?? throw new ArgumentNullException(nameof(playerTracking));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));

            _connector.Register(this, ProtocolNames.MGAM);

            var actions = SupportedActions.Get().ToArray();
            _operatorAction = actions.Single(a => a.Name == OperatorActionName);
            _technicianAction = actions.Single(a => a.Name == TechnicianActionName);

            _eventBus.Subscribe<OffEvent>(this, HandleEvent);
            _eventBus.Subscribe<OnEvent>(this, HandleEvent);
            _eventBus.Subscribe<HostOnlineEvent>(this, HandleEvent);
            _eventBus.Subscribe<HostOfflineEvent>(this, HandleEvent);
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, HandleEvent);
        }

        /// <inheritdoc />
        public string Name => typeof(IdentificationValidator).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IIdentificationValidator) };

        public bool IgnoreKeySwitches
        {
            get => _ignoreKeySwitches;
            set
            {
                if (_ignoreKeySwitches != value)
                {
                    _ignoreKeySwitches = value;
                    if (!_ignoreKeySwitches)
                    {
                        // coming out of ignoring the key switch.  Evaluate current key switch state.
                        if (!IsKeyTurned)
                        {
                            CloseMenu();
                        }
                    }
                }
            }
        }
        private string CurrentEmployeeId { get; set; }

        private bool IsHostOnline { get; set; }

        private bool IsKeyTurned { get; set; }

        private bool IsCardInserted { get; set; }

        private bool IsTechnician { get; set; }

        private CardType CurrentCardType { get; set; }

        /// <inheritdoc />
        public async Task ClearValidation(int readerId, CancellationToken token)
        {
            IsCardInserted = false;

            switch (CurrentCardType)
            {
                case CardType.Player:
                    await LogoffPlayer(token);
                    ShowMessage(Localizer.GetString(ResourceKeys.Identification_CardRemoved, CultureProviderType.Player), DisplayRole.VBD);
                    break;
                case CardType.Employee:
                    await LogoffEmployee();
                    ShowMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Identification_CardRemoved), DisplayRole.Main);
                    break;
            }

            CurrentCardType = CardType.Unknown;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _operatorMenu.DisableKey(OperatorMenuDisableKey);
            _identification = _egm.GetService<IIdentification>();
        }

        /// <inheritdoc />
        public void InitializeValidation(int readerId)
        {
        }

        /// <inheritdoc />
        public void HandleReadError(int readerId)
        {
            ShowMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Identification_ReadError), DisplayRole.Main, true);
            ShowMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Identification_ReadError), DisplayRole.VBD, true);
        }

        /// <inheritdoc />
        public async Task<bool> ValidateIdentification(int readerId, TrackData trackData, CancellationToken token)
        {
            const string error = "Validate Identification - error in validation.";
            IsCardInserted = true;

            if (!IsHostOnline && IsEmployeeCard(trackData?.Track1))
            {
                CurrentEmployeeId = GetEmployeeId(trackData?.Track1);
                CurrentCardType = CardType.Employee;
                return ShowMenu();
            }
            if (!IsHostOnline && !IsEmployeeCard(trackData?.Track1))
            {
                _logger.LogError("Validate Identification - unable to validate player while offline.");
            }
            else
            {
                try
                {
                    var result = await _identification.GetCardType(
                        trackData?.Track1,
                        trackData?.Track2,
                        trackData?.Track3,
                        token);

                    if (result.Status == MessageStatus.TimedOut || result.Status == MessageStatus.Aborted)
                    {
                        LogTaskCancelledError(error);
                    }
                    else if (result.Status == MessageStatus.Success && result.Response.ResponseCode == ServerResponseCode.Ok)
                    {
                        CurrentCardType = GetCardType(result.Response);
                        switch (CurrentCardType)
                        {
                            case CardType.Employee:
                                return await LoginEmployee(result.Response.CardString, token);

                            case CardType.Player:
                                return await LoginPlayer(result.Response.CardString, token);

                            default:
                                LogResponseCodeError("Validate Identification - unknown card type", result.Response, CultureProviderType.Operator, DisplayRole.Main, DisplayRole.VBD);
                                break;
                        }
                    }
                    else
                    {
                        LogResponseCodeError(error, result.Response, CultureProviderType.Operator, DisplayRole.Main);
                    }
                }
                catch (ServerResponseException ex)
                {
                    DisplayRole[] targetDisplays = {};

                    switch (CurrentCardType)
                    {
                        case CardType.Employee:
                            targetDisplays = new[] { DisplayRole.Main };
                            break;

                        case CardType.Player:
                            targetDisplays = new[] { DisplayRole.VBD };
                            break;

                        case CardType.Unknown:
                            targetDisplays = new[] { DisplayRole.Main, DisplayRole.VBD };
                            break;
                    }

                    LogResponseCodeError(error, ex, CultureProviderType.Operator, targetDisplays);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public async Task LogoffPlayer()
        {
            await LogoffPlayer(new CancellationToken());
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value that indicates whether the instance is being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CloseInfoBar(DisplayRole.Main);
                CloseInfoBar(DisplayRole.VBD);
                _eventBus.UnsubscribeAll(this);
                _connector.Clear(ProtocolNames.MGAM);
            }

            _disposed = true;
        }

        private CardType GetCardType(GetCardTypeResponse response)
        {
            switch (response.CardType)
            {
                case EmployeeCardPrefix:
                    return CardType.Employee;

                case PlayerCardPrefix:
                    return CardType.Player;

                default:
                    return CardType.Unknown;
            }
        }

        private string GetEmployeeId(string track1String)
        {
            return IsEmployeeCard(track1String) ? track1String.Replace(EmployeeCardPrefix, string.Empty) : string.Empty;
        }

        private EmployeeRole GetEmployeeRole(ActionInfo[] actions)
        {
            if (actions.Any(a => a == _technicianAction))
            {
                return EmployeeRole.Technician;
            }

            if (actions.Any(a => a == _operatorAction))
            {
                return EmployeeRole.Operator;
            }

            return EmployeeRole.Unknown;
        }

        private void HandleEvent(OffEvent e)
        {
            if (e.KeySwitchName.Equals(OperatorKeyName, StringComparison.Ordinal))
            {
                IsKeyTurned = false;
                if (!IgnoreKeySwitches)
                {
                    CloseMenu();
                }
            }
        }

        private void HandleEvent(OnEvent e)
        {
            if (e.KeySwitchName.Equals(OperatorKeyName, StringComparison.Ordinal))
            {
                IsKeyTurned = true;
                if (!IgnoreKeySwitches)
                {
                    ShowMenu();
                }
            }
        }

        private void HandleEvent(HostOnlineEvent e)
        {
            IsHostOnline = true;
        }

        private void HandleEvent(HostOfflineEvent e)
        {
            IsHostOnline = false;
        }

        private void HandleEvent(AttributesUpdatedEvent e)
        {
            // Revalidate currently inserted card if there is one when host goes back online
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>()?.Adapters?.FirstOrDefault();
            if (idReader?.TrackData != null)
            {
                _eventBus.Publish(new ValidationRequestedEvent(idReader.IdReaderId, idReader.TrackData));
            }
        }

        private void CloseMenu()
        {
            _operatorMenu.Close();
        }

        private bool IsEmployeeCard(string track1String)
        {
            if (string.IsNullOrEmpty(track1String))
            {
                return false;
            }

            return track1String.StartsWith(EmployeeCardPrefix);
        }

        private void LogResponseCodeError(string error, IResponse response, CultureProviderType providerType, params DisplayRole[] displayTargets)
        {
            foreach (var display in displayTargets)
            {
                ShowMessage(Localizer.GetString(ResourceKeys.Identification_InvalidCard, providerType), display, true);
            }

            _logger.LogError($"{error}. Response code: {response?.ResponseCode}.");
        }

        private void LogResponseCodeError(string error, ServerResponseException exception, CultureProviderType providerType, params DisplayRole[] displayTarget)
        {
            foreach (var display in displayTarget)
            {
                ShowMessage(Localizer.GetString(ResourceKeys.Identification_InvalidCard, providerType), display, true);
            }
            _logger.LogError($"{error}. Response code: {exception?.ResponseCode}.");
        }

        private void LogTaskCancelledError(string error)
        {
            _logger.LogError($"{error}. Task Cancelled.");
        }

        private async Task<bool> LoginEmployee(string cardString, CancellationToken token)
        {
            const string error = "Login Employee - error during login.";
            bool loginSuccessful = false;
            CurrentEmployeeId = string.Empty;
            var result = await _identification.EmployeeLogin(cardString, string.Empty, token);
            if (result.Status == MessageStatus.TimedOut || result.Status == MessageStatus.Aborted)
            {
                LogTaskCancelledError(error);
            }
            else if (result.Status == MessageStatus.Success && result.Response.ResponseCode == ServerResponseCode.UnknownCardString)
            {
                LogResponseCodeError("Login Employee - unknown card string", result.Response, CultureProviderType.Operator, DisplayRole.Main);
            }
            else if (result.Status == MessageStatus.Success && result.Response.ResponseCode == ServerResponseCode.Ok)
            {
                var role = GetEmployeeRole(result.Response.Actions.ToArray());
                CurrentEmployeeId = result.Response.EmployeeId.ToString();

                // We have already logged in, could be drop mode.
                if (_employeeLogin.IsLoggedIn &&
                    role == EmployeeRole.Unknown)
                {
                    return true;
                }

                if (role == EmployeeRole.Unknown)
                {
                    LogResponseCodeError("Login Employee - unknown employee action.", result.Response, CultureProviderType.Operator, DisplayRole.Main);
                }
                else
                {
                    IsTechnician = role == EmployeeRole.Technician;
                    ShowMenu();
                    _employeeLogin.Login(CurrentEmployeeId);
                    loginSuccessful = true;
                }

                await _notificationLift.Notify(NotificationCode.EmployeeLoggedIn, cardString);
            }
            else
            {
                LogResponseCodeError(error, result.Response, CultureProviderType.Operator, DisplayRole.Main);
            }

            return loginSuccessful;
        }

        private async Task LogoffEmployee()
        {
            CloseMenu();
            _employeeLogin.Logout(CurrentEmployeeId);
            _operatorMenu.DisableKey(OperatorMenuDisableKey);

            await _notificationLift.Notify(NotificationCode.EmployeeLoggedOut);
        }

        private async Task<bool> LoginPlayer(string cardString, CancellationToken token)
        {
            const string error = "Login Player - error during login.";
            bool loginSuccessful = false;

            var result = await _identification.PlayerTrackingLogin(cardString, token);
            if (result.Status == MessageStatus.TimedOut || result.Status == MessageStatus.Aborted)
            {
                LogTaskCancelledError(error);
            }
            else if (result.Status == MessageStatus.Success &&
                     result.Response.ResponseCode == ServerResponseCode.Ok)
            {
                _playerTracking.StartPlayerSession(result.Response.PlayerName,
                    result.Response.PlayerPoints,
                    result.Response.PromotionalInfo);
                _eventBus.Publish(new PlayerLoggedInEvent());
                loginSuccessful = true;
            }
            else
            {
                LogResponseCodeError(error, result.Response, CultureProviderType.Player, DisplayRole.VBD);
            }

            return loginSuccessful;
        }

        private async Task LogoffPlayer(CancellationToken token)
        {
            const string error = "Logoff Player - error during logoff.";

            _playerTracking.EndPlayerSession();

            try
            {
                var result = await _identification.PlayerTrackingLogoff(token);
                if (result.Status == MessageStatus.TimedOut || result.Status == MessageStatus.Aborted)
                {
                    LogTaskCancelledError(error);
                }
                else
                {
                    _eventBus.Publish(new PlayerLoggedOutEvent());
                }
            }
            catch (ServerResponseException ex)
            {
                LogResponseCodeError(error, ex, CultureProviderType.Player, DisplayRole.VBD);
            }
        }

        private bool ShowMenu()
        {
            var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>()?.Adapters?.FirstOrDefault();
            var idReaderEnabled = idReader?.Enabled ?? false;

            if (!idReader?.Connected ?? false)
            {
                return false;
            }

            if (idReaderEnabled && IsHostOnline && string.IsNullOrEmpty(CurrentEmployeeId))
            {
                return false;
            }

            if (idReaderEnabled && (!IsCardInserted || CurrentCardType != CardType.Employee))
            {
                return false;
            }

            ShowMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Identification_ValidEmpCard), DisplayRole.Main);

            if (IsKeyTurned)
            {
                if (idReaderEnabled && (!IsHostOnline || !IsTechnician))
                {
                    _eventBus.Publish(new OperatorMenuAccessRequestedEvent(false, CurrentEmployeeId));
                }
                else if (!idReaderEnabled)
                {
                    // Force operator menu access if ID reader has been disabled (but still connected) so the audit menu will still open
                    _logger.LogInfo("Operator Menu opened while ID reader is disabled");
                    _eventBus.Publish(new OperatorMenuAccessRequestedEvent(false, CurrentEmployeeId));
                }
                else
                {
                    _eventBus.Publish(new OperatorMenuAccessRequestedEvent(true, CurrentEmployeeId, true));
                }

                _operatorMenu.EnableKey(OperatorMenuDisableKey);
                return _operatorMenu.Show();
            }

            return false;
        }

        private void ShowMessage(string message, DisplayRole displayTarget, bool error = false)
        {
            var infoBarEvent = new InfoBarDisplayTransientMessageEvent(
                _infoBarOwnershipKey,
                message,
                error ? MgamConstants.PlayerMessageErrorTextColor : MgamConstants.PlayerMessageDefaultTextColor,
                MgamConstants.PlayerMessageDefaultBackgroundColor,
                InfoBarRegion.Center,
                displayTarget);
            _eventBus.Publish(infoBarEvent);
        }

        private void CloseInfoBar(DisplayRole displayTarget) => _eventBus.Publish(new InfoBarCloseEvent(displayTarget));
    }
}
