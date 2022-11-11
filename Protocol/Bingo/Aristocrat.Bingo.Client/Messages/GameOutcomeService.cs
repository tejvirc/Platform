namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using GamePlay;
    using Google.Protobuf.WellKnownTypes;
    using log4net;
    using ServerApiGateway;
    using GameOutcome = GamePlay.GameOutcome;

    public class GameOutcomeService : BaseClientCommunicationService, IGameOutcomeService
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

        public Task<bool> RequestGame(RequestGameOutcomeMessage message, CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return RequestGameInternal(message, token);
        }

        public Task<ClaimWinResults> ClaimWin(RequestClaimWinMessage message, CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ClaimWinInternal(message, token);
        }

        public async Task<ReportGameOutcomeResponse> ReportGameOutcome(ReportGameOutcomeMessage message, CancellationToken token)
        {
            var result = await Invoke(async x => await x.ReportGameOutcomeAsync(message.ToGameOutcome()))
                .ConfigureAwait(false);
            return new ReportGameOutcomeResponse(result.Succeeded ? ResponseCode.Ok : ResponseCode.Rejected);
        }

        private static GameOutcome ProcessRejectedResponse(GamePlayResponse gamePlayOutcome, GamePlayRequest request)
        {
            if (uint.TryParse(request.ActiveGameTitles, out var themeId))
            {
                Logger.Warn($"Invalid theme id: {request.ActiveGameTitles}");
            }

            var bingoDetails = new GameOutcomeBingoDetails(0, Array.Empty<CardPlayed>(), Array.Empty<int>(), 0);
            var gameDetails = new GameOutcomeGameDetails(
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId,
                gamePlayOutcome.Denomination,
                string.Empty,
                gamePlayOutcome.GameSerial);
            var winDetails = new GameOutcomeWinDetails(0, gamePlayOutcome.ProgressiveLevels, Array.Empty<WinResult>());
            var outcome = new GameOutcome(
                ResponseCode.Rejected,
                winDetails,
                gameDetails,
                bingoDetails,
                gamePlayOutcome.Status,
                gamePlayOutcome.ReportType == ReportType.End);

            Logger.Warn("Outcome response rejected");
            return outcome;
        }

        private static GameOutcome ProcessAcceptedResponse(GamePlayResponse gamePlayOutcome, GamePlayRequest request)
        {
            var serverBingoOutcome = gamePlayOutcome.GamePlayResponseMeta.Unpack<BingoGamePlayResponseMeta>();

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
            var ballCall = string.IsNullOrEmpty(serverBingoOutcome.BallCall)
                ? Array.Empty<int>()
                : serverBingoOutcome.BallCall.Split(BallCallDelimiter).Select(int.Parse).ToArray();

            Logger.Debug(
                $"Outcome response: {cards.Count} cards, {serverBingoOutcome.BallCall} balls, {wins.Count} wins, Status={gamePlayOutcome.Status} StatusMessage={gamePlayOutcome.StatusMessage} ReportType={gamePlayOutcome.ReportType}");

            if (uint.TryParse(request.ActiveGameTitles, out var themeId))
            {
                Logger.Warn($"Invalid theme id: {request.ActiveGameTitles}");
            }

            var bingoDetails = new GameOutcomeBingoDetails(
                serverBingoOutcome.GameEndWinEligibility,
                cards,
                ballCall,
                serverBingoOutcome.JoinBallNumber);
            var gameDetails = new GameOutcomeGameDetails(
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId,
                gamePlayOutcome.Denomination,
                serverBingoOutcome.Paytable,
                gamePlayOutcome.GameSerial);
            var winDetails = new GameOutcomeWinDetails(
                gamePlayOutcome.TotalWinAmount,
                gamePlayOutcome.ProgressiveLevels,
                wins);
            return new GameOutcome(
                ResponseCode.Ok,
                winDetails,
                gameDetails,
                bingoDetails,
                gamePlayOutcome.Status,
                gamePlayOutcome.ReportType == ReportType.End);
        }

        private async Task<bool> RequestGameInternal(RequestGameOutcomeMessage message, CancellationToken token)
        {
            Logger.Debug("Request new game");
            var request = new GamePlayRequest
            {
                MachineSerial = message.MachineSerial,
                BetAmount = message.BetAmount,
                ActiveDenomination = message.ActiveDenomination,
                BetLinePresetId = message.BetLinePresetId,
                LineBet = message.LineBet,
                Lines = message.Lines,
                Ante = message.Ante,
                ActiveGameTitles = message.ActiveGameTitles,
                GamePlayMeta = Any.Pack(new BingoGamePlayMeta())
            };

            using var caller = Invoke((x, c) => x.RequestGamePlay(request, null, null, c), token);
            var responseStream = caller.ResponseStream;
            while (await responseStream.MoveNext(token).ConfigureAwait(false) &&
                   await ReadGameOutcome(responseStream.Current, request, token).ConfigureAwait(false))
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

        private async Task<bool> ReadGameOutcome(GamePlayResponse gamePlayOutcome, GamePlayRequest request, CancellationToken token)
        {
            var outcome = gamePlayOutcome.Status
                ? ProcessAcceptedResponse(gamePlayOutcome, request)
                : ProcessRejectedResponse(gamePlayOutcome, request);
            var handlerResult = await _messageHandlerFactory.Handle<GameOutcomeResponse, GameOutcome>(outcome, token)
                .ConfigureAwait(false);
            return gamePlayOutcome.Status && !outcome.IsFinal && handlerResult.ResponseCode == ResponseCode.Ok;
        }
    }
}