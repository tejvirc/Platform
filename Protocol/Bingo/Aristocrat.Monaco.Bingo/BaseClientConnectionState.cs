namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Monaco.Common;
    using Protocol.Common.Storage.Entity;
    using Stateless;
    using Timer = System.Threading.Timer;

    public class BaseClientConnectionState<TClientType> : IBaseClientConnectionState, IDisposable
    {
        protected readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        protected readonly IPropertiesManager PropertiesManager;
        protected readonly IUnitOfWorkFactory UnitOfWorkFactory;
        protected CancellationTokenSource TokenSource;
        protected StateMachine<State, Trigger> RegistrationState;
        protected StateMachine<State, Trigger>.TriggerWithParameters<RegistrationFailureReason> FailedRegistrationTrigger;
        protected StateMachine<State, Trigger>.TriggerWithParameters<ConfigurationFailureReason> FailedConfigurationTrigger;
        protected readonly IEventBus EventBus;
        protected readonly ISystemDisableManager SystemDisable;

        private static readonly TimeSpan NoMessagesTimeout = TimeSpan.FromMilliseconds(40_000);
        private readonly IEnumerable<IClient> _clients;
        private readonly IClientConfigurationProvider _configurationProvider;
        private Timer _timeoutTimer;
        private bool _disposed;

        public BaseClientConnectionState(
            IEventBus eventBus,
            IEnumerable<IClient> clients,
            IClientConfigurationProvider configurationProvider,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisable,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            PropertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            SystemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            if (clients is null)
            {
                throw new ArgumentNullException(nameof(clients));
            }

            _clients = clients.AsEnumerable().Where(x => x is TClientType);
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

            EventBus.UnsubscribeAll(this);
            RegistrationState.Deactivate();
            foreach (var client in _clients)
            {
                client.Disconnected -= OnClientDisconnected;
                client.Connected -= OnClientConnected;
                client.MessageReceived -= OnMessageReceived;
            }

            _timeoutTimer.Dispose();
            var tokenSource = TokenSource;
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }

            TokenSource = null;
            _disposed = true;
        }

        public async Task Start()
        {
            await RegistrationState.FireAsync(Trigger.Initialized).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await RegistrationState.FireAsync(Trigger.Shutdown).ConfigureAwait(false);
        }

        protected virtual void Initialize()
        {
            CreateStateMachine();
            RegisterEventListeners();
            _timeoutTimer = new Timer(TimeoutOccurred);
        }

        protected virtual async Task OnDisconnected()
        {
            _timeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            ClientDisconnected?.Invoke(this, EventArgs.Empty);
            await RegistrationState.FireAsync(Trigger.Connecting).ConfigureAwait(false);
        }

        protected virtual async Task OnRegistering()
        {
            try
            {
                await RegistrationState.FireAsync(Trigger.Registered).ConfigureAwait(false);
            }
            catch (RegistrationException exception)
            {
                Logger.Error("Registration failed", exception);
                await RegistrationState.FireAsync(FailedRegistrationTrigger, exception.Reason).ConfigureAwait(false);
            }
        }

        protected virtual void OnRegisteringExit(StateMachine<State, Trigger>.Transition t)
        {
        }

        protected virtual void OnRegisteringFailed()
        {
        }

        protected virtual async Task OnConfiguring()
        {
            try
            {
                await RegistrationState.FireAsync(Trigger.Configured).ConfigureAwait(false);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error("Configuration failed", exception);
                await RegistrationState.FireAsync(FailedConfigurationTrigger, exception.Reason).ConfigureAwait(false);
            }
        }

        protected virtual void OnConfiguringExit(StateMachine<State, Trigger>.Transition t)
        {
        }

        private async Task OnIdle()
        {
            TokenSource?.Cancel();
            TokenSource?.Dispose();
            TokenSource = null;

            foreach (var client in _clients)
            {
                await client.Stop().ConfigureAwait(false);
            }
        }

        private async Task ConnectClient(CancellationToken token)
        {
            SetupFirewallRule();
            foreach (var client in _clients)
            {
                while (!await client.Start().ConfigureAwait(false) && !token.IsCancellationRequested)
                {
                    Logger.Info($"Client failed to connect retrying.  IsCancelled={token.IsCancellationRequested}");
                }
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
        }

        private void SetupFirewallRule()
        {
            using var configuration = _configurationProvider.CreateConfiguration();
            foreach (var client in _clients)
            {
                Firewall.AddRule(client.FirewallRuleName, (ushort)configuration.Address.Port, Firewall.Direction.Out);
            }
        }

        private void RegisterEventListeners()
        {
            EventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandleRestartingEvent,
                evt =>
                    string.Equals(ApplicationConstants.MachineId, evt.PropertyName, StringComparison.Ordinal) ||
                    string.Equals(ApplicationConstants.SerialNumber, evt.PropertyName, StringComparison.Ordinal));
            EventBus.Subscribe<ForceReconnectionEvent>(this, HandleRestartingEvent);
            foreach (var client in _clients)
            {
                client.Connected += OnClientConnected;
                client.Disconnected += OnClientDisconnected;
                client.MessageReceived += OnMessageReceived;
            }
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
            TokenSource?.Cancel(false);
            TokenSource?.Dispose();
            TokenSource = new CancellationTokenSource();
        }

        private void CreateStateMachine()
        {
            RegistrationState = new StateMachine<State, Trigger>(State.Idle);

            FailedRegistrationTrigger =
                RegistrationState.SetTriggerParameters<RegistrationFailureReason>(Trigger.RegistrationFailed);
            FailedConfigurationTrigger =
                RegistrationState.SetTriggerParameters<ConfigurationFailureReason>(Trigger.ConfiguringFailed);
            RegistrationState.Configure(State.Idle)
                .OnEntryAsync(OnIdle)
                .Permit(Trigger.Initialized, State.Disconnected);
            RegistrationState.Configure(State.Running)
                .Permit(Trigger.Shutdown, State.Idle);
            RegistrationState.Configure(State.Disconnected)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnDisconnected)
                .Permit(Trigger.Connecting, State.Connecting);
            RegistrationState.Configure(State.Connecting)
                .SubstateOf(State.Running)
                .OnEntry(OnConnecting)
                .PermitReentry(Trigger.Connecting)
                .PermitReentry(Trigger.Reconfigure)
                .Permit(Trigger.Connected, State.Registering);
            RegistrationState.Configure(State.Registering)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnRegistering)
                .OnExit(OnRegisteringExit)
                .PermitDynamic(FailedRegistrationTrigger, HandleRegistrationFailure)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Registered, State.Configuring);
            RegistrationState.Configure(State.InvalidRegistration)
                .SubstateOf(State.Running)
                .OnEntry(OnRegisteringFailed)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            RegistrationState.Configure(State.Configuring)
                .SubstateOf(State.Running)
                .OnEntryAsync(OnConfiguring)
                .OnExit(OnConfiguringExit)
                .PermitDynamic(FailedConfigurationTrigger, HandleConfigurationFailure)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Configured, State.Connected);
            RegistrationState.Configure(State.InvalidConfiguration)
                .SubstateOf(State.Running)
                .OnEntry(OnInvalidConfiguration)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            RegistrationState.Configure(State.ConfigurationMismatch)
                .SubstateOf(State.Running)
                .OnEntry(OnConfigurationMismatch)
                .Permit(Trigger.Disconnected, State.Disconnected)
                .Permit(Trigger.Reconfigure, State.Disconnected);
            RegistrationState.Configure(State.Connected)
                .SubstateOf(State.Running)
                .OnEntry(OnConnected)
                .Permit(Trigger.Reconfigure, State.Disconnected)
                .Permit(Trigger.Disconnected, State.Disconnected);

            RegistrationState.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error(
                        $"Invalid Registration State Transition. State : {state} Trigger : {trigger}");
                });

            RegistrationState.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
        }

        private void OnInvalidConfiguration()
        {
            EventBus.Publish(new InvalidConfigurationReceivedEvent());
            SystemDisable.Disable(
                BingoConstants.BingoHostConfigurationInvalidKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoInvalidConfiguration),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoInvalidConfigurationHelp));
        }

        private void OnConfigurationMismatch()
        {
            EventBus.Publish(new ConfigurationMismatchReceivedEvent());
            SystemDisable.Disable(
                BingoConstants.BingoHostConfigurationMismatchKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.BingoConfigurationChangeNVRamClearRequired),
                true,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.BingoConfigurationChangeNVRamClearRequiredHelp));
        }

        protected virtual void OnConnected()
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        private void TimeoutOccurred(object _)
        {
            RegistrationState.FireAsync(Trigger.Disconnected).FireAndForget();
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            RegistrationState.FireAsync(Trigger.Disconnected).FireAndForget();
        }

        private void OnClientConnected(object sender, ConnectedEventArgs e)
        {
            RegistrationState.FireAsync(Trigger.Connected).FireAndForget();
        }

        private void OnMessageReceived(object sender, EventArgs e)
        {
            if (RegistrationState.State is not State.Connected)
            {
                return;
            }

            _timeoutTimer.Change(NoMessagesConnectionTimeout, Timeout.InfiniteTimeSpan);
        }

        private void OnConnecting()
        {
            ResetConnectionToken();
            ConnectClient(TokenSource.Token).FireAndForget();
        }


        private async Task HandleRestartingEvent<TEvent>(TEvent evt, CancellationToken token)
        {
            SetupFirewallRule();
            await RegistrationState.FireAsync(Trigger.Reconfigure).ConfigureAwait(false);
        }

        protected enum State
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

        protected enum Trigger
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