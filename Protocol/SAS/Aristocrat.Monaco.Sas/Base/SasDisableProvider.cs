namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     Wraps the use of Guid, disable states, and interaction with the SystemDisableManager.
    /// </summary>
    public class SasDisableProvider : ISasDisableProvider, IDisposable
    {
        private static readonly Guid ProgressivesNotSupportedGuid = new Guid("{6AFB8906-4F6B-449d-A50A-0FB0337AE3C7}");
        private static readonly Guid ValidationQueueFullGuid = new Guid("{581CEE73-F905-4BF5-B18B-DC044B88163F}");
        private static readonly Guid PowerUpDisabledGuidHost0 = new Guid("{F72C5936-9133-41C6-B8D1-F8F94E84D990}");
        private static readonly Guid PowerUpDisabledGuidHost1 = new Guid("{67D8BC13-F777-49F4-A59B-5F20C4084D6B}");

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly Dictionary<DisableState, DisableData> DisableDataDictionary =
            new Dictionary<DisableState, DisableData>
            {
                {
                    DisableState.DisabledByHost0, new DisableData(
                        ApplicationConstants.DisabledByHost0Key,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByHost1))
                },
                {
                    DisableState.DisabledByHost1, new DisableData(
                        ApplicationConstants.DisabledByHost1Key,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByHost2))
                },
                {
                    DisableState.Host0CommunicationsOffline, new DisableData(
                        ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                        () => Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.DisabledHost1CommunicationsOffline))
                },
                {
                    DisableState.Host1CommunicationsOffline, new DisableData(
                        ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                        () => Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.DisabledHost2CommunicationsOffline))
                },
                {
                    DisableState.MaintenanceMode, new DisableData(
                        ApplicationConstants.MaintenanceModeGuid,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledMaintenanceMode))
                },
                {
                    DisableState.ValidationIdNeeded, new DisableData(
                        ApplicationConstants.ValidationIdNeededGuid,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledValidationIdNeeded))
                },
                {
                    DisableState.ProgressivesNotSupported, new DisableData(
                        ProgressivesNotSupportedGuid,
                        () => Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.DisabledProgressivesNotSupported))
                },
                {
                    DisableState.ValidationQueueFull, new DisableData(
                        ValidationQueueFullGuid,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisbaledValidationQueueFull))
                },
                {
                    DisableState.PowerUpDisabledByHost0, new DisableData(
                        PowerUpDisabledGuidHost0,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PowerUpDisabledByHost1))
                },
                {
                    DisableState.PowerUpDisabledByHost1, new DisableData(
                        PowerUpDisabledGuidHost1,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PowerUpDisabledByHost2))
                }
            };

        private readonly IPropertiesManager _propertiesManager;
        private readonly IStorageDataProvider<SasDisableInformation> _disableDataProvider;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlayState;

        private readonly List<Guid> _disableStatesWithForceCashoutAllowed = new List<Guid>
        {
            ApplicationConstants.DisabledByHost0Key,
            ApplicationConstants.DisabledByHost1Key,
            ApplicationConstants.Host0CommunicationsOfflineDisableKey,
            ApplicationConstants.Host1CommunicationsOfflineDisableKey
        };

        private readonly object _lockObject = new object();
        private readonly IPlayerBank _playerBank;

        private DisableState _sasDisableState = DisableState.None;
        private DisableState _sasSoftErrorState = DisableState.None;

        private bool _disposed;
        private CashableLockupStrategy _cashableLockupStrategy;

        /// <summary>
        ///     Creates the SasDisableProvider Instance
        /// </summary>
        /// <param name="propertiesManager">the properties manager</param>
        /// <param name="disableDataProvider"></param>
        /// <param name="systemDisableManager">the system disable manager</param>
        /// <param name="messageDisplay">the message display</param>
        /// <param name="eventBus">the Event bus</param>
        /// <param name="gamePlayState">the Game play state</param>
        /// <param name="playerBank">The Player bank</param>
        public SasDisableProvider(
            IPropertiesManager propertiesManager,
            IStorageDataProvider<SasDisableInformation> disableDataProvider,
            ISystemDisableManager systemDisableManager,
            IMessageDisplay messageDisplay,
            IEventBus eventBus,
            IGamePlayState gamePlayState,
            IPlayerBank playerBank)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _disableDataProvider = disableDataProvider ?? throw new ArgumentNullException(nameof(disableDataProvider));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));

            Initialize();
        }

        private bool DisabledByHost => SasDisableState.IsAnyStateActive(
            DisableState.DisabledByHost0,
            DisableState.DisabledByHost1);

        private bool CommunicationOffline => SasDisableState.IsAnyStateActive(
            DisableState.Host0CommunicationsOffline,
            DisableState.Host1CommunicationsOffline);

        private bool ShouldForceCashout => _cashableLockupStrategy == CashableLockupStrategy.ForceCashout &&
                                           (DisabledByHost || CommunicationOffline) && _playerBank.Balance > 0 &&
                                           _gamePlayState.Idle && _systemDisableManager.CurrentDisableKeys.All(
                                               x => _disableStatesWithForceCashoutAllowed.Contains(x));

        private DisableState SasDisableState
        {
            get
            {
                lock (_lockObject)
                {
                    return _sasDisableState;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _sasDisableState = value;
                }
            }
        }

        private DisableState SasSoftErrorState
        {
            get
            {
                lock (_lockObject)
                {
                    return _sasSoftErrorState;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _sasSoftErrorState = value;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task Disable(SystemDisablePriority priority, params DisableState[] states)
        {
            var disableData = states.Where(
                    x => x != DisableState.None && !IsDisableStateActive(x) && DisableDataDictionary.ContainsKey(x))
                .ToList();
            if (!disableData.Any())
            {
                return;
            }

            foreach (var state in disableData)
            {
                if (IsSoftErrorStateActive(state))
                {
                    // Clear soft error if we are wanting to lockup this state now
                    await Enable(state);
                }

                CreateLockup(priority, state);
            }

            await SaveDisableState();
        }

        /// <inheritdoc />
        public async Task Disable(SystemDisablePriority priority, DisableState state, bool isLockup)
        {
            if (state == DisableState.None || SasDisableState.IsAnyStateActive(state) ||
                IsSoftErrorStateActive(state) && !isLockup)
            {
                return;
            }

            if (isLockup)
            {
                if (IsSoftErrorStateActive(state))
                {
                    // Clear soft error if we are wanting to lockup this state now
                    await Enable(state);
                }

                CreateLockup(priority, state);
                await SaveDisableState();
            }
            else
            {
                var disableData = DisableDataDictionary[state];
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        disableData.Message,
                        DisplayableMessageClassification.SoftError,
                        DisplayableMessagePriority.Normal,
                        disableData.DisableGuid));
                SasSoftErrorState |= state;
            }
        }

        /// <inheritdoc />
        public async Task Enable(params DisableState[] states)
        {
            var disableData = states.Where(
                    x => x != DisableState.None && (IsDisableStateActive(x) || IsSoftErrorStateActive(x)) &&
                         DisableDataDictionary.ContainsKey(x))
                .ToList();
            if (!disableData.Any())
            {
                return;
            }

            foreach (var state in disableData)
            {
                var data = DisableDataDictionary[state];
                if (SasSoftErrorState.IsAnyStateActive(state))
                {
                    _messageDisplay.RemoveMessage(data.DisableGuid);
                    SasSoftErrorState ^= state;
                }
                else
                {
                    Logger.Debug(
                        $"Requesting the SystemDisableManager to clear disable state: {states}, guid: {data.DisableGuid}");
                    _systemDisableManager.Enable(data.DisableGuid);
                    ClearDisableState(state);
                }
            }

            await SaveDisableState();
        }

        /// <inheritdoc />
        public bool IsDisableStateActive(DisableState checkState) => SasDisableState.IsAnyStateActive(checkState);

        /// <inheritdoc />
        public bool IsSoftErrorStateActive(DisableState checkState) => SasSoftErrorState.IsAnyStateActive(checkState);

        /// <inheritdoc />
        public async Task OnSasReconfigured()
        {
            var states = (DisableState[])Enum.GetValues(typeof(DisableState));
            var enablingStates = states.Where(
                state =>
                    state != DisableState.None &&
                    (IsDisableStateActive(state) || IsSoftErrorStateActive(state))).ToArray();
            Logger.Debug($"Clearing disabled states {string.Join(", ", enablingStates)} due to SAS being reconfigured");
            await Enable(enablingStates);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing">If we are disposing the object or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Initialize()
        {
            GetSavedDisableState();
            ClearVolatileDisableStates();

            var persistedStates = SasDisableState;
            var disabledByHost = DisabledByHost;
            SasDisableState = DisableState.None;

            var states = (DisableState[])Enum.GetValues(typeof(DisableState));

            foreach (var state in states)
            {
                if (persistedStates.IsAnyStateActive(state))
                {
                    Disable(SystemDisablePriority.Normal, state).WaitForCompletion();
                }
            }

            _propertiesManager.SetProperty(SasProperties.SasShutdownCommandReceivedKey, disabledByHost);

            _cashableLockupStrategy = _propertiesManager.GetValue(
                GamingConstants.LockupBehavior,
                CashableLockupStrategy.NotAllowed);
            if (_cashableLockupStrategy != CashableLockupStrategy.ForceCashout)
            {
                return;
            }

            _eventBus.Subscribe<SystemDisableAddedEvent>(this, _ => CheckForceCashOut());
            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, _ => CheckForceCashOut());
            _eventBus.Subscribe<GamePlayStateChangedEvent>(this, _ => CheckForceCashOut());
        }

        private void CreateLockup(SystemDisablePriority priority, DisableState state)
        {
            var data = DisableDataDictionary[state];
            Logger.Debug(
                $"Requesting the disable with state={state} guid={data.DisableGuid} reason={data.Message} priority={priority}");

            _systemDisableManager.Disable(data.DisableGuid, priority, data.Message);
            SetDisableState(state);
            CheckForceCashOut();
        }

        private void CheckForceCashOut()
        {
            lock (_lockObject)
            {
                if (ShouldForceCashout)
                {
                    _playerBank.CashOut(true);
                }
            }
        }

        private void SetDisableState(DisableState state)
        {
            if (IsDisableStateActive(state))
            {
                return;
            }

            Logger.Debug($"Setting disable state: {state}");
            SasDisableState |= state;
        }

        private void ClearDisableState(DisableState state)
        {
            if (!IsDisableStateActive(state))
            {
                return;
            }

            Logger.Debug($"Clearing disable state: {state}");
            SasDisableState ^= state;
        }

        private void ClearVolatileDisableStates()
        {
            // Don't disable on reboot for maintenance mode.
            ClearDisableState(DisableState.MaintenanceMode);
            SaveDisableState().WaitForCompletion();
        }

        private async Task SaveDisableState()
        {
            var disableInformation = _disableDataProvider.GetData();
            disableInformation.DisableStates = SasDisableState;
            await _disableDataProvider.Save(disableInformation);
        }

        private void GetSavedDisableState()
        {
            var information = _disableDataProvider.GetData();
            SasDisableState = information?.DisableStates ?? DisableState.None;
        }

        private class DisableData
        {
            /// <summary>
            ///     Initializes a new instance of the DisableData class.
            /// </summary>
            /// <param name="guid">The guid used when disabling/enabling</param>
            /// <param name="message">The message to display on screen when disabled</param>
            public DisableData(Guid guid, Func<string> message)
            {
                DisableGuid = guid;
                Message = message;
            }

            public Guid DisableGuid { get; }

            public Func<string> Message { get; }
        }
    }
}