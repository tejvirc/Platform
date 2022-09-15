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

        public async Task<bool> RequestGame(RequestGameOutcomeMessage message, CancellationToken token)
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

            using var caller = Invoke(x => x.RequestGamePlay(request, null, null, token));
            var responseStream = caller.ResponseStream;
            while (await responseStream.MoveNext(token) && await ReadGameOutcome(responseStream.Current, request, token))
            {
            }

            return true;
        }

        public async Task<ClaimWinResults> ClaimWin(RequestClaimWinMessage message, CancellationToken token)
        {
            var request = new ClaimWinRequest
            {
                MachineSerial = message.MachineSerial,
                GameSerial = message.GameSerial,
                ClaimWinMeta = Any.Pack(new BingoGameClaimWinMeta { CardSerial = message.CardSerial })
            };

            var response = await Invoke(async x => await x.RequestClaimWinAsync(request, null, null, token));
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

        public async Task<ReportGameOutcomeResponse> ReportGameOutcome(ReportGameOutcomeMessage message, CancellationToken token)
        {
            var result = await Invoke(async x => await x.ReportGameOutcomeAsync(message.ToGameOutcome()));
            return new ReportGameOutcomeResponse(result.Succeeded ? ResponseCode.Ok : ResponseCode.Rejected);
        }

        private static GameOutcome ProcessRejectedResponse(GamePlayResponse gamePlayOutcome, GamePlayRequest request)
        {
            uint.TryParse(request.ActiveGameTitles, out var themeId);
            var outcome = new GameOutcome(
                ResponseCode.Rejected,
                gamePlayOutcome.MachineSerial,
                gamePlayOutcome.TotalWinAmount,
                gamePlayOutcome.ProgressiveLevels,
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId,
                gamePlayOutcome.Denomination,
                gamePlayOutcome.GameSerial,
                gamePlayOutcome.StatusMessage,
                gamePlayOutcome.Status,
                string.Empty,
                0,
                Enumerable.Empty<CardPlayed>(),
                Enumerable.Empty<int>(),
                Enumerable.Empty<WinResult>(),
                gamePlayOutcome.ReportType == GamePlayResponse.Types.ReportType.End,
                0);

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

            Logger.Debug(
                $"Outcome response: {cards.Count} cards, {serverBingoOutcome.BallCall} balls, {wins.Count} wins, Status={gamePlayOutcome.Status} StatusMessage={gamePlayOutcome.StatusMessage} ReportType={gamePlayOutcome.ReportType}");

            uint.TryParse(request.ActiveGameTitles, out var themeId);
            return new GameOutcome(
                ResponseCode.Ok,
                gamePlayOutcome.MachineSerial,
                gamePlayOutcome.TotalWinAmount,
                gamePlayOutcome.ProgressiveLevels,
                gamePlayOutcome.FacadeKey,
                gamePlayOutcome.GameTitleId,
                themeId,
                gamePlayOutcome.Denomination,
                gamePlayOutcome.GameSerial,
                gamePlayOutcome.StatusMessage,
                gamePlayOutcome.Status,
                serverBingoOutcome.Paytable,
                serverBingoOutcome.GameEndWinEligibility,
                cards,
                string.IsNullOrEmpty(serverBingoOutcome.BallCall)
                    ? Enumerable.Empty<int>()
                    : serverBingoOutcome.BallCall.Split(BallCallDelimiter).Select(int.Parse),
                wins,
                gamePlayOutcome.ReportType == GamePlayResponse.Types.ReportType.End,
                serverBingoOutcome.JoinBallNumber);
        }

        private async Task<bool> ReadGameOutcome(GamePlayResponse gamePlayOutcome, GamePlayRequest request, CancellationToken token)
        {
            var outcome = gamePlayOutcome.Status
                ? ProcessAcceptedResponse(gamePlayOutcome, request)
                : ProcessRejectedResponse(gamePlayOutcome, request);
            var handlerResult = await _messageHandlerFactory.Handle<GameOutcomeResponse, GameOutcome>(outcome, token);
            return gamePlayOutcome.Status && !outcome.IsFinal && handlerResult.ResponseCode == ResponseCode.Ok;
        }
    }
}