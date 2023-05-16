namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

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
        public static ReportMultiGameOutcomeMessage ToReportGameOutcomeMessage(
            this CentralTransaction transaction,
            string machineSerial,
            IGameHistoryLog log)
        {
            var description = transaction.Descriptions?.OfType<BingoGameDescription>().FirstOrDefault();
            if (description is null || log is null)
            {
                return null;
            }

            return new ReportMultiGameOutcomeMessage
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
                JoinBall = description.JoinBallIndex,
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

        /// <summary>
        ///     Converts the transaction into a RequestMultiPlayCommand
        /// </summary>
        /// <param name="transaction">The Central Transaction</param>
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="details">The bet details</param>
        /// <param name="titleId">The main game title id</param>
        /// <param name="subGameTitleId">The sub game title id</param>
        /// <returns></returns>
        public static RequestMultiPlayCommand GenerateMultiPlayRequest(
            this CentralTransaction transaction,
            string machineSerial,
            IBetDetails details,
            int titleId,
            int? subGameTitleId)
        {
            var requests = new List<RequestSingleGameOutcomeMessage>();

            // create play request for main game
            var message = new RequestSingleGameOutcomeMessage(
                0,
                transaction.WagerAmount.MillicentsToCents(),
                transaction.Denomination.MillicentsToCents(),
                details.BetLinePresetId,
                details.BetPerLine,
                details.NumberLines,
                details.Ante,
                titleId);

            requests.Add(message);

            // create play requests for any additional games
            if (transaction.AdditionalInfo.Any()  && subGameTitleId.HasValue)
            {
                // create multi-game request
                requests.AddRange(
                    transaction.AdditionalInfo.Select(
                        game => new RequestSingleGameOutcomeMessage(
                            game.GameIndex,
                            game.WagerAmount,
                            game.Denomination,
                            0,
                            0,
                            1,
                            0,
                            subGameTitleId.Value)));
            }

            return new RequestMultiPlayCommand(machineSerial, requests);
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
                pattern.WinIndex,
                Enumerable.Empty<string>());
        }
    }
}