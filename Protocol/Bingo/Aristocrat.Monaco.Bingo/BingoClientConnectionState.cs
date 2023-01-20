namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using Stateless;

    public class BingoClientConnectionState : IBingoClientConnectionState, IDisposable
    {
        private const int NoMessagesTimeout = 40_000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IEnumerable<IClient> _clients;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ICommandService _commandService;
        private readonly IProgressiveCommandService _progressiveCommandService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private CancellationTokenSource _tokenSource;
        private System.Timers.Timer _timeoutTimer;

        private StateMachine<State, Trigger> _registrationState;

        private StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureReason>
            _failedRegistrationTrigger;

        private StateMachine<State, Trigger>.TriggerWithParameters<ConfigurationFailureReason>
            _failedConfigurationTrigger;

        private bool _disposed;

        public BingoClientConnectionState(
            IEventBus eventBus,
            IEnumerable<IClient> clients,
            ICommandHandlerFactory commandFactory,
            ICommandService commandService,
            IProgressiveCommandService progressiveCommandService,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisable,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _clients = clients ?? throw new ArgumentNullException(nameof(clients));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _progressiveCommandService = progressiveCommandService ?? throw new ArgumentNullException(nameof(progressiveCommandService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            CreateStateMachine();
            RegisterEventListeners();
            CreateTimeoutTimer();
        }

        public event EventHandler ClientConnected;

        public event EventHandler ClientDisconnected;

        public int NoMessagesConnectionTimeout { get; set; } = NoMessagesTimeout;

        private bool IsCrossGameProgressiveEnabled => _unitOfWorkFactory.IsCrossGameProgressiveEnabledForMainGame(_propertiesManager);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Start()
        {
            await _registrationState.FireAsync(Trigger.Initialized);
        }

        public async Task Stop()
        {
            await _registrationState.FireAsync(Trigger.Shutdown);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _registrationState.Deactivate();
                foreach(var client in _clients)
                {
                    client.Disconnected -= OnClientDisconnected;
                    client.Connected -= OnClientConnected;
                    client.MessageReceived -= OnMessageReceived;
                }
                _timeoutTimer.Elapsed -= TimeoutOccurred;
                _timeoutTimer.Dispose();
                var tokenSource = _tokenSource;
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }

                _tokenSource = null;
            }

            _disposed = true;
        }

        private static State HandleConfigurationFailure(ConfigurationFailureReason reason)
        {
            switch (reason)
            {
                case ConfigurationFailureReason.Rejected:
                case ConfigurationFailureReason.InvalidGameConfiguration:
                    return State.InvalidConfiguration;
                case ConfigurationFailureReason.ConfigurationMismatch:
                    return State.ConfigurationMismatch;
                default:
                    return State.Connecting;
            }
        }

        private static State HandleRegistrationFailure(RegistrationFailureReason reason)
        {
            switch (reason)
            {
                case RegistrationFailureReason.Rejected:
                case RegistrationFailureReason.InvalidToken:
                    return State.InvalidRegistration;
                default:
                    return State.Connecting;
            }
        }

        private void ResetConnectionToken()
        {
            _tokenSource?.Cancel(false);
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private async Task OnRegistering()
        {
            try
            {
                await _commandFactory.Execute(new RegistrationCommand(), _tokenSource.Token);

                if (IsCrossGameProgressiveEnabled)
                {
                    await _commandFactory.Execute(new ProgressiveRegistrationCommand(), _tokenSource.Token);
                }

                await _registrationState.FireAsync(Trigger.Registered);
            }
            catch (RegistrationException exception)
            {
                Logger.Error("Registration failed", exception);
                await _registrationState.FireAsync(_failedRegistrationTrigger, exception.Reason);
            }
        }

        private void CreateStateMachine()
        {
            _registrationState = new StateMachine<State, Trigger>(State.Idle);

            _failedRegistrationTrigger =
                _registrationState.SetTriggerParameters<RegistrationFailureReason>(Trigger.RegistrationFailed);
            _failedConfigurationTrigger =
                _registrationState.SetTriggerParameters<ConfigurationFailureReason>(Trigger.ConfiguringFailed);
            _registrationState.Configure(State.Idle)
                .OnEntryAsync(OnIdle)
                .Permit(Trigger.Initialized, State.Disconnected);
            _registrationState.Configure(State.Running)
                .Permit(Trigger.Shutdown, State.Idle);
            _registrationState.Configure(State.Disconnected)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnDisconnected)
                .Permit(Trigger.Connecting, State.Connecting);
            _registrationState.Configure(State.Connecting)
                .SubstateOf(State.Running)
                .OnEntry(OnConnecting)
                .PermitReentry(Trigger.Connecting)
                .PermitReentry(Trigger.Reconfigure)
                .Permit(Trigger.Connected, State.Registering);
            _registrationState.Configure(State.Registering)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnRegistering)
                .OnExit(OnRegisteringExit)
                .PermitDynamic(_failedRegistrationTrigger, HandleRegistrationFailure)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Registered, State.Configuring);
            _registrationState.Configure(State.InvalidRegistration)
                .SubstateOf(State.Running)
                .OnEntry(OnRegisteringFailed)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            _registrationState.Configure(State.Configuring)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnConfiguring)
                .OnExit(OnConfiguringExit)
                .PermitDynamic(_failedConfigurationTrigger, HandleConfigurationFailure)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Configured, State.Connected);
            _registrationState.Configure(State.InvalidConfiguration)
                .SubstateOf(State.Running)
                .OnEntry(OnInvalidConfiguration)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            _registrationState.Configure(State.ConfigurationMismatch)
                .SubstateOf(State.Running)
                .OnEntry(OnConfigurationMismatch)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            _registrationState.Configure(State.Connected)
                .SubstateOf(State.Running)
                .OnEntry(OnConnected)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected);

            _registrationState.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error(
                        $"Invalid Registration State Transition. State : {state} Trigger : {trigger}");
                });

            _registrationState.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
        }

        private async Task OnIdle()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            foreach (var client in _clients)
            {
                await client.Stop();
            }
        }

        private void OnRegisteringExit(StateMachine<State, Trigger>.Transition t)
        {
            if (t.Trigger != Trigger.Registered)
            {
                return;
            }

            _systemDisable.Enable(BingoConstants.BingoHostRegistrationFailedKey);
        }

        private void OnConfiguringExit(StateMachine<State, Trigger>.Transition t)
        {
            if (t.Trigger != Trigger.Configured)
            {
                return;
            }

            _systemDisable.Enable(BingoConstants.BingoHostConfigurationInvalidKey);
            _systemDisable.Enable(BingoConstants.BingoHostConfigurationMismatchKey);
        }

        private void OnRegisteringFailed()
        {
            _eventBus.Publish(new RegistrationFailedEvent());
            _systemDisable.Disable(
                BingoConstants.BingoHostRegistrationFailedKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostRegistrationFailed),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostRegistrationFailedHelp));
        }

        private async Task OnConfiguring()
        {
            try
            {
                await _commandFactory.Execute(new ConfigureCommand(), _tokenSource.Token);
                await _registrationState.FireAsync(Trigger.Configured);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error("Configuration failed", exception);
                await _registrationState.FireAsync(_failedConfigurationTrigger, exception.Reason);
            }
        }

        private void OnInvalidConfiguration()
        {
            _eventBus.Publish(new InvalidConfigurationReceivedEvent());
            _systemDisable.Disable(
                BingoConstants.BingoHostConfigurationInvalidKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoInvalidConfiguration),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoInvalidConfigurationHelp));
        }

        private void OnConfigurationMismatch()
        {
            _eventBus.Publish(new ConfigurationMismatchReceivedEvent());
            _systemDisable.Disable(
                BingoConstants.BingoHostConfigurationMismatchKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.BingoConfigurationChangeNVRamClearRequired),
                true,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.BingoConfigurationChangeNVRamClearRequiredHelp));
        }

        private void OnConnected()
        {
            _eventBus.Publish(new HostConnectedEvent());
            _systemDisable.Enable(BingoConstants.BingoHostDisconnectedKey);
            ClientConnected?.Invoke(this, EventArgs.Empty);
            _commandService.HandleCommands(
                _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                _tokenSource.Token).FireAndForget();

            if (IsCrossGameProgressiveEnabled)
            {
                var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_propertiesManager);
                var gameTitleId = (int)(gameConfiguration?.GameTitleId ?? 0);
                _progressiveCommandService.HandleCommands(
                    _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                    gameTitleId,
                    _tokenSource.Token).FireAndForget();
            }

            _timeoutTimer.Start();
        }

        private void TimeoutOccurred(object sender, ElapsedEventArgs e)
        {
            _registrationState.FireAsync(Trigger.Disconnected).FireAndForget();
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            _registrationState.FireAsync(Trigger.Disconnected).FireAndForget();
        }

        private void OnClientConnected(object sender, ConnectedEventArgs e)
        {
            _registrationState.FireAsync(Trigger.Connected).FireAndForget();
        }

        private void OnMessageReceived(object sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Start();
        }

        private async Task OnDisconnected()
        {
            _eventBus.Publish(new HostDisconnectedEvent());
            _systemDisable.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostDisconnected),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHostDisconnectedHelp));

            _timeoutTimer.Stop();

            ClientDisconnected?.Invoke(this, EventArgs.Empty);
            await _registrationState.FireAsync(Trigger.Connecting);
        }

        private void OnConnecting()
        {
            ResetConnectionToken();
            ConnectClient(_tokenSource.Token).FireAndForget();
        }

        private async Task ConnectClient(CancellationToken token)
        {
            SetupFirewallRule();
            foreach (var client in _clients)
            {
                while (!await client.Start() && !token.IsCancellationRequested)
                {
                }
            }
        }

        private void SetupFirewallRule()
        {
            foreach (var client in _clients)
            {
                Firewall.AddRule(client.FirewallRuleName, (ushort)client.Configuration.Address.Port, Firewall.Direction.Out);
            }
        }

        private void RegisterEventListeners()
        {
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandleRestartingEvent,
                evt =>
                    string.Equals(ApplicationConstants.MachineId, evt.PropertyName) ||
                    string.Equals(ApplicationConstants.SerialNumber, evt.PropertyName));
            _eventBus.Subscribe<ForceReconnectionEvent>(this, HandleRestartingEvent);
            foreach (var client in _clients)
            {
                client.Connected += OnClientConnected;
                client.Disconnected += OnClientDisconnected;
                client.MessageReceived += OnMessageReceived;
            }
        }

        private async Task HandleRestartingEvent<TEvent>(TEvent evt, CancellationToken token)
        {
            SetupFirewallRule();
            await _registrationState.FireAsync(Trigger.Reconfigure);
        }

        private void CreateTimeoutTimer()
        {
            _timeoutTimer = new System.Timers.Timer(NoMessagesConnectionTimeout);
            _timeoutTimer.AutoReset = false;
            _timeoutTimer.Elapsed += TimeoutOccurred;
        }

        private enum State
        {
            Idle,
            Running,
            Disconnected,
            Connecting,
            Registering,
            InvalidRegistration,
            Configuring,
            InvalidConfiguration,
            ConfigurationMismatch,
            Connected
        }

        private enum Trigger
        {
            Initialized,
            Disconnected,
            Connecting,
            Connected,
            Registered,
            RegistrationFailed,
            Reconfigure,
            Configured,
            ConfiguringFailed,
            Shutdown
        }
    }
}