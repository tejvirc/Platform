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
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using ServerApiGateway;
    using Stateless;
    using Timer = System.Threading.Timer;

    public sealed class ProgressiveClientConnectionState : IProgressiveClientConnectionState, IDisposable
    {
        private static readonly TimeSpan NoMessagesTimeout = TimeSpan.FromMilliseconds(40_000);
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IPropertiesManager _propertiesManager;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IProgressiveCommandHandlerFactory _progressiveCommandFactory;
        private readonly IProgressiveCommandService _progressiveCommandService;
        private readonly IGameProvider _gameProvider;
        private readonly IEventBus _eventBus;
        private readonly IClient<ProgressiveApi.ProgressiveApiClient> _progressiveClient;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IClientConfigurationProvider _configurationProvider;
        private CancellationTokenSource _tokenSource;
        private StateMachine<State, Trigger> _registrationState;
        private StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureReason> _failedRegistrationTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<ConfigurationFailureReason> _failedConfigurationTrigger;
        private bool _readyToRegister;
        private Timer _timeoutTimer;
        private bool _disposed;

        public ProgressiveClientConnectionState(
            IEventBus eventBus,
            IClient<ProgressiveApi.ProgressiveApiClient> progressiveClient,
            IClientConfigurationProvider configurationProvider,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisable,
            IUnitOfWorkFactory unitOfWorkFactory,
            IProgressiveCommandHandlerFactory progressiveCommandFactory,
            IProgressiveCommandService progressiveCommandService,
            IGameProvider gameProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _progressiveClient = progressiveClient ?? throw new ArgumentNullException(nameof(progressiveClient));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _progressiveCommandFactory = progressiveCommandFactory ?? throw new ArgumentNullException(nameof(progressiveCommandFactory));
            _progressiveCommandService = progressiveCommandService ?? throw new ArgumentNullException(nameof(progressiveCommandService));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));

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
            _progressiveClient.Disconnected -= OnClientDisconnected;
            _progressiveClient.Connected -= OnClientConnected;
            _progressiveClient.MessageReceived -= OnMessageReceived;

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
            _timeoutTimer = new Timer(TimeoutOccurred);
        }

        private async Task OnRegistering()
        {
            // Must wait for bingo server configuration data before attempting to register the progressive host
            if (!_readyToRegister)
            {
                await _registrationState.FireAsync(Trigger.WaitToRegister).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    await _progressiveCommandFactory.Execute(new ProgressiveRegistrationCommand(), _tokenSource.Token)
                        .ConfigureAwait(false);
                    await _registrationState.FireAsync(Trigger.Registered).ConfigureAwait(false);
                }
                catch (RegistrationException exception)
                {
                    _logger.Error("Progressive client registration failed", exception);
                    await _registrationState.FireAsync(_failedRegistrationTrigger, exception.Reason)
                        .ConfigureAwait(false);
                }
            }
        }

        private void OnRegisteringExit(StateMachine<State, Trigger>.Transition t)
        {
            if (t.Trigger != Trigger.Registered)
            {
                return;
            }

            _systemDisable.Enable(BingoConstants.ProgressiveHostRegistrationFailedKey);
        }

        private void OnRegisteringFailed()
        {
            _eventBus.Publish(new RegistrationFailedEvent());
            _systemDisable.Disable(
                BingoConstants.ProgressiveHostRegistrationFailedKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostRegistrationFailed),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostRegistrationFailedHelp));
        }

        private async Task OnConfiguring()
        {
            try
            {
                await _registrationState.FireAsync(Trigger.Configured).ConfigureAwait(false);
            }
            catch (ConfigurationException exception)
            {
                _logger.Error("Configuration failed", exception);
                await _registrationState.FireAsync(_failedConfigurationTrigger, exception.Reason).ConfigureAwait(false);
            }
        }

        private static void OnConfiguringExit(StateMachine<State, Trigger>.Transition t)
        {
            // Progressive client does not require configuration
        }

        private void OnConnected()
        {
            _eventBus.Publish(new ProgressiveHostOnlineEvent());
            _systemDisable.Enable(BingoConstants.ProgressiveHostOfflineKey);
            ClientConnected?.Invoke(this, EventArgs.Empty);

            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);
            var gameTitleId = (int)(gameConfiguration?.GameTitleId ?? 0);
            _progressiveCommandService.HandleCommands(
                _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                gameTitleId,
                _tokenSource.Token).FireAndForget();
        }

        private async Task OnDisconnected()
        {
            _eventBus.Publish(new ProgressiveHostOfflineEvent());
            _systemDisable.Disable(
                BingoConstants.ProgressiveHostOfflineKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostDisconnected),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveHostDisconnectedHelp));

            _timeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            ClientDisconnected?.Invoke(this, EventArgs.Empty);
            await _registrationState.FireAsync(Trigger.Connecting).ConfigureAwait(false);
        }

        private async Task OnIdle()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            await _progressiveClient.Stop().ConfigureAwait(false);
        }

        private async Task ConnectClient(CancellationToken token)
        {
            SetupFirewallRule();
            while (!await _progressiveClient.Start().ConfigureAwait(false) && !token.IsCancellationRequested)
            {
                _logger.Info($"Client failed to connect retrying.  IsCancelled={token.IsCancellationRequested}");
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
        }

        private void SetupFirewallRule()
        {
            using var configuration = _configurationProvider.CreateConfiguration();
            Firewall.AddRule(
                _progressiveClient.FirewallRuleName,
                (ushort)configuration.Address.Port,
                Firewall.Direction.Out);
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
            _eventBus.Subscribe<ServerConfigurationCompletedEvent>(this, HandleConfigurationCompleteEvent);

            _progressiveClient.Connected += OnClientConnected;
            _progressiveClient.Disconnected += OnClientDisconnected;
            _progressiveClient.MessageReceived += OnMessageReceived;
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
                .Permit(Trigger.Registered, State.Configuring)
                .Permit(Trigger.WaitToRegister, State.WaitingToRegister);
            _registrationState.Configure(State.WaitingToRegister)
                .SubstateOf(State.Running)
                .Permit(Trigger.ReadyToRegister, State.Registering);
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
                        $"{GetType().Name} Invalid Registration State Transition. State : {state} Trigger : {trigger}");
                });

            _registrationState.OnTransitioned(
                transition =>
                {
                    _logger.Debug(
                        $"{GetType().Name} Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
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

        private async Task HandleConfigurationCompleteEvent<TEvent>(TEvent evt, CancellationToken token)
        {
            _readyToRegister = true;
            await _registrationState.FireAsync(Trigger.ReadyToRegister).ConfigureAwait(false);
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
            Connected,
            WaitingToRegister
        }

        private enum Trigger
        {
            Initialized,
            Disconnected,
            Connecting,
            Connected,
            Registered,
            WaitToRegister,
            ReadyToRegister,
            RegistrationFailed,
            Reconfigure,
            Configured,
            ConfiguringFailed,
            Shutdown
        }
    }
}