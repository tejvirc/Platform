namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Google.Protobuf.WellKnownTypes;
    using log4net;
    using ServerApiGateway;

    /// <summary>
    ///     Extension methods for <see cref="CentralTransaction"/>
    /// </summary>
    public static class CentralTransactionExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

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

            Logger.Debug($"transaction has {transaction.Descriptions?.Count()} items");
            var singleGameOutcomes = new List<SingleGameOutcome>();
            foreach (var outcome in log.Outcomes)
            {
                var outcomeDescription = transaction.Descriptions.OfType<BingoGameDescription>().SingleOrDefault(d => d.GameIndex == outcome.GameIndex);
                if (outcomeDescription is null)
                {
                    continue;
                }
                if (!int.TryParse(outcomeDescription.Paytable, out var paytableId))
                {
                    paytableId = 0;
                }
                var bingoSingleGameOutcome = new BingoSingleGameOutcomeMeta
                    { PaytableId = paytableId };
                bingoSingleGameOutcome.Cards.AddRange(outcomeDescription.Cards.Select(ToCardPlayed));
                bingoSingleGameOutcome.WinResults.AddRange(outcomeDescription.Patterns.Where(KeepPattern).Select(ToWinResult));

                var singleGameOutcome = new SingleGameOutcome
                    {
                        TotalWinAmount = outcomeDescription.Patterns.Where(KeepPattern).Sum(x => x.WinAmount),
                        FacadeKey = outcomeDescription.FacadeKey.ToString(),
                        GameTitleId = (uint)outcome.GameSetId,
                        Denomination = outcomeDescription.DenominationId,
                        GameOutcomeMeta = Any.Pack(bingoSingleGameOutcome),
                        StatusMessage = string.Empty,
                        FinalBalance = log.EndCredits.MillicentsToCents(),
                        InitialBalance = log.StartCredits.MillicentsToCents(),
                        PaidAmount = log.TotalWon,
                        BetAmount = log.FinalWager,
                        StartTime = Timestamp.FromDateTime(log.StartDateTime),
                        JoinTime = Timestamp.FromDateTime(outcomeDescription.JoinTime),
                        PresentationNumber = log.GameRoundDetails?.PresentationIndex ?? 0,
                        ThemeId = outcomeDescription.ThemeId,
                        GameNumber = outcomeDescription.GameIndex,
                    };
                Logger.Debug($"Adding outcome for game index {singleGameOutcome.GameNumber}; Paytable ID is {paytableId}, GameTitleId={singleGameOutcome.GameTitleId}, Denomination={singleGameOutcome.Denomination}, BetAmount={singleGameOutcome.BetAmount}, PaidAmount={singleGameOutcome.PaidAmount}");
                singleGameOutcomes.Add(singleGameOutcome);
            }

            var bingoMultiGameMeta = new MultiBingoGameOutcomeMeta
                {
                    GameSerial = description.GameSerial,
                    BallCall = string.Join(",", description.BallCallNumbers.Select(b => b.Number)),
                    JoinBallNumber = description.JoinBallIndex,
                    GameEndWinEligibility = description.GameEndWinEligibility
                };

            var multiGameOutcome = new MultiGameOutcome { MachineSerial = machineSerial };
            multiGameOutcome.GameOutcomes.AddRange(singleGameOutcomes);
            multiGameOutcome.MultiGameOutcomeMeta = Any.Pack(bingoMultiGameMeta);

            return new ReportMultiGameOutcomeMessage
            {
                TransactionId = transaction.TransactionId,
                Message = multiGameOutcome
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
            IEnumerable<IBetDetails> details,
            int titleId,
            int? subGameTitleId)
        {
            var requests = new List<RequestSingleGameOutcomeMessage>();

            if (!transaction.AdditionalInfo.Any(x => x.GameIndex == 0))
            {
                throw new ArgumentNullException(nameof(transaction.AdditionalInfo));
            }

            var betDetails = details.ToList();
            foreach (var gameInfo in transaction.AdditionalInfo)
            {
                var betDetail = betDetails.Single(x => x.GameId == gameInfo.GameId);
                AddSingleGameOutcomeRequest(gameInfo, betDetail, titleId, subGameTitleId, requests);
            }

            return new RequestMultiPlayCommand(machineSerial, requests);
        }

        private static void AddSingleGameOutcomeRequest(IAdditionalGamePlayInfo gameInfo, IBetDetails betDetail, int mainTitleId, int? subGameTitleId, ICollection<RequestSingleGameOutcomeMessage> requests)
        {
            if (gameInfo.GameIndex != 0 && !subGameTitleId.HasValue)
            {
                return;
            }
            requests.Add(new RequestSingleGameOutcomeMessage(
                gameInfo.GameIndex,
                gameInfo.WagerAmount,
                gameInfo.Denomination,
                betDetail.BetLinePresetId,
                betDetail.BetPerLine,
                betDetail.NumberLines,
                betDetail.Ante,
                gameInfo.GameIndex == 0 ? mainTitleId : subGameTitleId.Value,
                gameInfo.GameId));
        }

        private static BingoSingleGameOutcomeMeta.Types.CardPlayed ToCardPlayed(BingoCard card)
        {
            return new BingoSingleGameOutcomeMeta.Types.CardPlayed
            {
                Serial = card.SerialNumber,
                DaubBitPattern = card.DaubedBits,
                GewClaimable = card.IsGameEndWin,
                CardType = card.IsGolden ? CardType.Golden : CardType.Normal
            };
        }

        private static BingoSingleGameOutcomeMeta.Types.WinResult ToWinResult(BingoPattern pattern)
        {
            return new BingoSingleGameOutcomeMeta.Types.WinResult { 
                PatternId = pattern.PatternId,
                Payout = pattern.WinAmount,
                BallQuantity = pattern.BallQuantity,
                BitPattern = pattern.BitFlags,
                PatternName = pattern.Name,
                CardSerial = pattern.CardSerial,
                IsGew = pattern.IsGameEndWin,
                WinIndex = pattern.WinIndex
            };
        }
    }
}