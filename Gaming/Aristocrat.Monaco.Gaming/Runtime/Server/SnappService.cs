namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Application.Contracts.Extensions;
    using Client;
    using Commands;
    using Contracts;
    using Contracts.Central;
    using Contracts.Events;
    using Contracts.Process;
    using GdkRuntime.V1;
    using Kernel;
    using log4net;
    using EventTypes = GdkRuntime.V1.RuntimeEventNotification.Types.RuntimeEvent;
    using GameRoundDetails = GdkRuntime.V1.GameRoundDetails;
    using LocalStorage = GdkRuntime.V1.LocalStorage;

    public class SnappService : IGameServiceCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly Empty EmptyResult = new ();

        private readonly IEventBus _bus;
        private readonly ICommandHandlerFactory _handlerFactory;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IClientEndpointProvider<IRuntime> _serviceProvider;
        private readonly IClientEndpointProvider<IReelService> _reelServiceProvider;
        private readonly IClientEndpointProvider<IPresentationService> _presentationServiceProvider;
        private readonly IProcessManager _processManager;
        private readonly ICommandHandler<GetRandomNumber> _getRandomNumber;
        private readonly ICommandHandler<Shuffle> _shuffle;
        private readonly ActionBlock<ButtonStateChanged> _buttonStateChangedProcessor;

        public SnappService(
            IEventBus bus,
            ICommandHandlerFactory handlerFactory,
            IGameDiagnostics gameDiagnostics,
            IClientEndpointProvider<IRuntime> serviceProvider,
            IClientEndpointProvider<IReelService> reelServiceProvider,
            IClientEndpointProvider<IPresentationService> presentationServiceProvider,
            IProcessManager processManager)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
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

        public override Empty Join(JoinRequest request)
        {
            Logger.Debug("Client joined the Runtime Service");

            var snappClient = new SnappClient(_bus, _processManager);
            _serviceProvider.AddOrUpdate(snappClient);
            _reelServiceProvider.AddOrUpdate(snappClient);
            _presentationServiceProvider.AddOrUpdate(snappClient);

            _bus.Publish(new GameConnectedEvent(_gameDiagnostics.IsActive));

            return EmptyResult;
        }

        public override Empty Leave(LeaveRequest request)
        {
            Logger.Debug("Client left the Runtime Service");

            _serviceProvider.Clear();
            _reelServiceProvider.Clear();
            _presentationServiceProvider.Clear();

            _bus.Publish(new GameExitedNormalEvent());

            return EmptyResult;
        }

        public override Empty FatalError(FatalErrorNotification request)
        {
            Logger.Debug($"OnGameFatalError with message: {request.ErrorCode} {request.Message}");

            _handlerFactory.Create<GameFatalError>()
                .Handle(new GameFatalError((GameErrorCode)request.ErrorCode, request.Message));

            return EmptyResult;
        }

        public override Empty RuntimeEvent(RuntimeEventNotification request)
        {
            switch (request.RuntimeEvent)
            {
                case EventTypes.NotifyGameReady:
                    Logger.Debug("Notify Game Ready/Started");

                    _bus.Publish(new GameLoadedEvent());
                    break;
                case EventTypes.RequestGameExit:
                    Logger.Debug("Client Requested Game Exit");

                    if (!_gameDiagnostics.IsActive)
                    {
                        _bus.Publish(new GameRequestExitEvent());
                    }
                    else
                    {
                        _gameDiagnostics.End();
                    }

                    break;
                case EventTypes.RequestCashout:
                    Logger.Debug("Client Requested Cashout");

                    _handlerFactory.Create<RequestCashout>()
                        .Handle(new RequestCashout());
                    break;
                case EventTypes.ServiceButtonPressed:
                    Logger.Debug("Service button pressed");

                    _handlerFactory.Create<ServiceButton>()
                        .Handle(new ServiceButton());
                    break;
                case EventTypes.PlayerInfoDisplayMenuRequested:
                    Logger.Debug("'I' button pressed - requesting entry");

                    _handlerFactory.Create<PlayerInfoDisplayEnterRequest>()
                        .Handle(new PlayerInfoDisplayEnterRequest());
                    break;
                case EventTypes.PlayerInfoDisplayExited:
                    Logger.Debug("Play information display exited");

                    _handlerFactory.Create<PlayerInfoDisplayExitRequest>()
                        .Handle(new PlayerInfoDisplayExitRequest());
                    break;
                case EventTypes.PlayerMenuEntered:
                    Logger.Debug("PlayerMenu button pressed - requesting entry");

                    _handlerFactory.Create<PlayerMenuEnterRequest>()
                        .Handle(new PlayerMenuEnterRequest());
                    break;
                case EventTypes.PlayerMenuExited:
                    Logger.Debug("PlayerMenu button pressed - requesting exit");

                    _handlerFactory.Create<PlayerMenuExitRequest>()
                        .Handle(new PlayerMenuExitRequest());
                    break;
                case EventTypes.RequestConfiguration:
                    Logger.Debug("Client Requested Configuration");

                    _handlerFactory.Create<ConfigureClient>()
                        .Handle(new ConfigureClient());
                    break;
                case EventTypes.AdditionalInfoButtonPressed:
                    Logger.Debug("AdditionalInfo button pressed");

                    _handlerFactory.Create<AdditionalInfoButton>()
                        .Handle(new AdditionalInfoButton());
                    break;
                case EventTypes.GameAttractModeExited:
                    _handlerFactory.Create<AttractModeEnded>()
                        .Handle(new AttractModeEnded());
                    break;
                case EventTypes.GameAttractModeEntered:
                    _handlerFactory.Create<AttractModeStarted>()
                        .Handle(new AttractModeStarted());
                    break;
                case RuntimeEventNotification.Types.RuntimeEvent.GameSelectionScreenEntered:
                case RuntimeEventNotification.Types.RuntimeEvent.GameSelectionScreenExited:
                    _bus.Publish(new GameSelectionScreenEvent(
                        request.RuntimeEvent == RuntimeEventNotification.Types.RuntimeEvent.GameSelectionScreenEntered));
                    break;
                case RuntimeEventNotification.Types.RuntimeEvent.RequestAllowGameRound:
                    // Not used
                    break;
                case EventTypes.MaxWinReached:
                    if (!_gameDiagnostics.IsActive)
                    {
                        _bus.Publish(new MaxWinReachedEvent());
                    }
                    break;
                case EventTypes.GameIdleActivity:
                    _bus.Publish(new UserInteractionEvent());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return EmptyResult;
        }

        public override Empty RuntimeFlagChange(RuntimeFlagNotification request)
        {
            Logger.Debug($"OnRuntimeFlagChange {request.Flag} {request.State}");

            _handlerFactory.Create<RuntimeFlagChanged>()
                .Handle(new RuntimeFlagChanged((RuntimeCondition)request.Flag, request.State));

            return EmptyResult;
        }

        public override RuntimeRequestResponse RuntimeRequest(RuntimeRequestNotification request)
        {
            Logger.Debug($"Handle RuntimeRequest: {request.Request}");

            var command = new RuntimeRequest((RuntimeRequestState)request.Request);

            _handlerFactory.Create<RuntimeRequest>().Handle(command);

            Logger.Debug($"OnRuntimeRequest with request: {request} Result: {command.Result}");

            return new RuntimeRequestResponse { Result = command.Result };
        }

        public override Empty RuntimeStateChange(RuntimeStateNotication request)
        {
            Logger.Debug($"OnRuntimeStateChange {request.From} {request.To}");

            return EmptyResult;
        }

        public override Empty ButtonDeckImageChanged(Empty request)
        {
            Task.Run(() => _handlerFactory.Create<UpdateButtonDeckImage>().Handle(new UpdateButtonDeckImage()));

            return EmptyResult;
        }

        public override Empty ButtonStatesChanged(ButtonStatesChangedNotfication request)
        {
            Logger.Debug($"OnButtonStatesChanged - count: {request.ButtonStates.Count} ");

            if (!_gameDiagnostics.IsActive)
            {
                var command = new ButtonStateChanged(
                    request.ButtonStates.ToDictionary(s => s.ButtonId, s => (ButtonState)s.State));

                _buttonStateChangedProcessor.Post(command);
            }

            return EmptyResult;
        }

        public override BeginGameRoundResponse BeginGameRound(BeginGameRoundRequest request)
        {
            Logger.Debug($"BeginGameRound({request})");

            if (_gameDiagnostics.IsActive)
            {
                return new BeginGameRoundResponse { Result = true };
            }

            var command = new BeginGameRound((long)request.Denomination);

            _handlerFactory.Create<BeginGameRound>().Handle(command);

            Logger.Debug($"BeginGameRound responding with ({command.Success})");

            return new BeginGameRoundResponse { Result = command.Success };
        }

        public override Empty BeginGameRoundAsync(BeginGameRoundAsyncRequest request)
        {
            Logger.Debug($"BeginGameRoundAsync({request})");

            IOutcomeRequest outcomeRequest = null;

            if (request.OutcomeRequest?.Is(CentralOutcome.Descriptor) ?? false)
            {
                var central = request.OutcomeRequest.Unpack<CentralOutcome>();

                outcomeRequest = new OutcomeRequest((int)central.OutcomeCount, (int)central.TemplateId);
            }

            if (request.GameDetails.FirstOrDefault()?.Is(GameInfo.Descriptor) ?? false)
            {
                var details = request.GameDetails.First().Unpack<GameInfo>();

                var command = new BeginGameRoundAsync(
                    (long)request.Denomination,
                    (long)request.BetAmount,
                    (int)details.BetLinePreset,
                    request.Data.ToByteArray(),
                    outcomeRequest,
                    (int)request.WagerCategoryId);

                // This will be run asynchronously from this method only
                _handlerFactory.Create<BeginGameRoundAsync>().Handle(command);
            }

            return EmptyResult;
        }

        public override Empty BeginGameRoundResult(BeginGameRoundResultNotification request)
        {
            Logger.Debug($"BeginGameRoundResult({request})");

            if (request.Results == null)
            {
                return EmptyResult;
            }

            foreach (var result in request.Results)
            {
                if (result.Is(PendingJackpotTriggers.Descriptor))
                {
                    var pending = result.Unpack<PendingJackpotTriggers>();
                    var command = new PendingTrigger(pending.Levels.Select(l => (int)l).ToList());
                    _handlerFactory.Create<PendingTrigger>().Handle(command);
                }

                if (result.Is(GameRoundDetails.Descriptor))
                {
                    var details = result.Unpack<GameRoundDetails>();
                    _handlerFactory.Create<BeginGameRoundResults>()
                        .Handle(new BeginGameRoundResults((long)details.PresentationIndex));
                }
            }

            return EmptyResult;
        }

        public override Empty GameRoundEvent(GameRoundEventRequest request)
        {

            Logger.Debug(
                $"GameRoundEvent type: {request.Type} stage: {request.Stage} mode: {request.PlayMode} bet: {request.BetAmount} stake: {request.Stake} win: {request.WinAmount}");
            try
            {
                // Replay gets all data from blob.  Do not record events etc.
                if (!_gameDiagnostics.IsActive)
                {
                    _handlerFactory.Create<GameRoundEvent>()
                        .Handle(
                            new GameRoundEvent(
                                (GameRoundEventState)request.Type,
                                (GameRoundEventAction)request.Stage,
                                (Client.PlayMode)request.PlayMode,
                                request.GameRoundInfo,
                                request.BetAmount,
                                request.WinAmount,
                                request.Stake,
                                request.Data.ToByteArray()));
                }
                else if (request.PlayMode == GameRoundPlayMode.ModeReplay)
                {
                    _handlerFactory.Create<ReplayGameRoundEvent>()
                        .Handle(
                            new ReplayGameRoundEvent(
                                (GameRoundEventState)request.Type,
                                (GameRoundEventAction)request.Stage,
                                request.GameRoundInfo));
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to handle GameRoundEvent type: {request.Type} stage: {request.Stage} mode: {request.PlayMode} bet: {request.BetAmount} stake: {request.Stake} win: {request.WinAmount}", e);
                throw;
            }

            Logger.Debug(
                $"Handled GameRoundEvent type: {request.Type} stage: {request.Stage} mode: {request.PlayMode} bet: {request.BetAmount} stake: {request.Stake} win: {request.WinAmount}");
            return EmptyResult;
        }

        public override Empty RecoveryPoint(RecoveryPointNotification request)
        {
            Logger.Debug("Received Recovery Point");

            _handlerFactory.Create<AddRecoveryDataPoint>()
                .Handle(new AddRecoveryDataPoint(request.Data.ToByteArray()));

            return EmptyResult;
        }

        public override GetLocalStorageResponse GetLocalStorage(
            GetLocalStorageRequest request)
        {
            Logger.Debug("GetLocalStorage called");

            var storage = new GetLocalStorage();

            _handlerFactory.Create<GetLocalStorage>()
                .Handle(storage);

            var response = new GetLocalStorageResponse();

            foreach (var value in storage.Values)
            {
                var data = new LocalStorage { Location = (StorageLocation)value.Key };

                data.Data.Add(value.Value);

                response.StorageData.Add(data);
            }

            return response;
        }

        public override Empty UpdateLocalStorage(UpdateLocalStorageRequest request)
        {
            Logger.Debug("SetLocalStorage called");

            var values = request.StorageData
                .ToDictionary<LocalStorage, StorageType, IDictionary<string, string>>(
                    data => (StorageType)data.Location,
                    data => new Dictionary<string, string>(data.Data));

            var storage = new SetLocalStorage { Values = values };

            _handlerFactory.Create<SetLocalStorage>()
                .Handle(storage);

            return EmptyResult;
        }

        public override GetMetersResponse GetMeters(Empty request)
        {
            // Replay gets all of it's data from the blob
            var response = new GetMetersResponse();

            if (_gameDiagnostics.IsActive)
            {
                return response;
            }

            var command = new GetInGameMeters();

            _handlerFactory.Create<GetInGameMeters>()
                .Handle(command);

            Logger.Debug(
                $"GetMeters meters: {string.Join(",", command.MeterValues.Keys)} with values: {string.Join(",", command.MeterValues.Values)}");

            response.Meters.Add(command.MeterValues);

            return response;
        }

        public override Empty UpdateMeters(UpdateMetersRequest request)
        {
            if (_gameDiagnostics.IsActive)
            {
                return EmptyResult;
            }

            var meters = new Dictionary<string, ulong>(request.Meters);

            Logger.Debug(
                $"SetMeters meters: {string.Join(",", meters.Keys)} with values: {string.Join(",", meters.Values)}");

            _handlerFactory.Create<SetInGameMeters>()
                .Handle(new SetInGameMeters(meters));

            return EmptyResult;
        }

        public override GetRandomNumber32Response GetRandomNumber32(GetRandomNumber32Request request)
        {
            Logger.Debug($"GetRandomNumberU64({request.Range})");

            var command = new GetRandomNumber(request.Range);
            _getRandomNumber.Handle(command);

            return new GetRandomNumber32Response { Value = (uint)command.Value };
        }

        public override GetRandomNumber64Response GetRandomNumber64(GetRandomNumber64Request request)
        {
            Logger.Debug($"GetRandomNumberU64({request.Range})");

            var command = new GetRandomNumber(request.Range);
            _getRandomNumber.Handle(command);

            return new GetRandomNumber64Response { Value = command.Value };
        }

        public override ShuffleResponse Shuffle(ShuffleRequest request)
        {
            var command = new Shuffle(new List<ulong>(request.Value));

            _shuffle.Handle(command);

            var response = new ShuffleResponse();

            response.Value.Add(command.Values);

            return response;
        }

        public override Empty UpdateBonusKey(UpdateBonusKeyRequest request)
        {
            Logger.Debug($"SetBonusKey poolName: {request.PoolName} key: {request.Key}");

            var bonusKey = new SetBonusKey(request.PoolName, request.Key);

            _handlerFactory.Create<SetBonusKey>()
                .Handle(bonusKey);

            return EmptyResult;
        }

        public override Empty EndGameRound(EndGameRoundRequest request)
        {
            Logger.Debug($"EndGameRound: {request.BetAmount} {request.WinAmount}");

            // Replay gets all data from blob.  Do not record events etc.
            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<SetGameResult>()
                    .Handle(new SetGameResult((long)request.WinAmount));
            }

            return EmptyResult;
        }

        public override ConnectJackpotPoolResponse ConnectJackpotPool(ConnectJackpotPoolRequest request)
        {
            Logger.Debug($"ConnectJackpotPool {request.PoolName}");

            // Do not filter this for replay or recovery. The command will sort out what to do
            var command = new ConnectJackpotPool(request.PoolName);
            _handlerFactory.Create<ConnectJackpotPool>()
                .Handle(command);

            return new ConnectJackpotPoolResponse { Connected = command.Connected };
        }

        public override LevelInfoResponse GetJackpotValues(GetJackpotValuesRequest request)
        {
            var response = new LevelInfoResponse();
            GetJackpotValues command;

            if (!string.IsNullOrEmpty(request.GameName))
            {
                command = new GetJackpotValues(request.PoolName, false, request.GameName, request.Denomination);

                Logger.Debug($"GetJackpotValuesPerDenom Response poolName:{request.PoolName} ");
            }
            else
            {
                command = new GetJackpotValues(
                    request.PoolName,
                    request.PlayMode == GameRoundPlayMode.ModeRecovery ||
                    request.PlayMode == GameRoundPlayMode.ModeReplay);
            }

            _handlerFactory.Create<GetJackpotValues>().Handle(command);

            response.LevelInfo.Add(
                command.JackpotValues.Select(
                    r => new LevelInfo { LevelId = (uint)r.Key, Value = (ulong)r.Value.MillicentsToCents() }));

            return response;
        }

        public override Empty UpdateJackpotValues(UpdateJackpotValuesRequest request)
        {
            Logger.Debug(
                $"IncrementJackpotValues - mode: {request.PlayMode} name: {request.PoolName} values: {request.PoolValues.Count}");

            var values = request.PoolValues.ToDictionary(
                v => v.LevelId,
                v => new PoolIncrement { Cents = v.Cents, Fraction = v.Fraction });

            var incrementJackpotValues = new IncrementJackpotValues(
                request.PoolName,
                values,
                request.PlayMode == GameRoundPlayMode.ModeRecovery || request.PlayMode == GameRoundPlayMode.ModeReplay);

            _handlerFactory.Create<IncrementJackpotValues>()
                .Handle(incrementJackpotValues);

            return EmptyResult;
        }

        public override TriggerJackpotResponse TriggerJackpot(TriggerJackpotRequest request)
        {
            Logger.Debug(
                $"TriggerJackpot mode: {request.Mode} poolName: {request.PoolName} levels: {string.Join(",", request.Levels)} transactionIds:{string.Join(",", request.TransactionIds)}");

            var trigger = new TriggerJackpot(
                request.PoolName,
                request.Levels.Select(i => (int)i).ToList(),
                request.TransactionIds.Select(i => (long)i).ToList(),
                request.Mode == GameRoundPlayMode.ModeRecovery || request.Mode == GameRoundPlayMode.ModeReplay);

            _handlerFactory.Create<TriggerJackpot>()
                .Handle(trigger);

            var results = trigger.Results.ToDictionary(key => (uint)key.LevelId, value => (ulong)value.TransactionId);

            Logger.Debug(
                $"TriggerJackpot Response poolName:{request.PoolName} levels: {string.Join(",", request.Levels)} transactionIds:{string.Join(",", results.Values)}");

            var response = new TriggerJackpotResponse();

            response.Trigger.Add(results);

            return response;
        }

        public override LevelInfoResponse ClaimJackpot(ClaimJackpotRequest request)
        {
            Logger.Debug($"ClaimJackpot mode: {request.PlayMode} poolName:{request.PoolName} transactionIds:{string.Join(",", request.TransactionIds)}");

            var claim = new ClaimJackpot(
                request.PoolName,
                request.TransactionIds.Select(i => (long)i).ToList());

            _handlerFactory.Create<ClaimJackpot>()
                .Handle(claim);

            var response = new LevelInfoResponse();

            response.LevelInfo.Add(
                claim.Results.Select(
                    r => new LevelInfo { LevelId = (uint)r.LevelId, Value = (ulong)r.WinAmount.MillicentsToCents() }));

            Logger.Debug(
                $"ClaimJackpot Response poolName:{request.PoolName} transactionIds:{string.Join(",", request.TransactionIds)} amounts:{string.Join(",", claim.Results)}");

            return response;
        }

        public override Empty SetJackpotLevelWagers(LevelWagerRequest request)
        {
            Logger.Debug($"SetJackpotWager wagers:{string.Join(",", request.Wagers)}");

            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<ProgressiveLevelWagers>()
                    .Handle(new ProgressiveLevelWagers(request.Wagers.Select(x => (long)x)));
            }

            return EmptyResult;
        }

        public override Empty SelectDenomination(SelectDenominationRequest request)
        {
            Logger.Debug($"Select Denomination with denom: {request.Denomination}");

            var selectDenom = new SelectDenomination((long)request.Denomination);

            _handlerFactory.Create<SelectDenomination>().Handle(selectDenom);

            return EmptyResult;
        }

        public override Empty UpdateLanguage(LanguageRequest request)
        {
            throw new NotImplementedException();
        }

        public override Empty UpdateBetOptions(UpdateBetOptionsRequest request)
        {
            Logger.Debug($"Update Bet Line option with wager : {request.Wager}");

            var betOptions = new UpdateBetOptions(
                (long)request.Wager,
                (long)request.StakeAmount,
                (int)request.BetMultiplier,
                (int)request.LineCost,
                (int)request.NumberLines,
                (int)request.Ante,
                (int)request.BetLinePresetId);

            _handlerFactory.Create<UpdateBetOptions>().Handle(betOptions);

            return EmptyResult;
        }

        public override CheckMysteryJackpotResponse CheckMysteryJackpot(CheckMysteryJackpotRequest request)
        {
            var command = new CheckMysteryJackpot();

            _handlerFactory.Create<CheckMysteryJackpot>()
                .Handle(command);
            var response = new CheckMysteryJackpotResponse();

            response.Levels.Add(command.Results);

            return response;
        }

        //public override LevelInfoResponse GetJackpotValuesPerDenom(GetJackpotValuesPerDenomRequest request)
        //{
        //    var command = new GetJackpotValuesPerDenom(request.GameName, request.PackName, request.Denomination);
        //    // Add Jackpot Values and Extract into LevelInfoResponse
        //    _handlerFactory.Create<GetJackpotValuesPerDenom>().Handle(command);

        //    var response = new LevelInfoResponse();

        //    response.LevelInfo.Add(
        //        command.JackpotValues.Select(
        //            r => new LevelInfo { LevelId = (uint)r.Key, Value = (ulong)r.Value.MillicentsToCents() }));

        //    Logger.Debug(
        //        $"GetJackpotValuesPerDenom Response poolName:{request.PackName} ");

        //    return response;
        //}
    }
}