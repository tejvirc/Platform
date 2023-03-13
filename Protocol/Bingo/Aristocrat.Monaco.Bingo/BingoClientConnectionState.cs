namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Monaco.Common;
    using Stateless;
    using Timer = System.Threading.Timer;

    public sealed class BingoClientConnectionState : IBingoClientConnectionState, IDisposable
    {
        private static readonly TimeSpan NoMessagesTimeout = TimeSpan.FromMilliseconds(40_000);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IClient _client;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IClientConfigurationProvider _configurationProvider;
        private readonly Timer _timeoutTimer;

        private CancellationTokenSource _tokenSource;

        private StateMachine<State, Trigger> _registrationState;

        private StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureReason>
            _failedRegistrationTrigger;

        private StateMachine<State, Trigger>.TriggerWithParameters<ConfigurationFailureReason>
            _failedConfigurationTrigger;

        private bool _disposed;

        public BingoClientConnectionState(
            IEventBus eventBus,
            IClient client,
            ICommandHandlerFactory commandFactory,
            ISystemDisableManager systemDisable,
            IClientConfigurationProvider configurationProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));

            CreateStateMachine();
            RegisterEventListeners();
            _timeoutTimer = new Timer(TimeoutOccurred);
        }

        public event EventHandler ClientConnected;

        public event EventHandler ClientDisconnected;

        public TimeSpan NoMessagesConnectionTimeout { get; set; } = NoMessagesTimeout;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _registrationState.Deactivate();
            _client.Disconnected -= OnClientDisconnected;
            _client.Connected -= OnClientConnected;
            _client.MessageReceived -= OnMessageReceived;
            _timeoutTimer.Dispose();
            var tokenSource = _tokenSource;
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }

            _tokenSource = null;

            _disposed = true;
        }

        public async Task Start()
        {
            await _registrationState.FireAsync(Trigger.Initialized);
        }

        public async Task Stop()
        {
            await _registrationState.FireAsync(Trigger.Shutdown);
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
            await _client.Stop();
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
            _commandFactory.Execute(new ClientConnectedCommand(), _tokenSource.Token).FireAndForget();
        }

        private void TimeoutOccurred(object _)
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
            if (_registrationState.State is not State.Connected)
            {
                return;
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
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

            _timeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

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
            while (!await _client.Start() && !token.IsCancellationRequested)
            {
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
        }

        private void SetupFirewallRule()
        {
            const string firewallRuleName = "Platform.Bingo.Server";
            using var configuration = _configurationProvider.CreateConfiguration();
            Firewall.AddRule(firewallRuleName, (ushort)configuration.Address.Port, Firewall.Direction.Out);
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
            _client.Connected += OnClientConnected;
            _client.Disconnected += OnClientDisconnected;
            _client.MessageReceived += OnMessageReceived;
        }

        private async Task HandleRestartingEvent<TEvent>(TEvent evt, CancellationToken token)
        {
            SetupFirewallRule();
            await _registrationState.FireAsync(Trigger.Reconfigure);
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