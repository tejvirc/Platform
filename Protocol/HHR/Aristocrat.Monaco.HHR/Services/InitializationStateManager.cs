namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts.Localization;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Stateless;
    using Stateless.Graph;
    using Storage.Helpers;

    /// <summary>
    ///     Defines startup procedure for HHR Protocol and maintains InitializationState of HHR Protocol.
    /// </summary>
    public class InitializationStateManager : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICentralManager _centralManager;
        private readonly ICommunicationService _communicationService;
        private readonly Timer _connectTimer;
        private readonly IEventBus _eventBus;
        private readonly IGameDataService _gameDataService;
        private readonly Timer _initializationTimer;
        private readonly IPlayerSessionService _playerSessionService;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly Timer _readyToPlayTimer;
        private readonly ITransactionIdProvider _transactionIdProvider;
        private readonly IPendingRequestEntityHelper _pendingRequestEntityHelper;
        private readonly IDisposable _disposable;
        private readonly IPropertiesManager _properties;

        private StateMachine<State, Trigger> _stateMachine;
        private bool _disposed;

        /// <summary>
        ///     Constructor. Should subscribe for some events and use different service to perform startup routine.
        /// </summary>
        /// <param name="eventBus"></param>
        /// <param name="communicationService"></param>
        /// <param name="gameDataService"></param>
        /// <param name="centralManager"></param>
        /// <param name="playerSessionService"></param>
        /// <param name="systemDisableManager"></param>
        /// <param name="transactionIdProvider"></param>
        /// <param name="pendingRequestEntityHelper"></param>
        /// <param name="properties"></param>
        public InitializationStateManager(
            IEventBus eventBus,
            ICommunicationService communicationService,
            IGameDataService gameDataService,
            ICentralManager centralManager,
            IPlayerSessionService playerSessionService,
            ISystemDisableManager systemDisableManager,
            ITransactionIdProvider transactionIdProvider,
            IPendingRequestEntityHelper pendingRequestEntityHelper,
            IPropertiesManager properties)
        {
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _communicationService = communicationService
                ?? throw new ArgumentNullException(nameof(communicationService));
            _gameDataService = gameDataService
                ?? throw new ArgumentNullException(nameof(gameDataService));
            _centralManager = centralManager
                ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSessionService = playerSessionService
                ?? throw new ArgumentNullException(nameof(playerSessionService));
            _systemDisableManager = systemDisableManager
                ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _transactionIdProvider = transactionIdProvider
                ?? throw new ArgumentNullException(nameof(transactionIdProvider));
            _pendingRequestEntityHelper = pendingRequestEntityHelper
                ?? throw new ArgumentNullException(nameof(pendingRequestEntityHelper));
            _properties = properties
                ?? throw new ArgumentNullException(nameof(properties));

            _disposable = _centralManager.UnsolicitedResponses.Subscribe(
                UnsolicitedResponseReceived,
                error => { Logger.Error("Error while trying to receive UnsolicitedResponse from server.", error); });

            CreateStateMachine();

            Logger.Debug(UmlDotGraph.Format(_stateMachine.GetInfo()));
            SubscribeForEvents();

            _connectTimer = new Timer(HhrConstants.ReconnectTimeInMilliseconds) { AutoReset = true };
            _initializationTimer = new Timer(HhrConstants.ReInitializationTimeInMilliseconds) { AutoReset = true };
            _readyToPlayTimer = new Timer(HhrConstants.RetryReadyToPlay) { AutoReset = true };

            _connectTimer.Elapsed += ConnectTimerOnElapsed;
            _initializationTimer.Elapsed += InitializationTimerOnElapsed;
            _readyToPlayTimer.Elapsed += ReadyToPlayTimerOnElapsed;
            _stateMachine.FireAsync(Trigger.Disconnect);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _connectTimer?.Dispose();
                _initializationTimer?.Dispose();
                _readyToPlayTimer.Dispose();
                _eventBus.UnsubscribeAll(this);
                _disposable.Dispose();
            }

            _disposed = true;
        }

        private void ReadyToPlayTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _stateMachine.FireAsync(Trigger.WaitingForReadyToPlay);
        }

        private async void UnsolicitedResponseReceived(Response obj)
        {
            switch (obj)
            {
                case CommandResponse res when obj.Command == Command.CmdCommand:
                    switch (res.ECommand)
                    {
                        case GtCommand.PlayPause:
                            _readyToPlayTimer.Start();
                            break;
                        case GtCommand.Play:
                            _stateMachine.Fire(Trigger.Ready);
                            break;
                    }

                    break;

                case ParameterResponse _ when obj.Command == Command.CmdParameterGt:
                    await _communicationService.Disconnect();
                    _systemDisableManager.Enable(HhrConstants.ProgressivesInitializationFailedKey);
                    _systemDisableManager.Enable(HhrConstants.GameSelectionMismatchKey);
                    break;
            }
        }

        private void InitializationTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _stateMachine.FireAsync(Trigger.Initialize);
        }

        private async void ConnectTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await _communicationService.ConnectTcp();
        }

        private void SubscribeForEvents()
        {
            _eventBus.Subscribe<CentralServerOnline>(this, _ => _stateMachine.FireAsync(Trigger.Initialize));
            _eventBus.Subscribe<CentralServerOffline>(this, _ => _stateMachine.FireAsync(Trigger.Disconnect));
        }

        private void CreateStateMachine()
        {
            _stateMachine = new StateMachine<State, Trigger>(State.Created);

            _stateMachine.Configure(State.Created)
                .Permit(Trigger.Disconnect, State.Disconnected);

            _stateMachine.Configure(State.Disconnected)
                .OnEntry(OnDisconnected)
                .Ignore(Trigger.Disconnect)
                .PermitReentry(Trigger.WaitingForReadyToPlay)
                .Ignore(Trigger.InitializationFailed)
                .Permit(Trigger.Initialize, State.Initializing);

            _stateMachine.Configure(State.Initializing)
                .OnEntryAsync(OnInitializing)
                .Permit(Trigger.Disconnect, State.Disconnected)
                .Permit(Trigger.InitializationFailed, State.InitializationFailed)
                .PermitReentry(Trigger.Initialize)
                .Permit(Trigger.Ready, State.Ready)
                .Permit(Trigger.WaitingForReadyToPlay, State.WaitingForReadyToPlay);

            _stateMachine.Configure(State.InitializationFailed)
                .OnEntry(OnInitializationFailed)
                .Permit(Trigger.Disconnect, State.Disconnected)
                .Permit(Trigger.Initialize, State.Initializing);

            _stateMachine.Configure(State.WaitingForReadyToPlay)
                .OnEntryAsync(OnWaitingForReadyToPlay)
                .PermitReentry(Trigger.WaitingForReadyToPlay)
                .Permit(Trigger.Ready, State.Ready)
                .Permit(Trigger.Disconnect, State.Disconnected);

            _stateMachine.Configure(State.Ready)
                .OnEntry(OnReady)
                .Permit(Trigger.Disconnect, State.Disconnected);

            _stateMachine.OnTransitioned(OnTransitionAction);
        }

        private void OnTransitionAction(StateMachine<State, Trigger>.Transition transition)
        {
            Logger.Debug(
                $"Transitioning From: {transition.Source} To: {transition.Destination}, via Trigger: {transition.Trigger}");
        }

        private async Task OnWaitingForReadyToPlay()
        {
            try
            {
                _ = await _centralManager.Send<ReadyToPlayRequest, CloseTranResponse>(new ReadyToPlayRequest());
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to receive response for ReadyToPlay", e);
            }
        }

        private void OnInitializationFailed()
        {
            _initializationTimer.Start();
            if (!_systemDisableManager.CurrentDisableKeys.Contains(HhrConstants.ProgressivesInitializationFailedKey))
            {
                _eventBus.Publish(new ProtocolInitializationFailed());
            }
        }

        private void OnReady()
        {
            Logger.Debug("Protocol initialization complete");

            _eventBus.Publish(new ProtocolInitializationComplete());

            _readyToPlayTimer.Stop();
        }

        private async Task OnInitializing()
        {
            Logger.Debug("Protocol initializing");

            _connectTimer.Stop();
            _initializationTimer.Stop();

            _eventBus.Publish(new ProtocolInitializationInProgress());

            try
            {
                await _centralManager.Send(new ConnectRequest());
            }
            catch (Exception e)
            {
                Logger.Warn("Initialization Failed : Unable to connect to CentralServer.", e);
                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            var parameters = await _gameDataService.GetGameParameters(true);
            if (parameters == null)
            {
                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            // In case we had transactions that hadn't reached the server yet, find the highest transaction ID we have already
            // allocated for use.
            foreach (var request in _pendingRequestEntityHelper.PendingRequests)
            {
                if (request.Key is TransactionRequest transaction &&
                    transaction.TransactionId > parameters.LastTransactionId)
                {
                    parameters.LastTransactionId = transaction.TransactionId;
                }

                if (request.Key is HandpayCreateRequest handpay &&
                    handpay.TransactionId > parameters.LastTransactionId)
                {
                    parameters.LastTransactionId = handpay.TransactionId;
                }
            }

            _transactionIdProvider.SetLastId(parameters.LastTransactionId);

            ClientProperties.ParameterDeviceId = parameters.ParameterDeviceId;
            ClientProperties.ManualHandicapTimeOut = parameters.HandicapPickTimer;
            ClientProperties.RaceStatTimeOut = parameters.HandicapStatTimer;

            ClientProperties.ManualHandicapMode =
                _properties.GetValue(HHRPropertyNames.ManualHandicapMode, HhrConstants.DetectPickMode);

            if (ClientProperties.ManualHandicapMode == HhrConstants.DetectPickMode)
            {
                ClientProperties.ManualHandicapMode = parameters.ManualHandicapMode == 1 ?
                    HhrConstants.AutoPickMode : HhrConstants.QuickPickMode;
            }

            ClientProperties.ProgressiveUdpIp = !string.IsNullOrEmpty(parameters.UdpIp)
                ? parameters.UdpIp
                : IPAddress.Broadcast.ToString();
            if (!await _communicationService.ConnectUdp())
            {
                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            var gameInfo = (await _gameDataService.GetGameInfo(true)).ToList();
            if (gameInfo.Count != parameters.GameIdCount)
            {
                Logger.Warn(
                    $"Initialization failed : Game info mismatch (GT Game count ={gameInfo.Count}, Server Game count={parameters.GameIdCount})");

                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            try
            {
                // In the field after a server restart we see very long delays here, so we give an
                // extra long timeout to avoid getting stuck on this repeatedly.
                _ = await _playerSessionService.GetCurrentPlayerId(HhrConstants.StartupPlayerIdFetchTimeoutMilliseconds);
            }
            catch (Exception)
            {
                Logger.Warn("Initialization failed : Unable to fetch Player Id");

                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            try
            {
                _ = await _gameDataService.GetProgressiveInfo(true);
            }
            catch (Exception ex)
            {
                Logger.Warn("Initialization failed : Unable to fetch progressives", ex);
                await _stateMachine.FireAsync(Trigger.InitializationFailed);
                return;
            }

            await _stateMachine.FireAsync(Trigger.WaitingForReadyToPlay);
        }

        private void OnDisconnected()
        {
            _initializationTimer.Stop();
            _readyToPlayTimer.Stop();

            _systemDisableManager.Disable(
                HhrConstants.CentralServerOffline,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HHRCentralServerOffline),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoHHRCentralServerOffline));

            _connectTimer.Start();
        }

        private enum Trigger
        {
            Disconnect,
            Initialize,
            InitializationFailed,
            WaitingForReadyToPlay,
            Ready
        }

        private enum State
        {
            Created,
            Disconnected,
            InitializationFailed,
            Initializing,
            WaitingForReadyToPlay,
            Ready
        }
    }
}