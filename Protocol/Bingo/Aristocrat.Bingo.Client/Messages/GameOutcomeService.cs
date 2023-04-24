namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using GamePlay;
    using Google.Protobuf.WellKnownTypes;
    using log4net;
    using ServerApiGateway;
    using GameOutcome = GamePlay.GameOutcome;

    /// <summary>
    ///     Provides outcomes of bingo games
    /// </summary>
    public class GameOutcomeService :
        BaseClientCommunicationService<ClientApi.ClientApiClient>,
        IGameOutcomeService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const char BallCallDelimiter = ',';

        private readonly IMessageHandlerFactory _messageHandlerFactory;

        public GameOutcomeService(
            IMessageHandlerFactory messageHandlerFactory,
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
            _messageHandlerFactory =
                messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
        }

        /// <inheritdoc/>
        public Task<bool> RequestMultiGame(RequestMultipleGameOutcomeMessage message, CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return RequestMultiGameInternal(message, token);
        }

        /// <inheritdoc/>
        public Task<ClaimWinResults> ClaimWin(RequestClaimWinMessage message, CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ClaimWinInternal(message, token);
        }

        /// <inheritdoc/>
        public Task<ReportMultiGameOutcomeResponse> ReportMultiGameOutcome(ReportMultiGameOutcomeMessage message, CancellationToken token)
        {
            // TODO: fill this in as part of the game play story
            //var result = await Invoke(async x => await x.ReportMultiGameOutcomeAsync(message.ToGameOutcome()))
            //    .ConfigureAwait(false);
            return Task.FromResult(new ReportMultiGameOutcomeResponse(ResponseCode.Rejected));
        }

        private static GameOutcome ProcessAcceptedResponse(SingleGamePlayResponse gamePlayOutcome, SingleGamePlayRequest request)
        {
            Logger.Debug("ProcessAcceptedResponse");

            var serverBingoOutcome = gamePlayOutcome.GamePlayResponseMeta.Unpack<BingoSingleGamePlayResponseMeta>();

            var cards = serverBingoOutcome.Cards.Select(
                cardMeta => new CardPlayed(cardMeta.Serial, cardMeta.DaubBitPattern, cardMeta.GewClaimable)).ToList();

            var wins = serverBingoOutcome.WinResults.Select(
                    winMeta => new WinResult(
                        winMeta.PatternId,
                        winMeta.Payout,
                        winMeta.BallQuantity,
                        winMeta.BitPattern,
                        winMeta.PaytableId,
                        winMeta.PatternName,
                        winMeta.CardSerial,
                        winMeta.IsGew,
                        winMeta.WinIndex))
                .ToList();

            var ballCall = new List<int>();
            //var ballCall = string.IsNullOrEmpty(serverBingoOutcome.BallCall)
            //    ? Array.Empty<int>()
            //    : serverBingoOutcome.BallCall.Split(BallCallDelimiter).Select(int.Parse).ToArray();

            Logger.Debug(
                $"Outcome response GameNumber {gamePlayOutcome.GameNumber}: {cards.Count} cards, {ballCall} balls, {wins.Count} wins, Status={gamePlayOutcome.Status} StatusMessage={gamePlayOutcome.StatusMessage} ReportType={gamePlayOutcome.ReportType}");

            //if (uint.TryParse(request.ActiveGameTitles, out var themeId))
            //{
            //    Logger.Warn($"Invalid theme id: {request.ActiveGameTitles}");
            //}

            // TODO there is no theme id available now
            var themeId = 1U;

            var bingoDetails = new GameOutcomeBingoDetails(
                gamePlayOutcome.GameNumber == 0 ? 1 : 0, // TODO where is this serverBingoOutcome.GameEndWinEligibility
                cards,
                ballCall,
                1); // TODO where is this serverBingoOutcome.JoinBallNumber
            var gameDetails = new GameOutcomeGameDetails(
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId, 
                gamePlayOutcome.Denomination,
                serverBingoOutcome.Paytable,
                gamePlayOutcome.GameSerial);
            var winDetails = new GameOutcomeWinDetails(
                gamePlayOutcome.TotalWinAmount,
                string.Empty, // TODO there is no progressive levels available (gamePlayOutcome.ProgressiveLevels)
                wins);
            return new GameOutcome(
                ResponseCode.Ok,
                winDetails,
                gameDetails,
                bingoDetails,
                gamePlayOutcome.Status,
                gamePlayOutcome.ReportType == SingleGamePlayResponse.Types.ReportType.End);
        }

        private static GameOutcome ProcessRejectedResponse(SingleGamePlayResponse gamePlayOutcome, SingleGamePlayRequest request)
        {
            Logger.Debug("ProcessRejectedResponse");

            //if (uint.TryParse(request.ActiveGameTitles, out var themeId))
            //{
            //    Logger.Warn($"Invalid theme id: {request.ActiveGameTitles}");
            //}

            var themeId = 1U;

            var bingoDetails = new GameOutcomeBingoDetails(0, Array.Empty<CardPlayed>(), Array.Empty<int>(), 0);
            var gameDetails = new GameOutcomeGameDetails(
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId,
                gamePlayOutcome.Denomination,
                string.Empty,
                gamePlayOutcome.GameSerial);
            // TODO there is no progressive levels available (gamePlayOutcome.ProgressiveLevels)
            var winDetails = new GameOutcomeWinDetails(0, string.Empty, Array.Empty<WinResult>());
            var outcome = new GameOutcome(
                ResponseCode.Rejected,
                winDetails,
                gameDetails,
                bingoDetails,
                gamePlayOutcome.Status,
                gamePlayOutcome.ReportType == SingleGamePlayResponse.Types.ReportType.End);

            Logger.Warn($"Outcome response GameNumber {gamePlayOutcome.GameNumber} rejected");
            return outcome;
        }

        private async Task<bool> RequestMultiGameInternal(RequestMultipleGameOutcomeMessage message, CancellationToken token)
        {
            Logger.Debug("Request new multi games");
            var requests = new MultiGamePlayRequest
            {
                MachineSerial = message.MachineSerial,
            };

            foreach (var singleRequest in message.GameRequests)
            {
                requests.GamePlayRequests.Add(
                    new SingleGamePlayRequest
                    {
                        Ante = singleRequest.Ante,
                        Lines = singleRequest.Lines,
                        LineBet = singleRequest.LineBet,
                        BetLinePresetId = singleRequest.BetLinePresetId,
                        ActiveDenomination = (int)singleRequest.ActiveDenomination,
                        BetAmount = singleRequest.BetAmount,
                        GameNumber = singleRequest.GameIndex,
                        TitleId = singleRequest.ActiveGameTitle,
                        GamePlayMeta = Any.Pack(new BingoGamePlayMeta())
                    });
            }

            using var caller = Invoke((x, c) => x.RequestMultiGamePlay(requests, null, null, c), token);
            var responseStream = caller.ResponseStream;
            while (await responseStream.MoveNext(token).ConfigureAwait(false) &&
                   await ReadMultiGameOutcome(responseStream.Current, requests, token).ConfigureAwait(false))
            {
            }

            return true;
        }

        private async Task<ClaimWinResults> ClaimWinInternal(RequestClaimWinMessage message, CancellationToken token)
        {
            var request = new ClaimWinRequest
            {
                MachineSerial = message.MachineSerial,
                GameSerial = message.GameSerial,
                ClaimWinMeta = Any.Pack(new BingoGameClaimWinMeta { CardSerial = message.CardSerial })
            };

            var response = await Invoke(async (x, c) => await x.RequestClaimWinAsync(request, null, null, c), token)
                .ConfigureAwait(false);
            var claimWin = response.ClaimWinMeta.Unpack<BingoGameClaimWinMeta>();

            var result = new ClaimWinResults(
                response.WinResult == ClaimWinResponse.Types.ClaimWinResult.Accepted
                    ? ResponseCode.Ok
                    : ResponseCode.Rejected,
                response.WinResult == ClaimWinResponse.Types.ClaimWinResult.Accepted,
                response.GameSerial,
                claimWin.CardSerial);

            return result;
        }

        private async Task<bool> ReadMultiGameOutcome(MultiGamePlayResponse gamePlayOutcome, MultiGamePlayRequest multiRequest, CancellationToken token)
        {
            Logger.Debug("ReadMultiGameOutcome");

            if (gamePlayOutcome.GamePlayResponses.Count != multiRequest.GamePlayRequests.Count)
            {
                throw new ArgumentException("Game play responses do not match the number of game play requests");
            }

            var status = true;
            for (var i = 0; i < gamePlayOutcome.GamePlayResponses.Count; ++i)
            {
                var request = multiRequest.GamePlayRequests[i];
                var response = gamePlayOutcome.GamePlayResponses[i];

                if (!response.Status)
                {
                    status = false;
                }

                var outcome = response.Status
                    ? ProcessAcceptedResponse(response, request)
                    : ProcessRejectedResponse(response, request);
                var handlerResult = await _messageHandlerFactory
                    .Handle<GameOutcomeResponse, GameOutcome>(outcome, token)
                    .ConfigureAwait(false);

                Logger.Debug($"status={response.Status}, isFinal={outcome.IsFinal}, handler.ResponseCode={handlerResult.ResponseCode}");

                if (!response.Status || outcome.IsFinal && handlerResult.ResponseCode != ResponseCode.Ok)
                {
                    status = false;
                }
            }

            return status;
        }
    }
}