namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Exceptions;
    using Hardware.Contracts.Button;
    using Kernel;
    using Protocol.Common.Logging;
    using Storage.Helpers;
    using log4net;

    /// <inheritdoc />
    public class GameRecoveryService : IGameRecoveryService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICentralManager _centralManager;
        private readonly IEventBus _eventBus;
        private readonly IGameDataService _gameDataService;
        private readonly IGamePlayEntityHelper _gamePlayEntityHelper;
        private readonly ISystemDisableManager _systemDisableManager;

        public GameRecoveryService(
            ICentralManager centralManager,
            IGameDataService gameDataService,
            IGamePlayEntityHelper gamePlayEntityHelper,
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager)
        {
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
            _gamePlayEntityHelper =
                gamePlayEntityHelper ?? throw new ArgumentNullException(nameof(gamePlayEntityHelper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _eventBus.Subscribe<DownEvent>(
                this,
                Handle,
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30 &&
                       _systemDisableManager.CurrentDisableKeys.Contains(HhrConstants.GamePlayRequestFailedKey));

            if (_gamePlayEntityHelper.GamePlayRequestFailed)
            {
                _eventBus.Publish(new GamePlayRequestFailedEvent());
            }
        }

        /// <inheritdoc />
        public async Task<GamePlayResponse> Recover(CancellationToken token = default)
        {
            var gamePlayRequest = _gamePlayEntityHelper.GamePlayRequest;
            var raceStartRequest = _gamePlayEntityHelper.RaceStartRequest;

            if (gamePlayRequest == null && raceStartRequest == null)
            {
                throw new GameRecoveryFailedException("Game request not found");
            }

            // We only need four things to request/process recovery message. Populate them from game play request or Race start request
            var (sequenceId, gameId, numberOfCredits, lines) = FetchValues();
            Logger.Debug($"Recovering with values: seq={sequenceId}, game={gameId}, credits={numberOfCredits}, lines={lines}");

            var gamePlayResponse = _gamePlayEntityHelper.GamePlayResponse;
            Logger.Debug($"Checking game play response: {gamePlayResponse.ToJson()}");

            if (gamePlayResponse != null && sequenceId == gamePlayResponse.ReplyId)
            {
                return gamePlayResponse;
            }

            GameRecoveryResponse gameRecoveryResponse;
            try
            {
                gameRecoveryResponse = await SendGameRecoveryMessage();
            }
            catch (UnexpectedResponseException e)
            {
                Logger.Error("Unexpected response for GameRecovery received.", e);
                _gamePlayEntityHelper.GamePlayRequestFailed = true;
                _eventBus.Publish(new GamePlayRequestFailedEvent());
                throw;
            }

            Logger.Debug($"Got game recovery response: {gameRecoveryResponse.ToJson()}");

            if (gameRecoveryResponse.GameId != 0)
            {
                return await PopulateGamePlayResponseFromRecoveryResponse();
            }

            _eventBus.Publish(new GamePlayRequestFailedEvent());
            throw new GameRecoveryFailedException(
                "Empty response for GameRecovery. Server doesn't have any information about this game. Aborting.");

            async Task<GamePlayResponse> PopulateGamePlayResponseFromRecoveryResponse()
            {
                var prize = "P=0";
                var racePatterns = await _gameDataService.GetRacePatterns(
                    gameId,
                    numberOfCredits,
                    lines);

                if (gameRecoveryResponse.PrizeLoc1 != 0)
                {
                    var prizeLocationIndex = await _gameDataService.GetPrizeLocationForAPattern(
                        gameId,
                        numberOfCredits,
                        (int)(gameRecoveryResponse.PrizeLoc1 - 1));

                    Logger.Debug($"Got PrizeLoc1 index: {prizeLocationIndex}");

                    prize = racePatterns.Pattern[prizeLocationIndex].Prize;
                }

                if (gameRecoveryResponse.PrizeLoc2 != 0)
                {
                    var prizeLocationIndex = await _gameDataService.GetPrizeLocationForAPattern(
                        gameId,
                        numberOfCredits,
                        (int)(gameRecoveryResponse.PrizeLoc2 - 1));

                    Logger.Debug($"Got PrizeLoc2 index: {prizeLocationIndex}");

                    prize = racePatterns.Pattern[prizeLocationIndex].Prize;
                }

                Logger.Debug($"Got prize string: {prize}");

                return new GamePlayResponse
                {
                    Prize = prize,
                    RaceInfo = gameRecoveryResponse.RaceInfo,
                    LastGamePlayTime = gameRecoveryResponse.LastGamePlayTime,
                    ScratchTicketId = gameRecoveryResponse.RaceTicketId,
                    ScratchTicketSetId = gameRecoveryResponse.RaceTicketSetId,
                    // NOTE: Don't set the ReplyId here as having zero is how we detect recovery
                    // messages in other logic and you'll break that.
                    ReplyId = 0
                };
            }

            async Task<GameRecoveryResponse> SendGameRecoveryMessage()
            {
                return await _centralManager.Send<GameRecoveryRequest, GameRecoveryResponse>(
                    new GameRecoveryRequest
                    {
                        GameNo = sequenceId,
                        TimeoutInMilliseconds = HhrConstants.GamePlayRequestTimeout,
                        RetryCount = int.MaxValue
                    },
                    token);
            }

            (uint sequenceId, uint gameId, uint creditsPlayed, uint linesPlayed) FetchValues()
            {
                if (gamePlayRequest != null)
                {
                    return (gamePlayRequest.SequenceId, gamePlayRequest.GameId, gamePlayRequest.CreditsPlayed,
                        gamePlayRequest.LinesPlayed);
                }

                return (raceStartRequest.SequenceId, raceStartRequest.GameId, raceStartRequest.CreditsPlayed,
                    raceStartRequest.LinesPlayed);
            }
        }

        private void Handle(DownEvent obj)
        {
            _systemDisableManager.Enable(HhrConstants.GamePlayRequestFailedKey);
            _gamePlayEntityHelper.GamePlayRequestFailed = false;
        }
    }
}