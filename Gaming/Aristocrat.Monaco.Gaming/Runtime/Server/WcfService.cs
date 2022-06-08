namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Application.Contracts.Extensions;
    using Client;
    using Commands;
    using Contracts;
    using Contracts.Central;
    using Contracts.Process;
    using GDKRuntime.Contract;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using LevelId = System.UInt32;
    using TransactionId = System.UInt64;
    using Value = System.UInt64;
    using WinAmount = System.UInt64;

    /// <summary>
    ///     Implements the IGameSession service contract for inter process communications with a game.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    public class WcfService : IGameSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IClientEndpointProvider<IRuntime> _serviceProvider;
        private readonly IClientEndpointProvider<IReelService> _reelServiceProvider;
        private readonly IClientEndpointProvider<IPresentationService> _presentationServiceProvider;
        private readonly IProcessManager _processManager;
        private readonly ICommandHandler<GetRandomNumber> _getRandomNumber;
        private readonly ICommandHandler<Shuffle> _shuffle;
        private readonly ICommandHandlerFactory _handlerFactory;
        private readonly ActionBlock<ButtonStateChanged> _buttonStateChangedProcessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WcfService" /> class.
        /// </summary>
        public WcfService(
            IEventBus eventBus,
            ICommandHandlerFactory handlerFactory,
            IGameDiagnostics gameDiagnostics,
            IClientEndpointProvider<IRuntime> serviceProvider,
            IClientEndpointProvider<IReelService> reelServiceProvider,
            IClientEndpointProvider<IPresentationService> presentationServiceProvider,
            IProcessManager processManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _reelServiceProvider = reelServiceProvider ?? throw new ArgumentNullException(nameof(reelServiceProvider));
            _presentationServiceProvider = presentationServiceProvider ??
                                           throw new ArgumentNullException(nameof(presentationServiceProvider));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            _buttonStateChangedProcessor =
                new ActionBlock<ButtonStateChanged>(cmd => _handlerFactory.Create<ButtonStateChanged>().Handle(cmd));

            _getRandomNumber = _handlerFactory.Create<GetRandomNumber>();
            _shuffle = _handlerFactory.Create<Shuffle>();
        }

        /// <inheritdoc />
        public void Join()
        {
            Logger.Debug("Client joined the Runtime Service");

            var wcfClient = new WcfClient(OperationContext.Current, _eventBus, _processManager);
            _serviceProvider.AddOrUpdate(wcfClient);
            _reelServiceProvider.AddOrUpdate(wcfClient);
            _presentationServiceProvider.AddOrUpdate(wcfClient);
            _eventBus.Publish(new GameConnectedEvent(_gameDiagnostics.IsActive));
        }

        /// <inheritdoc />
        public void Leave()
        {
            Logger.Debug("Client left the Runtime Service");

            _serviceProvider.Clear();
            _reelServiceProvider.Clear();
            _presentationServiceProvider.Clear();
            _eventBus.Publish(new GameExitedNormalEvent());
        }

        /// <inheritdoc />
        public void OnRuntimeStateChange(GDKRuntime.Contract.RuntimeState from, GDKRuntime.Contract.RuntimeState to)
        {
            Logger.Debug($"OnRuntimeStateChange {from}->{to}");
        }

        /// <inheritdoc />
        public void OnRuntimeFlagChange(RuntimeFlag flag, bool newState)
        {
            Logger.Debug($"OnRuntimeFlagChange {flag}={newState}");

            _handlerFactory.Create<RuntimeFlagChanged>()
                .Handle(new RuntimeFlagChanged((RuntimeCondition)flag, newState));
        }

        public void OnRecoveryPoint(byte[] data)
        {
            Logger.Debug("Received Recovery Point");

            _handlerFactory.Create<AddRecoveryDataPoint>()
                .Handle(new AddRecoveryDataPoint(data));
        }

        /// <inheritdoc />
        public bool BeginGameRound(uint denom)
        {
            Logger.Debug($"BeginGameRound({denom})");

            if (_gameDiagnostics.IsActive)
            {
                return true;
            }

            var command = new BeginGameRound(denom);

            _handlerFactory.Create<BeginGameRound>().Handle(command);

            Logger.Debug($"BeginGameRound responding with ({command.Success})");

            return command.Success;
        }

        public bool BeginGameRoundAsync(
            uint denom,
            uint betAmount,
            uint wagerCategoryId,
            CentralOutcome request,
            IList<GameInfo> gameDetails,
            byte[] data)
        {
            Logger.Debug($"BeginGameRoundAsync(denom={denom}, betAmount={betAmount}, wagerCategoryId={wagerCategoryId}, request=(cnt={request.OutcomeCount}, tmplId={request.TemplateId}, gameDetails={gameDetails?.FirstOrDefault()} ,data={data})");

            IOutcomeRequest outcomeRequest = request.OutcomeCount == 0 ? null : new OutcomeRequest((int)request.OutcomeCount, (int)request.TemplateId);

            if (gameDetails?.Count > 0)
            {
                var details = gameDetails.First();

                var command = new BeginGameRoundAsync(
                    denom,
                    betAmount,
                    (int)details.BetLinePreset,
                    data,
                    outcomeRequest,
                    (int)wagerCategoryId);

                // This will be run asynchronously from this method only
                _handlerFactory.Create<BeginGameRoundAsync>().Handle(command);
            }

            return true;
        }

        public void BeginGameRoundResult(IList<uint> pendingJackpotTriggers, GameRoundDetails gameRoundDetails)
        {
            Logger.Debug(
                $"BeginGameRoundResult({pendingJackpotTriggers.Count}, {gameRoundDetails?.PresentationIndex})");

            if (!pendingJackpotTriggers.IsNullOrEmpty())
            {
                var command = new PendingTrigger(pendingJackpotTriggers.Select(l => (int)l).ToList());
                _handlerFactory.Create<PendingTrigger>().Handle(command);
            }

            if (gameRoundDetails is not null)
            {
                _handlerFactory.Create<BeginGameRoundResults>()
                    .Handle(new BeginGameRoundResults((long)gameRoundDetails.PresentationIndex));
            }
        }

        /// <inheritdoc />
        public void GameRoundEvent(
            GameRoundEventType eventType,
            GameRoundEventStage stage,
            GameRoundPlayMode playMode,
            IList<string> gameRoundInfo,
            ulong bet,
            ulong win,
            ulong stake,
            byte[] data)
        {
            Logger.Debug(
                $"GameRoundEvent type: {eventType} stage: {stage} mode: {playMode} bet: {bet} stake: {stake} win: {win}");

            // Replay gets all data from blob.  Do not record events etc.
            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<GameRoundEvent>()
                    .Handle(new GameRoundEvent((GameRoundEventState)eventType, (GameRoundEventAction)stage, (Client.PlayMode)playMode, gameRoundInfo, bet, win, stake, data));
            }
            else if (playMode == GameRoundPlayMode.Replay)
            {
                _handlerFactory.Create<ReplayGameRoundEvent>()
                    .Handle(new ReplayGameRoundEvent((GameRoundEventState)eventType, (GameRoundEventAction)stage, gameRoundInfo));
            }
        }

        /// <inheritdoc />
        public void EndGameRound(ulong betAmount, ulong winAmount)
        {
            Logger.Debug($"EndGameRound: {betAmount} {winAmount}");

            // Replay gets all data from blob.  Do not record events etc.
            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<SetGameResult>()
                    .Handle(new SetGameResult((long)winAmount));
            }
        }

        /// <inheritdoc />
        public ulong GetRandomNumberU64(ulong range)
        {
            Logger.Debug($"GetRandomNumberU64({range})");

            var command = new GetRandomNumber(range);
            _getRandomNumber.Handle(command);

            return command.Value;
        }

        /// <inheritdoc />
        public uint GetRandomNumberU32(uint range)
        {
            Logger.Debug($"GetRandomNumberU32({range})");

            var command = new GetRandomNumber(range);
            _getRandomNumber.Handle(command);

            return (uint)command.Value;
        }

        /// <inheritdoc />
        public IList<ulong> Shuffle(IList<ulong> values)
        {
            var command = new Shuffle(new List<ulong>(values));

            _shuffle.Handle(command);

            return command.Values;
        }

        /// <inheritdoc />
        public void ButtonDeckImageChanged()
        {
            Task.Run(() => _handlerFactory.Create<UpdateButtonDeckImage>().Handle(new UpdateButtonDeckImage()));
        }

        /// <inheritdoc />
        public void GetDisplaysToBeCaptured(int[] displayIndices)
        {
            Logger.Debug("GetDisplaysToBeCaptured");
        }

        /// <inheritdoc />
        public IDictionary<string, ulong> GetMeters()
        {
            // Replay gets all of it's data from the blob
            if (_gameDiagnostics.IsActive)
            {
                return new Dictionary<string, ulong>();
            }

            var command = new GetInGameMeters();

            _handlerFactory.Create<GetInGameMeters>()
                .Handle(command);

            Logger.Debug(
                $"GetMeters meters: {string.Join(",", command.MeterValues.Keys)} with values: {string.Join(",", command.MeterValues.Values)}");

            return command.MeterValues;
        }

        /// <inheritdoc />
        public void SetMeters(IDictionary<string, ulong> values)
        {
            Logger.Debug(
                $"SetMeters meters: {string.Join(",", values.Keys)} with values: {string.Join(",", values.Values)}");

            // Don't update meters for replay
            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<SetInGameMeters>()
                    .Handle(new SetInGameMeters(values));
            }
        }

        /// <inheritdoc />
        public void OnRuntimeEvent(RuntimeEvent runtimeEvent)
        {
            switch (runtimeEvent)
            {
                case RuntimeEvent.NotifyGameReady:
                    Logger.Debug("Notify Game Ready/Started");

                    _eventBus.Publish(new GameLoadedEvent());
                    break;
                case RuntimeEvent.RequestGameExit:
                    Logger.Debug("Client Requested Game Exit");

                    if (!_gameDiagnostics.IsActive)
                    {
                        _eventBus.Publish(new GameRequestExitEvent());
                    }
                    else
                    {
                        _gameDiagnostics.End();
                    }
                    break;
                case RuntimeEvent.RequestCashout:
                    Logger.Debug("Client Requested Cashout");

                    _handlerFactory.Create<RequestCashout>()
                        .Handle(new RequestCashout());
                    break;
                case RuntimeEvent.ServiceButtonPressed:
                    Logger.Debug("Service button pressed");

                    _handlerFactory.Create<ServiceButton>()
                        .Handle(new ServiceButton());
                    break;
                case RuntimeEvent.PlayerInfoDisplayMenuRequested:
                    Logger.Debug("'I' button pressed - requesting entry");

                    _handlerFactory.Create<PlayerInfoDisplayEnterRequest>()
                        .Handle(new PlayerInfoDisplayEnterRequest());
                    break;
                case RuntimeEvent.PlayerInfoDisplayExited:
                    Logger.Debug("Play information display exited");

                    _handlerFactory.Create<PlayerInfoDisplayExitRequest>()
                        .Handle(new PlayerInfoDisplayExitRequest());
                    break;
                case RuntimeEvent.PlayerMenuEntered:
                    Logger.Debug("PlayerMenu button pressed - requesting entry");

                    _handlerFactory.Create<PlayerMenuEnterRequest>()
                        .Handle(new PlayerMenuEnterRequest());
                    break;
                case RuntimeEvent.PlayerMenuExited:
                    Logger.Debug("PlayerMenu button pressed - requesting exit");

                    _handlerFactory.Create<PlayerMenuExitRequest>()
                        .Handle(new PlayerMenuExitRequest());
                    break;
                case RuntimeEvent.RequestConfiguration:
                    Logger.Debug("Client Requested Configuration");

                    _handlerFactory.Create<ConfigureClient>()
                        .Handle(new ConfigureClient());
                    break;
                case RuntimeEvent.AdditionalInfoButtonPressed:
                    Logger.Debug("AdditionalInfo button pressed");

                    _handlerFactory.Create<AdditionalInfoButton>()
                        .Handle(new AdditionalInfoButton());
                    break;
                case RuntimeEvent.GameAttractModeExited:
                    _handlerFactory.Create<AttractModeEnded>()
                        .Handle(new AttractModeEnded());
                    break;
                case RuntimeEvent.GameAttractModeEntered:
                    _handlerFactory.Create<AttractModeStarted>()
                        .Handle(new AttractModeStarted());
                    break;
                case RuntimeEvent.GameSelectionScreenEntered:
                case RuntimeEvent.GameSelectionScreenExited:
                    _eventBus.Publish(new GameSelectionScreenEvent(
                        runtimeEvent == RuntimeEvent.GameSelectionScreenEntered));
                    break;
                case RuntimeEvent.RequestAllowGameRound:
                    // Not used
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public void OnButtonStatesChanged(IDictionary<uint, SystemButtonState> newStates)
        {
            Logger.Debug($"OnButtonStatesChanged - number of count: {newStates.Count} ");

            if (!_gameDiagnostics.IsActive)
            {
                var command = new ButtonStateChanged(
                    newStates.ToDictionary(s => s.Key, s => (ButtonState)s.Value));

                _buttonStateChangedProcessor.Post(command);
            }
        }

        /// <inheritdoc />
        public bool ConnectJackpotPool(string poolName)
        {
            Logger.Debug($"ConnectJackpotPool {poolName}");

            // Do not filter this for replay or recovery. The command will sort out what to do
            var command = new ConnectJackpotPool(poolName);
            _handlerFactory.Create<ConnectJackpotPool>()
                .Handle(command);

            return command.Connected;
        }

        /// <inheritdoc />
        public IDictionary<LevelId, Value> GetJackpotValues(GameRoundPlayMode playMode, string poolName)
        {
            //Logger.Debug($"GetJackpotValues mode: {playMode} name: {poolName}");

            var command = new GetJackpotValues(poolName, playMode.IsReplayOrRecovery());

            _handlerFactory.Create<GetJackpotValues>()
                .Handle(command);

            return command.JackpotValues.ToDictionary(
                item => (uint)item.Key,
                item => (ulong)item.Value.MillicentsToCents());
        }

        /// <inheritdoc />
        public void IncrementJackpotValues(
            GameRoundPlayMode playMode,
            string poolName,
            IDictionary<LevelId, PoolValue> values)
        {
            Logger.Debug($"IncrementJackpotValues - mode: {playMode} name: {poolName} values: {values.Count}");

            var command = values.ToDictionary(
                v => v.Key,
                v => new PoolIncrement { Cents = v.Value.Cents, Fraction = v.Value.Fraction });

            var incrementJackpotValues = new IncrementJackpotValues(poolName, command, playMode.IsReplayOrRecovery());

            _handlerFactory.Create<IncrementJackpotValues>()
                .Handle(incrementJackpotValues);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Return value is IDictionary{LevelId, TransactionId}
        /// </remarks>
        public IDictionary<LevelId, TransactionId> TriggerJackpot(
            GameRoundPlayMode playMode,
            string poolName,
            IList<LevelId> levels,
            IList<TransactionId> transactionIds)
        {
            Logger.Debug(
                $"TriggerJackpot mode: {playMode} poolName: {poolName} levels: {string.Join(",", levels)} transactionIds:{string.Join(",", transactionIds)}");

            var command = new TriggerJackpot(
                poolName,
                levels.Select(i => (int)i).ToList(),
                transactionIds.Select(i => (long)i).ToList(),
                playMode.IsReplayOrRecovery());

            _handlerFactory.Create<TriggerJackpot>()
                .Handle(command);

            var results = command.Results.ToDictionary(key => (uint)key.LevelId, value => (ulong)value.TransactionId);

            Logger.Debug(
                $"TriggerJackpot Response poolName:{poolName} levels: {string.Join(",", levels)} transactionIds:{string.Join(",", results.Values)}");

            return results;
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Return value is IDictionary{LevelId, Amount}
        /// </remarks>
        public IDictionary<LevelId, WinAmount> ClaimJackpot(GameRoundPlayMode playMode, string poolName, IList<ulong> transactionIds)
        {
            Logger.Debug($"ClaimJackpot mode: {playMode} poolName:{poolName} transactionIds:{string.Join(",", transactionIds)}");

            var command = new ClaimJackpot(
                poolName,
                transactionIds.Select(i => (long)i).ToList());

            _handlerFactory.Create<ClaimJackpot>()
                .Handle(command);

            var results = command.Results.ToDictionary(
                key => (uint)key.LevelId,
                value => (ulong)value.WinAmount / GamingConstants.Millicents);

            Logger.Debug(
                $"ClaimJackpot Response poolName:{poolName} transactionIds:{string.Join(",", transactionIds)} amounts:{string.Join(",", results.Values)}");

            return results;
        }

        public void SetJackpotLevelWagers(IList<ulong> wagers)
        {
            Logger.Debug($"SetJackpotWager wagers:{string.Join(",", wagers)}");

            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<ProgressiveLevelWagers>()
                    .Handle(new ProgressiveLevelWagers(wagers.Select(x => (long)x)));
            }
        }

        public void SetBonusKey(string poolName, string key)
        {
            Logger.Debug($"SetBonusKey poolName: {poolName} key: {key}");

            _handlerFactory.Create<SetBonusKey>()
                .Handle(new SetBonusKey(poolName, key));
        }

        public void SelectDenomination(uint denom)
        {
            Logger.Debug($"Select Denomination with denom: {denom}");

            var selectDenom = new SelectDenomination(denom);

            _handlerFactory.Create<SelectDenomination>()
                .Handle(selectDenom);
        }

        public void UpdateBetOptions(BetOptionData betOptions)
        {
            Logger.Debug(
                $"Update Bet Line options: [Ante: {betOptions.Ante}, Wager: {betOptions.Wager}, BetMultiplier: {betOptions.BetMultiplier}, LineCost: {betOptions.LineCost}, NumberLines: {betOptions.NumberLines}, StakeAmount: {betOptions.StakeAmount}]");

            var bet = new UpdateBetOptions(
                (long)betOptions.Wager,
                (long)betOptions.StakeAmount,
                (int)betOptions.BetMultiplier,
                (int)betOptions.LineCost,
                (int)betOptions.NumberLines,
                (int)betOptions.Ante,
                (int)betOptions.BetLinePresetId);

            _handlerFactory.Create<UpdateBetOptions>()
                .Handle(bet);
        }

        /// <inheritdoc />
        public void SetLocalStorage(IDictionary<StorageLocation, IDictionary<string, string>> storages)
        {
            Logger.Debug("SetLocalStorage called");

            var command = new SetLocalStorage
            {
                Values = storages.ToDictionary(
                    value => (StorageType)value.Key,
                    value => new Dictionary<string, string>(value.Value) as IDictionary<string, string>)
            };

            _handlerFactory.Create<SetLocalStorage>()
                .Handle(command);
        }

        /// <inheritdoc />
        public IDictionary<StorageLocation, IDictionary<string, string>> GetLocalStorage()
        {
            Logger.Debug("GetLocalStorage called");

            var command = new GetLocalStorage();

            _handlerFactory.Create<GetLocalStorage>()
                .Handle(command);

            return command.Values.ToDictionary(
                value => (StorageLocation)value.Key,
                value => new Dictionary<string, string>(value.Value) as IDictionary<string, string>);
        }

        public void OnGameFatalError(ErrorCode errorCode, string errorMessage)
        {
            Logger.Debug($"OnGameFatalError with message: {errorMessage}");

            _handlerFactory.Create<GameFatalError>()
                .Handle(new GameFatalError((GameErrorCode)errorCode, errorMessage));
        }

        public bool OnRuntimeRequest(RuntimeRequestType request)
        {
            Logger.Debug($"Handle RuntimeRequest: {request}");

            var command = new RuntimeRequest((RuntimeRequestState)request);

            _handlerFactory.Create<RuntimeRequest>().Handle(command);

            Logger.Debug($"OnRuntimeRequest with request: {request} Result: {command.Result}");

            return command.Result;
        }

        public bool SpinReels(IList<GDKRuntime.Contract.ReelSpinData> request)
        {
            Logger.Debug($"SpinReels");

            var spinData = new Hardware.Contracts.Reel.ReelSpinData[request.Count];
            for (var i = 0; i < request.Count; ++i)
            {
                var direction = request[i].Direction == Direction.Forward ? SpinDirection.Forward : SpinDirection.Backwards;
                spinData[i] = new Hardware.Contracts.Reel.ReelSpinData(
                    request[i].ReelId,
                    direction,
                    request[i].Speed,
                    request[i].Step);
            }

            var command = new SpinReels(spinData);
            _handlerFactory.Create<SpinReels>()
                .Handle(command);

            Logger.Debug($"SpinReels with request: {request} Result: {command.Success}");

            return command.Success;
        }

        public bool NudgeReels(IList<ReelNudgeData> request)
        {
            Logger.Debug($"NudgeReels");

            var nudgeSpinData = new NudgeReelData[request.Count];
            for (var i = 0; i < request.Count; ++i)
            {
                var direction = request[i].Direction == Direction.Forward ? SpinDirection.Forward : SpinDirection.Backwards;
                nudgeSpinData[i] = new NudgeReelData(
                    request[i].ReelId,
                    direction,
                    request[i].Speed,
                    request[i].Steps);
            }

            var command = new NudgeReels(nudgeSpinData);
            _handlerFactory.Create<NudgeReels>()
                .Handle(command);

            Logger.Debug($"NudgeReels with request: {request} Result: { command.Success}");

            return command.Success;
        }

        public IDictionary<int, ReelState> GetReelsState()
        {
            Logger.Debug($"GetReelsState");

            var command = new GetReelState();
            _handlerFactory.Create<GetReelState>()
                .Handle(command);

            var response = new Dictionary<int, ReelState>();
            foreach (var reelState in command.States)
            {
                response.Add(reelState.Key, (ReelState)reelState.Value);
            }

            return response;
        }

        public IList<int> GetConnectedReels()
        {
            Logger.Debug($"GetConnectedReels");

            var command = new ConnectedReels();
            _handlerFactory.Create<ConnectedReels>()
                .Handle(command);

            var response = new List<int>();
            response.AddRange(command.ReelIds);
            return response;
        }

        public bool UpdateReelsSpeed(IList<GDKRuntime.Contract.ReelSpeedData> request)
        {
            Logger.Debug($"UpdateReelsSpeed");

            var speedData = new Hardware.Contracts.Reel.ReelSpeedData[request.Count];
            for (var i = 0; i < request.Count; ++i)
            {
                speedData[i] = new Hardware.Contracts.Reel.ReelSpeedData(
                    request[i].ReelId,
                    request[i].Speed);
            }

            var command = new UpdateReelsSpeed(speedData);
            _handlerFactory.Create<UpdateReelsSpeed>()
                .Handle(command);

            Logger.Debug($"UpdateReelsSpeed with request: {request} Result: {command.Success}");

            return command.Success;
        }

        /// <inheritdoc />
        public bool RegisterPresentation(IList<GDKRuntime.Contract.PresentationOverrideTypes> presentationTypes)
        {
            Logger.Debug($"OnRegisterPresentation");

            var registeredTypes = new Contracts.PresentationOverrideTypes[presentationTypes.Count];

            for (var i = 0; i < presentationTypes.Count; i++)
            {
                registeredTypes[i] = presentationTypes[i] switch
                {
                    GDKRuntime.Contract.PresentationOverrideTypes.PrintingCashoutTicket => Contracts.PresentationOverrideTypes.PrintingCashoutTicket,
                    GDKRuntime.Contract.PresentationOverrideTypes.PrintingCashwinTicket => Contracts.PresentationOverrideTypes.PrintingCashwinTicket,
                    GDKRuntime.Contract.PresentationOverrideTypes.TransferingInCredits => Contracts.PresentationOverrideTypes.TransferingInCredits,
                    GDKRuntime.Contract.PresentationOverrideTypes.TransferingOutCredits => Contracts.PresentationOverrideTypes.TransferingOutCredits,
                    GDKRuntime.Contract.PresentationOverrideTypes.JackpotHandpay => Contracts.PresentationOverrideTypes.JackpotHandpay,
                    GDKRuntime.Contract.PresentationOverrideTypes.BonusJackpot => Contracts.PresentationOverrideTypes.BonusJackpot,
                    GDKRuntime.Contract.PresentationOverrideTypes.CancelledCreditsHandpay => Contracts.PresentationOverrideTypes.CancelledCreditsHandpay,
                    _ => throw new ArgumentOutOfRangeException(nameof(presentationTypes), @"Unexpected presentation type registration")
                };
            }

            var command = new RegisterPresentation(true, registeredTypes);
            _handlerFactory.Create<RegisterPresentation>()
                .Handle(command);

            return command.Result;

        }

        public IDictionary<uint, bool> CheckMysteryJackpot()
        {
            var command = new CheckMysteryJackpot();

            _handlerFactory.Create<CheckMysteryJackpot>()
                .Handle(command);

            var results = command.Results;

            return results;
        }
    }
}
