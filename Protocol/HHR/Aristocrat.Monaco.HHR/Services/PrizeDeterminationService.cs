namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Exceptions;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Button;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using log4net;
    using Protocol.Common.Logging;
    using Storage.Helpers;

    /// <summary>
    /// Implementation of IPrizeDeterminationService
    /// </summary>
    public class PrizeDeterminationService : IPrizeDeterminationService, IDisposable
    {
        private const int ForceRecovery = 0;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICentralManager _centralManager;
        private readonly IEventBus _eventBus;
        private readonly IGameDataService _gameDataService;
        private readonly IPlayerSessionService _playerSession;
        private readonly IPropertiesManager _propertiesManager;
        private readonly PrizeCalculator _prizeCalculator;
        private readonly IGameRecoveryService _gameRecoveryService;
        private readonly IGamePlayEntityHelper _gamePlayEntityHelper;
        private readonly IPrizeInformationEntityHelper _prizeInformationEntityHelper;
        private readonly ISystemDisableManager _systemDisableManager;
        private IReadOnlyCollection<string> _manualHandicapPicks;
        private GamePlayResponse _gamePlayResponse;
        private CRacePatterns _racePatterns;
        private bool _manualHandicapInProgress;
        private bool _recoveryInProgress;

        private CRaceInfo? _currentRaceInfo;
        private uint _totalLinesPlayed;
        private bool _disposed;

        /// <summary>
        /// </summary>
        /// <param name="eventBus"></param>
        /// <param name="centralManager"></param>
        /// <param name="playerSession"></param>
        /// <param name="gameDataService"></param>
        /// <param name="protocolLinkedProgressiveAdapter"></param>
        /// <param name="propertiesManager"></param>
        /// <param name="gameRecoveryService"></param>
        /// <param name="gamePlayEntityHelper"></param>
        /// <param name="prizeInformationEntityHelper"></param>
        /// <param name="systemDisableManager"></param>
        /// <param name="gameHistory"></param>
        public PrizeDeterminationService(
            IEventBus eventBus,
            ICentralManager centralManager,
            IPlayerSessionService playerSession,
            IGameDataService gameDataService,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IPropertiesManager propertiesManager,
            IGameRecoveryService gameRecoveryService,
            IGamePlayEntityHelper gamePlayEntityHelper,
            IPrizeInformationEntityHelper prizeInformationEntityHelper,
            ISystemDisableManager systemDisableManager,
            IGameHistory gameHistory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _prizeCalculator = new PrizeCalculator(protocolLinkedProgressiveAdapter, eventBus);
            _gameRecoveryService = gameRecoveryService ?? throw new ArgumentNullException(nameof(gameRecoveryService));
            _gamePlayEntityHelper =
                gamePlayEntityHelper ?? throw new ArgumentNullException(nameof(gamePlayEntityHelper));
            _prizeInformationEntityHelper = prizeInformationEntityHelper ??
                                           throw new ArgumentNullException(nameof(prizeInformationEntityHelper));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _centralManager.RequestModifiedHandler += SendingNewRequest;

            _eventBus.Subscribe<DownEvent>(this,
                _ =>
                {
                    _gamePlayEntityHelper.ManualHandicapWin = false;
                    systemDisableManager.Enable(HhrConstants.ManualHandicapWinKey);
                }, evt => evt.LogicalId == (int)ButtonLogicalId.Button30 && _systemDisableManager.CurrentDisableKeys.Contains(HhrConstants.ManualHandicapWinKey));

            _eventBus.Subscribe<GamePlayStateChangedEvent>(this,
                _ => ManualHandicapWinDisable(), evt => evt.CurrentState == PlayState.GameEnded);

            if (!gameHistory.IsRecoveryNeeded)
            {
                ManualHandicapWinDisable();
            }
        }
        
        /// <inheritdoc />
        public async Task<PrizeInformation> DeterminePrize(
            uint gameId,
            uint numberOfCredits,
            uint denomination,
            long transactionId,
            bool isRecovering = false,
            CancellationToken token = default)
        {
            _currentRaceInfo = null;
            SetRacePatterns(gameId, numberOfCredits);
            _recoveryInProgress = isRecovering;

            try
            {
                if (isRecovering)
                {
                    _gamePlayResponse = await _gameRecoveryService.Recover(ForceRecovery, token);
                }
                else
                {
                    if (_manualHandicapInProgress)
                    {
                        Logger.Debug("Manual handicap GamePlay");

                        PopulateRaceData();
                        _prizeCalculator.Calculate(_gamePlayResponse, _racePatterns);

                        Logger.Debug(
                            $"Manual handicap race data (Before Race Start) : {_gamePlayResponse.RaceInfo.RaceData.ToJson()}");

                        var isOverriden = _gamePlayResponse.BOverride;

                        _gamePlayResponse = await SendRaceStart();

                        // If we decided to recover (e.g. because of a game restart) while we were waiting for
                        // this message, then throw the response away and wait for recovery to finish.
                        if (_recoveryInProgress)
                        {
                            throw new IgnoreOutcomesException("Ignoring response as recovery has started (manual handicap)");
                        }

                        _gamePlayResponse.BOverride = isOverriden;

                        Logger.Debug(
                            $"Manual handicap race data (After Race Start) : {_gamePlayResponse.RaceInfo.RaceData.ToJson()}");
                    }
                    else
                    {
                        Logger.Debug("Non-Manual handicap GamePlay");

                        _gamePlayResponse = await SendGamePlay(gameId, false, token);

                        // If we decided to recover (e.g. because of a game restart) while we were waiting for
                        // this message, then throw the response away and wait for recovery to finish.
                        if (_recoveryInProgress)
                        {
                            throw new IgnoreOutcomesException("Ignoring response as recovery has started (auto handicap)");
                        }

                        Logger.Debug($"Non-Manual handicap race data : {_gamePlayResponse.RaceInfo.RaceData.ToJson()}");
                    }

                    if (_manualHandicapInProgress)
                    {
                        Logger.Debug($"Manual Handicap selection : [{_manualHandicapPicks}]");
                    }
                }

                PopulateRaceData(isRecovering);

                var gamePlaySequenceMatch = _gamePlayEntityHelper.GamePlayRequest != null &&
                                            _gamePlayResponse.ReplyId == _gamePlayEntityHelper.GamePlayRequest.SequenceId;
                var raceStartSequenceMatch = _gamePlayEntityHelper.RaceStartRequest != null &&
                                            _gamePlayResponse.ReplyId == _gamePlayEntityHelper.RaceStartRequest.SequenceId;
                var isRecoveryResponse = _gamePlayResponse.ReplyId == 0;

                // If we are not recovering, then ignore any game response that doesn't match our request,
                // except for recovery responses (where the game request timed out and we sent for recovery).
                // If we were recovering, we would have bailed out above after finding that _recoveryInProgress
                // is true.
                if (!isRecovering)
                {
                    if (!gamePlaySequenceMatch && !raceStartSequenceMatch && !isRecoveryResponse)
                    {
                        throw new IgnoreOutcomesException(
                            "Ignoring response as it did not match nor was it a recovery:"
                            + $" {_gamePlayResponse.ReplyId}"
                            + $" {_gamePlayEntityHelper.GamePlayRequest?.SequenceId}"
                            + $" {_gamePlayEntityHelper.RaceStartRequest?.SequenceId}");
                    }
                }

                _gamePlayEntityHelper.GamePlayResponse = _gamePlayResponse;
                _recoveryInProgress = false;

                var prizeInformation = _prizeCalculator.Calculate(_gamePlayResponse, _racePatterns);
                prizeInformation.GameMapId = gameId;
                prizeInformation.TransactionId = transactionId;
                prizeInformation.Denomination = denomination;
                prizeInformation.Outcomes = prizeInformation.ExtractOutcomes();
                prizeInformation.GameRoundInfo = prizeInformation.ExtractGameRoundInfo();

                // If manual handicap, than any win should result into lockup after game play.
                if (_gamePlayResponse.RaceInfo.HandicapData == 1 && (prizeInformation.RaceSet1AmountWon > 0 || prizeInformation.RaceSet2AmountWon > 0))
                {
                    _gamePlayEntityHelper.ManualHandicapWin = true;
                    _eventBus.Publish(new ManualHandicapWinEvent());
                }

                _prizeInformationEntityHelper.PrizeInformation = prizeInformation;

                _eventBus.Publish(new PrizeInformationEvent(prizeInformation));

                if (_manualHandicapInProgress)
                {
                    Logger.Debug($"Manual Handicap selection : [{_manualHandicapPicks}]");
                }

                Logger.Debug($"Prize information : [{prizeInformation.ToJson()}]");

                FinishGameTransaction();

                return prizeInformation;
            }
            catch (UnexpectedResponseException e)
            {
                Logger.Error(
                    $"Got incorrect response, to GamePlay request, of type {e.Response.GetType()} with status {e.Response.MessageStatus} : {e.Message}",
                    e);
                throw;
            }
            catch (PrizeCalculationException e)
            {
                Logger.Error($"Prize Match Error : {e.Message}", e);
                throw;
            }
            catch (GameRecoveryFailedException e)
            {
                Logger.Error($"Unable to recover game - {e.Message}");
                throw;
            }
            finally
            {
                _manualHandicapInProgress = false;
            }

            async Task<GamePlayResponse> SendRaceStart()
            {
                var raceStartRequest = new RaceStartRequest
                {
                    GameId = gameId,
                    PlayerId = await _playerSession.GetCurrentPlayerId(),
                    CreditsPlayed = (ushort)numberOfCredits,
                    LinesPlayed = (ushort)_totalLinesPlayed,
                    RaceInfo = _gamePlayResponse.RaceInfo,
                    RaceTicketId = _gamePlayResponse.ScratchTicketId,
                    RaceTicketSetId = _gamePlayResponse.ScratchTicketSetId,
                    TimeoutInMilliseconds = HhrConstants.GamePlayRequestTimeout
                };

                try
                {
                    return await _centralManager.Send<RaceStartRequest, GamePlayResponse>(raceStartRequest, token);
                }
                catch (UnexpectedResponseException e)
                {
                    Logger.Debug($"Got incorrect response to RaceStartRequest, of type {e.Response.GetType()} with status {e.Response.MessageStatus} : {e.Message}");
                    Logger.Debug($"Attempting recovery from server for failed request {raceStartRequest.SequenceId}");
                    return await _gameRecoveryService.Recover(raceStartRequest.SequenceId, token);
                }
            }
        }

        /// <inheritdoc />
        public void SetHandicapPicks(IReadOnlyCollection<string> picks)
        {
            _manualHandicapPicks = picks;
        }

        /// <inheritdoc />
        public async Task<CRaceInfo?> RequestRaceInfo(uint gameId, uint numberOfCredits)
        {
            if (_currentRaceInfo != null)
            {
                return _currentRaceInfo;
            }

            try
            {
                SetRacePatterns(gameId, numberOfCredits);

                _gamePlayResponse = await SendGamePlay(gameId, true, CancellationToken.None);
                _manualHandicapInProgress = true;
                _currentRaceInfo = _gamePlayResponse.RaceInfo;
                Logger.Debug($"Received Race Info : [{_gamePlayResponse.RaceInfo.ToJson2()}]");
            }
            catch (Exception ex)
            {
                Logger.Error($"Manual Handicap GamePlay failed : {ex.Message}", ex);

                _eventBus.Publish(new ManualHandicapAbortedEvent());
            }

            return _currentRaceInfo;
        }

        /// <inheritdoc />
        public void ClearManualHandicapData()
        {
            _manualHandicapInProgress = false;
            _manualHandicapPicks = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose Pattern to prevent reentry
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
                if (_centralManager != null)
                {
                    _centralManager.RequestModifiedHandler -= SendingNewRequest;
                }
            }

            _disposed = true;
        }

        private async void SetRacePatterns(uint gameId, uint numberOfCredits)
        {
            _totalLinesPlayed = (await _gameDataService.GetGameOpen(gameId)).MaxLines;
            _racePatterns = await _gameDataService.GetRacePatterns(gameId, numberOfCredits, _totalLinesPlayed);
        }

        private void SendingNewRequest(object obj, Request request)
        {
            //Not a valid request if sequence Id is 0
            if (request.SequenceId == 0)
            {
                return;
            }

            switch (request)
            {
                case GamePlayRequest gamePlayRequest:
                    // If not manual handicap, than only we preserve previous request.
                    if (!gamePlayRequest.ForceCheck)
                    {
                        _gamePlayEntityHelper.GamePlayRequest = gamePlayRequest;
                    }

                    break;
                case RaceStartRequest raceStartRequest:
                    _gamePlayEntityHelper.RaceStartRequest = raceStartRequest;
                    break;
            }

        }

        private void PopulateRaceData(bool isRecovering = false)
        {
            if (_gamePlayResponse != null)
            {
                _gamePlayResponse.RaceInfo.RaceData = _gamePlayResponse.RaceInfo.RaceData.Select(
                    (x, index) =>
                    {
                        if (_gamePlayResponse.RaceInfo.HandicapData == HhrConstants.ManualHandicap ||
                            _gamePlayResponse.HandicapEnter == HhrConstants.ManualHandicap)
                        {
                            x.HorseSelection = isRecovering ? _gamePlayEntityHelper.RaceStartRequest.RaceInfo.RaceData[index].HorseSelection : _manualHandicapPicks.ElementAt(index);
                        }
                        else
                        {
                            x.HorseSelection = x.HorseOdds;
                        }

                        return x;
                    }).ToArray();
            }
        }

        private void FinishGameTransaction()
        {
            _manualHandicapInProgress = false;
            _propertiesManager.SetProperty(HHRPropertyNames.LastGamePlayTime, _gamePlayResponse.LastGamePlayTime);
        }

        private async Task<GamePlayResponse> SendGamePlay(
            uint gameId,
            bool manualHandicap,
            CancellationToken token = default)
        {
            var gamePlayRequest = new GamePlayRequest
            {
                PlayerId = await _playerSession.GetCurrentPlayerId(),
                GameId = gameId,
                CreditsPlayed = (ushort)_racePatterns.Credits,
                ForceCheck = manualHandicap, // 0,false = regular(auto) game | 1,true=Handicap Game
                LinesPlayed = (ushort)_totalLinesPlayed,
                GameMode = HhrConstants.GameMode,
                RaceTicketSetId = _racePatterns.RaceTicketSetId,
                TimeoutInMilliseconds = HhrConstants.GamePlayRequestTimeout
            };

            try
            {
                return await _centralManager.Send<GamePlayRequest, GamePlayResponse>(gamePlayRequest, token);
            }
            catch (UnexpectedResponseException e)
            {
                Logger.Debug($"Got incorrect response to GamePlayRequest, of type {e.Response.GetType()} with status {e.Response.MessageStatus} : {e.Message}");
                Logger.Debug($"Attempting recovery from server for failed request {gamePlayRequest.SequenceId}");
                return await _gameRecoveryService.Recover(gamePlayRequest.SequenceId, token);
            }
        }

        private void ManualHandicapWinDisable()
        {
            if (_gamePlayEntityHelper.ManualHandicapWin)
            {
                _systemDisableManager.Disable(HhrConstants.ManualHandicapWinKey,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ManualHandicapWinText,
                    CultureProviderType.Player,
                    true,
                    () => Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.ManualHandicapWinHelpText));
            }
        }
    }
}