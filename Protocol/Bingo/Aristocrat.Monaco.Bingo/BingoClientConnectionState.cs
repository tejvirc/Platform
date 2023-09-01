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
    using ServerApiGateway;
    using Stateless;
    using Timer = System.Threading.Timer;

    public sealed class BingoClientConnectionState : IBingoClientConnectionState, IDisposable
    {
        private static readonly TimeSpan NoMessagesTimeout = TimeSpan.FromMilliseconds(40_000);
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IEventBus _eventBus;
        private readonly IClient<ClientApi.ClientApiClient> _bingoClient;
        private readonly ISystemDisableManager _systemDisable;
        private CancellationTokenSource _tokenSource;
        private StateMachine<State, Trigger> _registrationState;
        private StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureReason> _failedRegistrationTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<ConfigurationFailureReason> _failedConfigurationTrigger;
        private readonly IClientConfigurationProvider _configurationProvider;
        private Timer _timeoutTimer;
        private bool _disposed;

        public BingoClientConnectionState(
            IEventBus eventBus,
            IClient<ClientApi.ClientApiClient> bingoClient,
            IClientConfigurationProvider configurationProvider,
            ISystemDisableManager systemDisable,
            ICommandHandlerFactory commandFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bingoClient = bingoClient ?? throw new ArgumentNullException(nameof(bingoClient));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _configurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));

            Initialize();

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
            _bingoClient.Disconnected -= OnClientDisconnected;
            _bingoClient.Connected -= OnClientConnected;
            _bingoClient.MessageReceived -= OnMessageReceived;

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
            await _registrationState.FireAsync(Trigger.Initialized).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _registrationState.FireAsync(Trigger.Shutdown).ConfigureAwait(false);
        }

        private void Initialize()
        {
            CreateStateMachine();
            RegisterEventListeners();

            _bingoClient.Stop().ConfigureAwait(false);
            _timeoutTimer = new Timer(TimeoutOccurred);
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
            await _registrationState.FireAsync(Trigger.Connecting).ConfigureAwait(false);
        }

        private async Task OnRegistering()
        {
            try
            {
                await _commandFactory.Execute(new RegistrationCommand(), _tokenSource.Token).ConfigureAwait(false);
                await _registrationState.FireAsync(Trigger.Registered).ConfigureAwait(false);
            }
            catch (RegistrationException exception)
            {
                _logger.Error("Bingo client registration failed", exception);
                await _registrationState.FireAsync(_failedRegistrationTrigger, exception.Reason).ConfigureAwait(false);
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
                await _commandFactory.Execute(new ConfigureCommand(), _tokenSource.Token).ConfigureAwait(false);
                await _registrationState.FireAsync(Trigger.Configured).ConfigureAwait(false);
            }
            catch (ConfigurationException exception)
            {
                _logger.Error("Bingo client configuration failed", exception);
                await _registrationState.FireAsync(_failedConfigurationTrigger, exception.Reason).ConfigureAwait(false);
            }
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

        private async Task OnIdle()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
            await _bingoClient.Stop().ConfigureAwait(false);
        }

        private async Task ConnectClient(CancellationToken token)
        {
            SetupFirewallRule();
            while (!await _bingoClient.Start().ConfigureAwait(false) && !token.IsCancellationRequested)
            {
                _logger.Info($"Client failed to connect retrying.  IsCancelled={token.IsCancellationRequested}");
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
        }

        private void SetupFirewallRule()
        {
            using var configuration = _configurationProvider.CreateConfiguration();
            Firewall.AddRule(_bingoClient.FirewallRuleName, (ushort)configuration.Address.Port, Firewall.Direction.Out);
        }

        private void RegisterEventListeners()
        {
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandleRestartingEvent,
                evt =>
                    string.Equals(ApplicationConstants.MachineId, evt.PropertyName, StringComparison.Ordinal) ||
                    string.Equals(ApplicationConstants.SerialNumber, evt.PropertyName, StringComparison.Ordinal));
            _eventBus.Subscribe<ForceReconnectionEvent>(this, HandleRestartingEvent);
            _bingoClient.Connected += OnClientConnected;
            _bingoClient.Disconnected += OnClientDisconnected;
            _bingoClient.MessageReceived += OnMessageReceived;
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
                    _logger.Error(
                        $"{this.GetType().Name} Invalid Registration State Transition. State : {state} Trigger : {trigger}");
                });

            _registrationState.OnTransitioned(
                transition =>
                {
                    _logger.Debug(
                        $"{this.GetType().Name} Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
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

        private void OnConnecting()
        {
            ResetConnectionToken();
            ConnectClient(_tokenSource.Token).FireAndForget();
        }

        private async Task HandleRestartingEvent<TEvent>(TEvent evt, CancellationToken token)
        {
            SetupFirewallRule();
            await _registrationState.FireAsync(Trigger.Reconfigure).ConfigureAwait(false);
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
