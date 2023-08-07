namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Kernel;
    using log4net;
    using Progressive;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     Implementation of <see cref="IGameDataService" />
    /// </summary>
    public class GameDataService : IGameDataService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICentralManager _centralManager;
        private readonly List<GameInfoResponse> _gameData = new List<GameInfoResponse>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly List<ProgressiveInfoResponse> _progressiveInfo = new List<ProgressiveInfoResponse>();
        private readonly SemaphoreSlim _progressiveInfoLock = new SemaphoreSlim(1);
        private readonly IEventBus _eventBus;
        private readonly IProgressiveAssociation _progressiveAssociation;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        private ParameterResponse _parameterResponse;
        private bool _disposed;
        private const int InvalidValue = -1;

        /// <summary>
        /// </summary>
        /// <param name="centralManager"></param>
        /// <param name="propertiesManager"></param>
        /// <param name="progressiveAssociation"></param>
        /// <param name="eventBus"></param>
        /// <param name="protocolLinkedProgressiveAdapter"></param>
        public GameDataService(
            ICentralManager centralManager,
            IPropertiesManager propertiesManager,
            IProgressiveAssociation progressiveAssociation,
            IEventBus eventBus,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveAssociation =
                progressiveAssociation ?? throw new ArgumentNullException(nameof(progressiveAssociation));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<GameInfoResponse>> GetGameInfo(bool reinitializeData = false)
        {
            if (reinitializeData)
            {
                _gameData.Clear();
            }

            await PopulateGameInfo();
            return _gameData;
        }

        /// <inheritdoc />
        public async Task<ParameterResponse> GetGameParameters(bool reinitializeData = false)
        {
            if (reinitializeData)
            {
                _parameterResponse = null;
            }

            await PopulateParameter();
            return _parameterResponse;
        }

        /// <inheritdoc />
        public async Task<RacePariResponse> GetRaceInformation(uint gameId, uint creditsPlayed, uint linesPlayed)
        {
            // Set up a request for race information. We do not retry, unless the server responds telling us to do so.
            var request = new RacePariRequest
            {
                GameId = gameId, CreditsPlayed = creditsPlayed, LinesPlayed = linesPlayed
            };

            try
            {
                // Ask the central server for the race info.
                var response = await _centralManager.Send<RacePariRequest, RacePariResponse>(request);
                return response;
            }
            catch (UnexpectedResponseException ex)
            {
                // If the response isn't forthcoming, we will just log the problem and return null.
                Logger.Warn(
                    $"Got incorrect response to race info request of type {ex.Response.GetType()} with status {ex.Response.MessageStatus}",
                    ex);

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProgressiveInfoResponse>> GetProgressiveInfo(bool reinitializeData = false)
        {
            if (reinitializeData)
            {
                _progressiveInfo.Clear();
            }

            await FetchProgressiveInfo();
            return _progressiveInfo;
        }

        /// <inheritdoc />
        public async Task<GameInfoResponse> GetGameOpen(uint gameId)
        {
            return (await GetGameInfo()).FirstOrDefault(go => go.GameId == gameId);
        }

        /// <inheritdoc />
        public async Task<CRacePatterns> GetRacePatterns(uint gameId, uint numberOfCredits, uint totalLinesPlayed)
        {
            return (await GetGameOpen(gameId)).RaceTicketSets.TicketSet.FirstOrDefault(
                ts =>
                    ts.Credits == numberOfCredits && ts.Line == totalLinesPlayed);
        }

        public async Task<int> GetPrizeLocationForAPattern(uint gameId, uint credits, int patternIndex)
        {
            if (patternIndex >= MessageLengthConstants.MaxNumPatterns)
            {
                Logger.Error($"patternIndex passed {patternIndex} cannot be more than {MessageLengthConstants.MaxNumPatterns}");
                return InvalidValue;
            }

            var gameInfoResponse = await GetGameOpen(gameId);
            var ticketSetIndex = Array.FindIndex(gameInfoResponse.RaceTicketSets.TicketSet, x => x.Credits == credits);

            if (ticketSetIndex != InvalidValue)
            {
                var prizeLoc = gameInfoResponse.PrizeLocations[ticketSetIndex][patternIndex];
                Logger.Debug($"Got prize location [{ticketSetIndex}][{patternIndex}] as {prizeLoc}");
                return prizeLoc;
            }

            Logger.Error($"Ticket Set doesn't exist for the parameters passed");
            return InvalidValue;
        }

        /// <summary>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _lock.Dispose();
                _progressiveInfoLock.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private async Task PopulateGameInfo()
        {
            await _lock.WaitAsync();
            try
            {
                if (_gameData.Any())
                {
                    return;
                }

                await PopulateParameter();

                if (_parameterResponse == null)
                {
                    return;
                }

                for (var index = 0; index < _parameterResponse.GameIdCount; index++)
                {
                    var request = new GameInfoRequest
                    {
                        GameId = _parameterResponse.GameIds[index],
                        TimeoutInMilliseconds = HhrConstants.StartupMessageTimeouts
                    };

                    var responseFromServer =
                        await _centralManager.Send<GameInfoRequest, GameInfoResponse>(request, CancellationToken.None);
                    responseFromServer.PrizeLocations = new int[MessageLengthConstants.MaxNumTickets][];

                    var raceTicketIndex = 0;
                    foreach (var patternSet in responseFromServer.RaceTicketSets.TicketSet.Where(x => x.RaceTicketSetId != 0))
                    {
                        var patternIndex = 0;
                        responseFromServer.PrizeLocations[raceTicketIndex] = new int[MessageLengthConstants.MaxNumPatterns];
                        foreach (var pattern in patternSet.Pattern)
                        {
                            var locationString = pattern.Prize.GetPrizeString(HhrConstants.PrizeLocation);
                            var locationValue = locationString != string.Empty ? Convert.ToInt32(locationString) : InvalidValue;
                            responseFromServer.PrizeLocations[raceTicketIndex][patternIndex] = locationValue;
                            ++patternIndex;
                        }

                        ++raceTicketIndex;
                    }

                    _gameData.Add(responseFromServer);
                }
            }
            catch (UnexpectedResponseException exception)
            {
                // Any other response, may be we lost connection, we clear and return empty list. Caller should attempt to get game data again.
                Logger.Warn(
                    $"Failed to fetch GameInfo response - Expected {typeof(GameInfoResponse)}, received {exception.Response.GetType()}, MessageStatus - {exception.Response.MessageStatus}",
                    exception);
                _gameData.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task PopulateParameter()
        {
            if (_parameterResponse != null)
            {
                return;
            }

            var request = new ParameterRequest
            {
                SerialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                TimeoutInMilliseconds = HhrConstants.StartupMessageTimeouts
            };

            try
            {
                _parameterResponse =
                    await _centralManager.Send<ParameterRequest, ParameterResponse>(request, CancellationToken.None);
                Logger.Debug("Received response for Parameters and GameData.");
            }
            catch (UnexpectedResponseException e)
            {
                Logger.Warn(
                    $"Failed to fetch GameInfo response - Expected {typeof(ParameterResponse)}, received {e.Response.GetType()}",
                    e);
            }
        }

        private async Task FetchProgressiveInfo()
        {
            await _progressiveInfoLock.WaitAsync();

            try
            {
                if (_progressiveInfo.Any())
                {
                    Logger.Debug("Already initialized");
                    _propertiesManager.SetProperty(ApplicationConstants.WaitForProgressiveInitialization, false);

                    return;
                }

                await PopulateGameInfo();

                if (!_gameData.Any())
                {
                    Logger.Debug("Game data not available");

                    return;
                }

                var failure = false;

                Logger.Debug("Initializing Progressives");

                foreach (var game in _gameData)
                {
                    var levelAssignments = new List<ProgressiveLevelAssignment>();
                    foreach (var progId in game.ProgressiveIds.Where(x => x != 0))
                    {
                       var request = new ProgressiveInfoRequest { ProgressiveId = progId };

                        var progressiveInfo =
                            await _centralManager.Send<ProgressiveInfoRequest, ProgressiveInfoResponse>(
                                request,
                                CancellationToken.None);

                        if (await _progressiveAssociation.AssociateServerLevelsToGame(progressiveInfo, game, levelAssignments))
                        {
                            _progressiveInfo.Add(progressiveInfo);
                            continue;
                        }

                        failure = true;
                    }

                    if (!failure)
                    {
                        _protocolLinkedProgressiveAdapter.AssignLevelsToGame(
                            levelAssignments, ProtocolNames.HHR);
                    }
                }

                if (failure)
                {
                    Logger.Warn("Progressive initialization failed");

                    _eventBus.Publish(new ProgressiveInitializationFailed());
                }
                else
                {
                    _propertiesManager.SetProperty(ApplicationConstants.WaitForProgressiveInitialization, false);
                }
            }
            catch (UnexpectedResponseException exception)
            {
                Logger.Warn(
                    $"Failed to fetch Progressive Info response - Expected {typeof(ProgressiveInfoResponse)}, received {exception.Response?.GetType()}, MessageStatus - {exception.Response?.MessageStatus}",
                    exception);
                _progressiveInfo.Clear();
            }
            finally
            {
                _progressiveInfoLock.Release();
            }
        }
    }
}