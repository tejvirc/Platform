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
            // TODO: fill this in as part of the report game outcomes story
            //var result = await Invoke(async x => await x.ReportMultiGameOutcomeAsync(message.ToGameOutcome()))
            //    .ConfigureAwait(false);
            return Task.FromResult(new ReportMultiGameOutcomeResponse(ResponseCode.Rejected));
        }

        private static GameOutcomes ProcessAcceptedResponse(MultiGamePlayResponse gamePlayOutcome, MultiGamePlayRequest request)
        {
            Logger.Debug("ProcessAcceptedResponses");

            var gamePlayResponse = gamePlayOutcome.MultiGamePlayResponseMeta.Unpack<BingoMultiGamePlayResponseMeta>();
            var ballCall = string.IsNullOrEmpty(gamePlayResponse.BallCall)
                ? Array.Empty<int>()
                : gamePlayResponse.BallCall.Split(BallCallDelimiter).Select(int.Parse).ToArray();

            var joinBallNumber = gamePlayResponse.JoinBallNumber;
            var gameEndWinEligibility = gamePlayResponse.GameEndWinEligibility;

            var response = new List<GameOutcome>();
            foreach (var game in gamePlayOutcome.GamePlayResponses)
            {
                AddResponseToOutcomes(game, ballCall, joinBallNumber, gameEndWinEligibility, response);
            }

            return new GameOutcomes(ResponseCode.Ok, response);
        }

        private static void AddResponseToOutcomes(SingleGamePlayResponse game, IReadOnlyCollection<int> ballCall, int joinBallNumber, int gameEndWinEligibility, ICollection<GameOutcome> outcomes)
        {
            Logger.Debug($"Processing game play outcome for game: gameNumber={game.GameNumber} gameSerial={game.GameSerial} titleId={game.GameTitleId} totalWin={game.TotalWinAmount} denom={game.Denomination}");

            var meta = game.GamePlayResponseMeta.Unpack<BingoSingleGamePlayResponseMeta>();

            Logger.Debug($"BingoSingleGamePlayResponseMeta = {meta}");

            var cards = meta.Cards.Select(
                cardMeta => new CardPlayed(cardMeta.Serial, cardMeta.DaubBitPattern, cardMeta.GewClaimable, cardMeta.CardType == CardType.Golden)).ToList();

            var wins = meta.WinResults.Select(
                    winMeta => new WinResult(
                        winMeta.PatternId,
                        winMeta.Payout,
                        winMeta.BallQuantity,
                        winMeta.BitPattern,
                        winMeta.PaytableId,
                        winMeta.PatternName,
                        winMeta.CardSerial,
                        winMeta.IsGew,
                        winMeta.WinIndex,
                        winMeta.ProgressiveWins))
                .ToList();

            foreach (var win in wins)
            {
                Logger.Debug($"Win: patternId={win.PatternId} patternName={win.PatternName} balls={win.BallQuantity} paytableId={win.PaytableId} payout={win.Payout}");
            }

            Logger.Debug($"Outcome response GameNumber {game.GameNumber}: {cards.Count} cards, {ballCall} balls," +
                         $" {wins.Count} wins, Status={game.Status} StatusMessage={game.StatusMessage} ReportType={game.ReportType}");

            // TODO there is no theme id available now
            var themeId = 1U;

            var bingoDetails = new GameOutcomeBingoDetails(
                gameEndWinEligibility,
                cards,
                ballCall,
                joinBallNumber);

            var gameDetails = new GameOutcomeGameDetails(
                game.FacadeKey,
                game.GameTitleId,
                themeId,
                game.Denomination,
                meta.Paytable,
                game.GameSerial);

            var winDetails = new GameOutcomeWinDetails(
                game.TotalWinAmount,
                string.Empty, // TODO there is no progressive levels available (gamePlayOutcome.ProgressiveLevels)
                wins);

            outcomes.Add(new GameOutcome(
                ResponseCode.Ok,
                winDetails,
                gameDetails,
                bingoDetails,
                game.Status,
                game.ReportType == SingleGamePlayResponse.Types.ReportType.End,
                game.GameNumber, // TODO no access to GameId so using game number
                game.GameNumber));
        }

        private static GameOutcomes ProcessRejectedResponse(MultiGamePlayResponse gamePlayOutcome, MultiGamePlayRequest request)
        {
            Logger.Debug("ProcessRejectedResponse");

            var response = new List<GameOutcome>();
            foreach (var game in gamePlayOutcome.GamePlayResponses)
            {
                // TODO there is no theme id available now
                var themeId = 1U;

                var bingoDetails = new GameOutcomeBingoDetails(0, Array.Empty<CardPlayed>(), Array.Empty<int>(), 0);

                var gameDetails = new GameOutcomeGameDetails(
                    game.FacadeKey,
                    game.GameTitleId,
                    themeId,
                    game.Denomination,
                    string.Empty,
                    game.GameSerial);

                var winDetails = new GameOutcomeWinDetails(0, string.Empty, Array.Empty<WinResult>());
                Logger.Debug($"Outcome response GameNumber {game.GameNumber}: {bingoDetails.CardsPlayed.Count} cards, {bingoDetails.BallCall.Count} balls," +
                             $" {winDetails.WinResults.Count} wins, Status={game.Status} StatusMessage={game.StatusMessage} ReportType={game.ReportType}");

                response.Add(new GameOutcome(
                    ResponseCode.Rejected,
                    winDetails,
                    gameDetails,
                    bingoDetails,
                    game.Status,
                    game.ReportType == SingleGamePlayResponse.Types.ReportType.End,
                    0));  // TODO no access to GameId

                Logger.Warn($"Outcome response GameNumber {game.GameNumber} rejected");
            }

            return new GameOutcomes(ResponseCode.Rejected, response);
        }

        private async Task<bool> RequestMultiGameInternal(RequestMultipleGameOutcomeMessage message, CancellationToken token)
        {
            var requests = new MultiGamePlayRequest
            {
                MachineSerial = message.MachineSerial,
            };

            foreach (var singleRequest in message.GameRequests)
            {
                Logger.Debug($"Request game: GameNumber{singleRequest.GameIndex}, Bet={singleRequest.BetAmount}, denom={singleRequest.ActiveDenomination}, title={singleRequest.ActiveGameTitle }");

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
            if (gamePlayOutcome.GamePlayResponses.Count != multiRequest.GamePlayRequests.Count)
            {
                throw new ArgumentException("Game play responses do not match the number of game play requests");
            }

            var status = true;

            // This assumes if the first game play response has a Rejected status then
            // all the responses should be rejected. We can't have a rejected and ok status in
            // the same outcome.
            var responseStatus = gamePlayOutcome.GamePlayResponses.First().Status;
            var outcome = responseStatus
                ? ProcessAcceptedResponse(gamePlayOutcome, multiRequest)
                : ProcessRejectedResponse(gamePlayOutcome, multiRequest);

            var handlerResult = await _messageHandlerFactory
                .Handle<GameOutcomeResponse, GameOutcomes>(outcome, token)
                .ConfigureAwait(false);

            if (!responseStatus || outcome.Outcomes.Any(g => g.IsFinal) && handlerResult.ResponseCode != ResponseCode.Ok)
            {
                status = false;
            }

            return status;
        }
    }
}