namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Linq;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    public static class ReportGameOutcomeMessageExtensions
    {
        public static ServerApiGateway.GameOutcome ToGameOutcome(this ReportGameOutcomeMessage message)
        {
            var bingoOutcome = new BingoGameOutcomeMeta
            {
                BallCall = string.Join(",", message.BallCall),
                Paytable = message.Paytable,
                ReportType = BingoGameOutcomeMeta.Types.ReportType.End,
                GameEndWinEligibility = message.GameEndWinEligibility,
                JoinBallNumber = message.JoinBall
            };

            bingoOutcome.Cards.AddRange(message.CardsPlayed.Select(ToCardPlayed));
            bingoOutcome.WinResults.AddRange(message.WinResults.Select(ToWinResult));
            return new ServerApiGateway.GameOutcome
            {
                Denomination = message.DenominationId,
                BetAmount = message.BetAmount,
                StartTime = message.StartTime.ToUniversalTime().ToTimestamp(),
                FacadeKey = message.FacadeKey,
                FinalBalance = message.FinalBalance,
                GameSerial = message.GameSerial,
                GameTitleId = message.GameTitleId,
                InitialBalance = message.StartingBalance,
                JoinTime = message.JoinTime.ToUniversalTime().ToTimestamp(),
                ThemeId = message.ThemeId,
                MachineSerial = message.MachineSerial,
                GameOutcomeMeta = Any.Pack(bingoOutcome),
                Status = true,
                PaidAmount = message.PaidAmount,
                TotalWinAmount = message.TotalWin,
                ProgressiveLevels = string.Join(",", message.ProgressiveLevels),
                PresentationNumber = message.PresentationIndex
            };
        }

        private static BingoGameOutcomeMeta.Types.WinResult ToWinResult(WinResult winResult)
        {
            return new BingoGameOutcomeMeta.Types.WinResult
            {
                PatternId = winResult.PatternId,
                Payout = winResult.Payout,
                BallQuantity = winResult.BallQuantity,
                BitPattern = winResult.BitPattern,
                PaytableId = winResult.PatternId,
                PatternName = winResult.PatternName,
                CardSerial = winResult.CardSerial,
                IsGew = winResult.IsGameEndWin,
                WinIndex = winResult.WinIndex
            };
        }

        private static BingoGameOutcomeMeta.Types.CardPlayed ToCardPlayed(CardPlayed cardPlayed)
        {
            return new BingoGameOutcomeMeta.Types.CardPlayed
            {
                DaubBitPattern = cardPlayed.BitPattern,
                GewClaimable = cardPlayed.IsGameEndWin,
                Serial = cardPlayed.SerialNumber
            };
        }
    }
}