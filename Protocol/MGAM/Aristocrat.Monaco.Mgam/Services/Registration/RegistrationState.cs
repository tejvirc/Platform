namespace Aristocrat.Monaco.Mgam.Services.Registration
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Registration;
    using Attributes;
    using Commands;
    using Common;
    using Common.Data.Models;
    using Common.Events;
    using GamePlay;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using Lockup;
    using Monaco.Common;
    using Notification;
    using Protocol.Common.Storage.Entity;
    using Stateless;

    /// <summary>
    ///     Registers the EGM with a VLT Service.
    /// </summary>
    public sealed class RegistrationState : IRegistrationState, IDisposable
    {
        private const int ConnectionTimeout = 300;

        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IAttributeManager _attributes;
        private readonly ISystemDisableManager _disableManager;
        private readonly IEgm _egm;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IProgressiveController _progressiveController;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGameProvider _gameProvider;

        private readonly ConcurrentQueue<IPEndPoint> _endPoints = new ConcurrentQueue<IPEndPoint>();

        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();

        private IPEndPoint _currentEndPoint;

        private State _state;
        private StateMachine<State, Trigger> _stateMachine;
        private StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureBehavior> _failedTrigger;
        private bool _initialized;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationState"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategorty}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="attributes"><see cref="IAttributeManager"/>.</param>
        /// <param name="disableManager"><see cref="ISystemDisableManager"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="notificationLift"><see cref="INotificationLift"/>.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/>.</param>
        /// <param name="progressiveController"><see cref="IProgressiveController"/>.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        /// <param name="gameProvider"><see cref="IGameProvider"/>.</param>
        public RegistrationState(
            ILogger<RegistrationState> logger,
            IEventBus eventBus,
            IPropertiesManager properties,
            IAttributeManager attributes,
            ISystemDisableManager disableManager,
            IEgm egm,
            ILockup lockup,
            INotificationLift notificationLift,
            ICommandHandlerFactory commandFactory,
            IProgressiveController progressiveController,
            IUnitOfWorkFactory unitOfWorkFactory,
            IGameProvider gameProvider)
        {
            _logger = logger;
            _eventBus = eventBus;
            _properties = properties;
            _attributes = attributes;
            _disableManager = disableManager;
            _egm = egm;
            _lockup = lockup;
            _notificationLift = notificationLift;
            _commandFactory = commandFactory;
            _progressiveController = progressiveController;
            _unitOfWorkFactory = unitOfWorkFactory;
            _gameProvider = gameProvider;

            Configure(State.Idle);

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~RegistrationState()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);

                if (_shutdown != null)
                {
                    if (_shutdown.IsCancellationRequested)
                    {
                        _shutdown.Cancel();
                    }

                    _shutdown.Dispose();
                }
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ProtocolsInitializedEvent>(this, Handle);
            _eventBus.Subscribe<HostOfflineEvent>(this, Handle);
            _eventBus.Subscribe<ServiceFoundEvent>(this, Handle, _ => _state == State.Wait);
            _eventBus.Subscribe<LockupResolvedEvent>(this, Handle);
            _eventBus.Subscribe<ForceDisconnectEvent>(this, Handle);
            _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
            _eventBus.Subscribe<GameConfiguringEvent>(this, Handle);
            _eventBus.Subscribe<ExitRequestedEvent>(this, _ => _shutdown.Cancel());
        }

        private void Configure(State initialState)
        {
            _state = initialState;

            _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _failedTrigger = _stateMachine.SetTriggerParameters<RegistrationFailureBehavior>(Trigger.Failed);

            _stateMachine.Configure(State.Idle)
                .OnEntry(() => { })
                .PermitIf(
                    Trigger.Initialized,
                    State.Locate,
                    () => _egm.State < EgmState.Stopping && _gameProvider.GetConfiguredGames().Any())
                .PermitIf(
                    Trigger.Disconnected,
                    State.Locate,
                    () => _egm.State < EgmState.Stopping && _gameProvider.GetConfiguredGames().Any() && _initialized);

            _stateMachine.Configure(State.Locate)
                .OnEntryAsync(async () => await OnLocate())
                .Permit(Trigger.Locating, State.Wait)
                .Permit(Trigger.ServiceFound, State.Connect);

            _stateMachine.Configure(State.Wait)
                .OnEntry(() => { })
                .Permit(Trigger.ServiceFound, State.Connect);

            _stateMachine.Configure(State.Connect)
                .OnEntryAsync(async () => await OnConnect())
                .Permit(Trigger.Connected, State.RegisterInstance)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterInstance)
                .OnEntryAsync(async () => await OnRegisterInstance())
                .PermitDynamic(
                    Trigger.InstanceRegistered,
                    () => _properties.GetValue(PropertyNames.KnownRegistration, false)
                            ? State.UpdateAttributes
                        : State.RegisterAttributes)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterAttributes)
                .OnEntryAsync(async () => await OnRegisterAttributes())
                .Permit(Trigger.AttributesRegistered, State.UpdateAttributes)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.UpdateAttributes)
                .OnEntryAsync(async () => await OnUpdateAttributes())
                .PermitDynamic(
                    Trigger.AttributesUpdated,
                    () => _properties.GetValue(PropertyNames.KnownRegistration, false)
                        ? State.RegisterGames
                        : State.RegisterCommands)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterCommands)
                .OnEntryAsync(async () => await OnRegisterCommands())
                .Permit(Trigger.CommandsRegistered, State.RegisterNotifications)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterNotifications)
                .OnEntryAsync(async () => await OnRegisterNotifications())
                .Permit(Trigger.NotificationsRegistered, State.RegisterActions)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterActions)
                .OnEntryAsync(async () => await OnRegisterActions())
                .Permit(Trigger.ActionsRegistered, State.RegisterGames)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterGames)
                .OnEntryAsync(async () => await OnRegisterGames())
                .Permit(Trigger.GamesRegistered, State.RegisterDenominations)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterDenominations)
                .OnEntryAsync(async () => await OnRegisterDenominations())
                .Permit(Trigger.DenominationsRegistered, State.RegisterProgressives)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.RegisterProgressives)
                .OnEntryAsync(async () => await OnRegisterProgressives())
                .Permit(Trigger.ProgressivesRegistered, State.Complete)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.Complete)
                .OnEntryAsync(async () => await OnComplete())
                .Permit(Trigger.Completed, State.Idle)
                .PermitDynamic(_failedTrigger, SelectState);

            _stateMachine.Configure(State.Relocate)
                .OnEntryAsync(async () => await OnRelocate())
                .Permit(Trigger.Locate, State.Locate);

            _stateMachine.Configure(State.Lock)
                .OnEntryAsync(async () => await OnLock())
                .PermitDynamic(Trigger.Unlocked, () => _egm.ActiveInstance != null ? State.RegisterInstance : State.Locate);

            _stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    _logger.LogError(
                        $"Invalid Registration State Transition. State : {state} Trigger : {trigger}");
                });

            _stateMachine.OnTransitioned(
                transition =>
                {
                    _logger.LogDebug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
        }

        private async Task OnLocate()
        {
            if (_shutdown.IsCancellationRequested)
            {
                return;
            }

            if (_endPoints.Any())
            {
                await _stateMachine.FireAsync(Trigger.ServiceFound);
                return;
            }

            _eventBus.Publish(new RequestServiceLocationEvent());

            await _stateMachine.FireAsync(Trigger.Locating);
        }

        private async Task OnConnect()
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeout)))
            {
                using (var lts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _shutdown.Token))
                {
                    if (_endPoints.TryDequeue(out var endPoint) && await Connect(endPoint, lts.Token))
                    {
                        _currentEndPoint = endPoint;

                        await _stateMachine.FireAsync(Trigger.Connected);
                    }
                    else
                    {
                        _endPoints.TryDequeue(out _);

                        _logger.LogDebug($"Discarding service address {endPoint} due to connection attempt failures");

                        await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Relocate);
                    }
                }
            }
        }

        private async Task OnRegisterInstance()
        {
            try
            {
                if (_currentEndPoint == null)
                {
                    throw new InvalidOperationException("Current end point is not set");
                }

                await _commandFactory.Execute(new Commands.RegisterInstance { EndPoint = _currentEndPoint });

                _eventBus.Publish(new InstanceRegisteredEvent());

                await _stateMachine.FireAsync(Trigger.InstanceRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering instance");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterAttributes()
        {
            try
            {
                await _commandFactory.Execute(new RegisterAttributes());

                await _stateMachine.FireAsync(Trigger.AttributesRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering attributes");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }
        private async Task OnUpdateAttributes()
        {
            try
            {
                await _attributes.Update();

                await _stateMachine.FireAsync(Trigger.AttributesUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal retrieving registering attributes");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterCommands()
        {
            try
            {
                await _commandFactory.Execute(new RegisterCommands());

                await _stateMachine.FireAsync(Trigger.CommandsRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering commands");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterNotifications()
        {
            try
            {
                await _commandFactory.Execute(new RegisterNotifications());

                await _stateMachine.FireAsync(Trigger.NotificationsRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering notifications");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterActions()
        {
            try
            {
                await _commandFactory.Execute(new RegisterActions());

                await _stateMachine.FireAsync(Trigger.ActionsRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering actions");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterGames()
        {
            try
            {
                await _commandFactory.Execute(new RegisterGames());

                await _stateMachine.FireAsync(Trigger.GamesRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering games");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterDenominations()
        {
            try
            {
                await _commandFactory.Execute(new RegisterDenominations());

                await _stateMachine.FireAsync(Trigger.DenominationsRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering denominations");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRegisterProgressives()
        {
            try
            {
                await _commandFactory.Execute(new RegisterProgressives());

                await _stateMachine.FireAsync(Trigger.ProgressivesRegistered);
            }
            catch (RegistrationException ex)
            {
                _logger.LogError(ex, ex.Message);
                await _stateMachine.FireAsync(_failedTrigger, ex.FailureBehavior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error registering progressives");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnComplete()
        {
            try
            {
                await _notificationLift.Continue();

                await ReadyToPlay();

                _eventBus.Publish(new ReadyToPlayEvent());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error requesting ready for play");
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private async Task OnRelocate()
        {
            await Disconnect(CancellationToken.None);

            await _stateMachine.FireAsync(Trigger.Locate);
        }

        private async Task OnLock()
        {
            if (_lockup.IsEmployeeLoggedIn)
            {
                await _stateMachine.FireAsync(Trigger.Unlocked);
                return;
            }

            _lockup.LockupForEmployeeCard();

            await _notificationLift.Notify(NotificationCode.LockedRegistrationFailed);

            if (!_disableManager.CurrentDisableKeys.Contains(MgamConstants.RegistrationFailedDisabledKey))
            {
                _disableManager.Disable(
                        MgamConstants.RegistrationFailedDisabledKey,
                        SystemDisablePriority.Immediate,
                        () =>
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RegistrationFailed));
            }
        }

        private async Task<bool> Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            try
            {
                await _egm.Connect(endPoint, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connect to VLT service failure");
                return false;
            }
        }

        private async Task Disconnect(CancellationToken cancellationToken)
        {
            try
            {
                await _egm.Disconnect(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Disconnect from VLT service failure");
            }
        }

        private async Task Handle(ProtocolsInitializedEvent evt, CancellationToken cancellationToken)
        {
            _initialized = true;

            if (_stateMachine.CanFire(Trigger.Initialized))
            {
                _progressiveController.Configure();

                await _stateMachine.FireAsync(Trigger.Initialized);
            }
        }

        private async Task Handle(HostOfflineEvent evt, CancellationToken cancellationToken)
        {
            _currentEndPoint = null;
            _egm.ClearActiveInstance();

            if (_stateMachine.CanFire(Trigger.Disconnected))
            {
                await _stateMachine.FireAsync(Trigger.Disconnected);
            }
        }

        private async Task Handle(ForceDisconnectEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogWarn($"Forcing disconnection, Reason: {evt.Reason}.");

            if (_stateMachine.CanFire(Trigger.Disconnected))
            {
                await Disconnect(cancellationToken);

                await _stateMachine.FireAsync(Trigger.Disconnected);
            }
        }

        private void Handle(ServiceFoundEvent evt)
        {
            if (!_endPoints.Any())
            {
                _endPoints.Enqueue(evt.EndPoint);
            }

            if (_stateMachine.CanFire(Trigger.ServiceFound))
            {
                Task.Run(async () => await _stateMachine.FireAsync(Trigger.ServiceFound))
                    .FireAndForget(ex => _logger.LogError(ex, $"ServiceFound trigger failed, {evt.EndPoint}"));
            }
        }

        private void Handle(PropertyChangedEvent evt)
        {
            switch (evt.PropertyName)
            {
                case ApplicationConstants.CalculatedDeviceName:
                    var deviceName = _properties.GetValue(ApplicationConstants.CalculatedDeviceName, string.Empty);

                    using (var unitOfWork = _unitOfWorkFactory.Create())
                    {
                        var device = unitOfWork.Repository<Device>().Queryable().First();
                        device.Name = deviceName;
                        unitOfWork.SaveChanges();
                    }

                    _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.DeviceChanged));
                    break;
            }
        }

        private async Task Handle(LockupResolvedEvent evt, CancellationToken cancellationToken)
        {
            _disableManager.Enable(MgamConstants.RegistrationFailedDisabledKey);

            if (_stateMachine.CanFire(Trigger.Unlocked))
            {
                await _stateMachine.FireAsync(Trigger.Unlocked);
            }
        }

        private void Handle(GameConfiguringEvent evt)
        {
            // Disable system during game configuration
            _disableManager.Disable(
                    MgamConstants.ConfiguringGamesGuid,
                    SystemDisablePriority.Immediate,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfiguringGames));
        }

        private async Task ReadyToPlay()
        {
            var registration = _egm.GetService<IRegistration>();

            var result = await registration.ReadyToPlay();

            if (result.Status == MessageStatus.Success && result.Response.ResponseCode == ServerResponseCode.Ok)
            {
                _logger.LogInfo("Ready for play acknowledged.  Waiting for 'Play' Command");

                foreach (var game in _gameProvider.GetConfiguredGames())
                {
                    _gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);
                }

                await _stateMachine.FireAsync(Trigger.Completed);
            }
            else
            {
                await _stateMachine.FireAsync(_failedTrigger, RegistrationFailureBehavior.Lock);
            }
        }

        private State SelectState(RegistrationFailureBehavior behavior)
        {
            switch (behavior)
            {
                case RegistrationFailureBehavior.Relocate:
                    return State.Relocate;
                case RegistrationFailureBehavior.Lock:
                    return State.Lock;
                default:
                    throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
            }
        }

        private enum State
        {
            Idle,

            Locate,

            Wait,

            Connect,

            RegisterInstance,

            RegisterAttributes,

            UpdateAttributes,

            RegisterCommands,

            RegisterNotifications,

            RegisterActions,

            RegisterGames,

            RegisterDenominations,

            RegisterProgressives,

            Complete,

            Lock,

            Relocate
        }

        private enum Trigger
        {
            Initialized,

            Locating,

            Locate,

            ServiceFound,

            Connected,

            Disconnected,

            InstanceRegistered,

            AttributesRegistered,

            AttributesUpdated,

            CommandsRegistered,

            NotificationsRegistered,

            ActionsRegistered,

            GamesRegistered,

            DenominationsRegistered,

            ProgressivesRegistered,

            Completed,

            Failed,

            Unlocked
        }
    }
}
