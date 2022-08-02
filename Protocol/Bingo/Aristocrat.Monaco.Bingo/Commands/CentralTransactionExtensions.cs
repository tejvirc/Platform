namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Services.GamePlay;

    /// <summary>
    ///     Extension methods for <see cref="CentralTransaction"/>
    /// </summary>
    public static class CentralTransactionExtensions
    {
        /// <summary>
        ///     Converts the central transaction into the report game outcome message that gets sent to the sever
        /// </summary>
        /// <param name="transaction">The transaction to convert</param>
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="log">The game history log for this transaction is for</param>
        /// <returns>The report game outcome message that gets sent to the server</returns>
        public static ReportGameOutcomeMessage ToReportGameOutcomeMessage(
            this CentralTransaction transaction,
            string machineSerial,
            IGameHistoryLog log)
        {
            var description = transaction.Descriptions?.OfType<BingoGameDescription>().FirstOrDefault();
            if (description is null || log is null)
            {
                return null;
            }

            return new ReportGameOutcomeMessage
            {
                TransactionId = transaction.TransactionId,
                MachineSerial = machineSerial,
                BetAmount = log.FinalWager,
                TotalWin = description.Patterns.Where(KeepPattern).Sum(x => x.WinAmount),
                PaidAmount = log.TotalWon,
                StartingBalance = log.StartCredits.MillicentsToCents(),
                FinalBalance = log.EndCredits.MillicentsToCents(),
                FacadeKey = description.FacadeKey,
                GameEndWinEligibility = description.GameEndWinEligibility,
                GameTitleId = description.GameTitleId,
                ThemeId = description.ThemeId,
                DenominationId = description.DenominationId,
                GameSerial = description.GameSerial,
                Paytable = description.Paytable,
                JoinBall = description.GetJoiningBall().Number,
                StartTime = log.StartDateTime,
                JoinTime = description.JoinTime,
                ProgressiveLevels = description.ProgressiveLevels,
                CardsPlayed = description.Cards.Select(ToCardPlayed),
                BallCall = description.BallCallNumbers.Select(x => x.Number),
                WinResults = description.Patterns.Where(KeepPattern).Select(ToWinResult),
                PresentationIndex = log.GameRoundDetails?.PresentationIndex ?? 0
            };

            bool KeepPattern(BingoPattern x) => log.GameWinBonus > 0 || !x.IsGameEndWin;
        }

        private static CardPlayed ToCardPlayed(BingoCard card)
        {
            return new CardPlayed(card.SerialNumber, card.DaubedBits, card.IsGameEndWin);
        }

        private static WinResult ToWinResult(BingoPattern pattern)
        {
            return new WinResult(
                pattern.PatternId,
                pattern.WinAmount,
                pattern.BallQuantity,
                pattern.BitFlags,
                pattern.PaytableId,
                pattern.Name,
                pattern.CardSerial,
                pattern.IsGameEndWin,
                pattern.WinIndex);
        }
    }
}