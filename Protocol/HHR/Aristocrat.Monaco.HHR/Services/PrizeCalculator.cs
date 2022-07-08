namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using AutoMapper.Internal;
    using Client.Data;
    using Client.Messages;
    using Events;
    using Exceptions;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Facilitates PrizeDeterminationService to Calculate the prize.
    /// </summary>
    internal class PrizeCalculator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IEventBus _eventBus;

        public PrizeCalculator(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IEventBus eventBus)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        ///     Calculated Prize from Bonanza, RacePatterns
        /// </summary>
        /// <param name="gamePlayResponse">Bonanza messages received from CentralServer.</param>
        /// <param name="racePatterns">Race patterns for played GameId.</param>
        /// <returns>PrizeInformation populated after pattern matching.</returns>
        public PrizeInformation Calculate(GamePlayResponse gamePlayResponse, CRacePatterns racePatterns)
        {
            var prizeInformation = new PrizeInformation();
            var szData = gamePlayResponse.Prize;

            string prizeWinner, prizeRaceSet1, prizeRaceSet2,
                szWagerR1 = string.Empty,
                szWagerR2 = string.Empty;

            var prizeZero = "P=0";
            gamePlayResponse.RaceInfo.PrizeRaceSet1 = gamePlayResponse.RaceInfo.PrizeRaceSet2 = prizeRaceSet1 = prizeRaceSet2 = prizeWinner = prizeZero;

            MatchPatterns(FindPatterns(gamePlayResponse));

            // When manual handicapping, we just calculate the prize to populate RaceData so that Server can take down prizes, we will again come here when we attempt to send RaceStart.
            if (gamePlayResponse.HandicapEnter == 1)
            {
                return null;
            }

            ValidateWin();

            PopulatePrizeInformation();

            PopulateProgressiveInfo();

            return prizeInformation;

            void PopulatePrizeInformation()
            {
                prizeInformation.RaceInfo = gamePlayResponse.RaceInfo;
                prizeInformation.ReplyId = gamePlayResponse.ReplyId;

                // Set IDs
                prizeInformation.ScratchTicketSetId = gamePlayResponse.ScratchTicketSetId;
                prizeInformation.ScratchTicketId = gamePlayResponse.ScratchTicketId;

                //Set Wagers
                uint.TryParse(szWagerR1.GetPrizeString(HhrConstants.Wager), out prizeInformation.RaceSet1Wager);
                uint.TryParse(szWagerR2.GetPrizeString(HhrConstants.Wager), out prizeInformation.RaceSet2Wager);

                long.TryParse(
                    gamePlayResponse.RaceInfo.PrizeRaceSet1.GetPrizeString(HhrConstants.PrizeValue),
                    out prizeInformation.RaceSet1AmountWon);

                long.TryParse(
                    gamePlayResponse.RaceInfo.PrizeRaceSet2.GetPrizeString(HhrConstants.PrizeValue),
                    out prizeInformation.RaceSet2AmountWonWithoutProgressives);

                // Populate ExtraWinnings
                prizeInformation.RaceSet1ExtraWinnings = gamePlayResponse.RaceInfo.PariPrize1;
                prizeInformation.RaceSet2ExtraWinnings = gamePlayResponse.RaceInfo.PariPrize2;
                prizeInformation.LastGamePlayedTime = gamePlayResponse.LastGamePlayTime;

                // Update game overriden value
                prizeInformation.BOverride = gamePlayResponse.BOverride;

                prizeInformation.ProgressiveWin = gamePlayResponse.RaceInfo.ProgWon;
            }

            void MatchPatterns(IReadOnlyList<int> patterns)
            {
                Logger.Debug("Matching patterns " + string.Join(" ", patterns.Select(x => x.ToString("X2"))));

                // Evaluate achieved Pattern vs. GameOpen prize Patterns.
                // Store winning prizes in PrizeRaceSet1 and PrizeRaceSet2
                // Get individual prize amounts from Race Set 1 and Race Set 2
                racePatterns.Pattern.ForAll(
                    pattern =>
                    {
                        switch (pattern.RaceGroup)
                        {
                            case 1:
                                // TODO: Why not just get this once from the first pattern we find?
                                if (szWagerR1 == string.Empty)
                                {
                                    szWagerR1 = pattern.Prize;
                                }

                                // TODO: How about breaking out of the loop once we've found a prize?
                                if (!gamePlayResponse.RaceInfo.PrizeRaceSet1.Equals(prizeZero) ||
                                    !PatternMatch(patterns, pattern))
                                {
                                    return;
                                }

                                prizeWinner = prizeRaceSet1 = gamePlayResponse.RaceInfo.PrizeRaceSet1 = pattern.Prize;
                                break;

                            case 2:
                                if (szWagerR2 == string.Empty)
                                {
                                    szWagerR2 = pattern.Prize;
                                }

                                if (!gamePlayResponse.RaceInfo.PrizeRaceSet2.Equals(prizeZero) ||
                                    !PatternMatch(patterns, pattern))
                                {
                                    return;
                                }

                                prizeWinner = prizeRaceSet2 = gamePlayResponse.RaceInfo.PrizeRaceSet2 = pattern.Prize;
                                break;
                        }
                    });
            }

            void PopulateProgressiveInfo()
            {
                prizeInformation.ProgressiveLevelsHit = gamePlayResponse.RaceInfo.PrizeRaceSet2
                    .GetPrizeString(HhrConstants.ProgressiveInformation).ParseProgressiveHitInfo();

                if (!prizeInformation.ProgressiveLevelsHit.Any())
                {
                    return;
                }

                long totalProgressiveResetValue = 0;
                var progressiveLevelAmountHit = new List<(int, long)>();

                foreach (var (levelId, count) in prizeInformation.ProgressiveLevelsHit)
                {
                    var progressiveLevel = prizeInformation
                        .GetActiveProgressiveLevelsForWager(_protocolLinkedProgressiveAdapter)
                        .First(x => x.LevelId == levelId); // We are getting GameProgressiveLevel
                            // corresponding to Server level Id (and not LinkedProgressiveLevel),
                            // so both are indexed from 0

                    var currentAmount = prizeInformation.ProgressiveWin.ElementAt(levelId) +
                        progressiveLevel.ResetValue.MillicentsToCents() * (count - 1);
                    prizeInformation.TotalProgressiveAmountWon += currentAmount;

                    progressiveLevelAmountHit.Add((levelId, currentAmount));

                    totalProgressiveResetValue += progressiveLevel.ResetValue.MillicentsToCents() * count;
                }

                //Subtract ProgReset from AmountWon
                prizeInformation.RaceSet2AmountWonWithoutProgressives -= totalProgressiveResetValue;
                prizeInformation.ProgressiveLevelAmountHit = progressiveLevelAmountHit;

                Logger.Debug(
                    $"[PROG] RaceSet2AmountWonWithoutProgressives={prizeInformation.RaceSet2AmountWonWithoutProgressives}, ProgResetValue={totalProgressiveResetValue}, TotalProgressiveAmountWon={prizeInformation.TotalProgressiveAmountWon}");
            }

            void ValidateWin()
            {
                // If not handicapping then verify that szData and PrizeWinner Match
                if (gamePlayResponse.RaceInfo.HandicapData == 0)
                {
                    CheckAndTriggerPrizeMismatch(szData, prizeWinner);
                }
                else
                {
                    // If Manual Handicapping match RaceSet1 prize and RaceSet2 prize.
                    CheckAndTriggerPrizeMismatch(prizeRaceSet1, gamePlayResponse.RaceInfo.PrizeRaceSet1);
                    CheckAndTriggerPrizeMismatch(prizeRaceSet2, gamePlayResponse.RaceInfo.PrizeRaceSet2);
                }

                void CheckAndTriggerPrizeMismatch(string expected, string actual)
                {
                    if (expected == actual)
                    {
                        return;
                    }

                    _eventBus.Publish(new PrizeCalculationErrorEvent());
                    throw new PrizeCalculationException(expected, actual);
                }
            }
        }

        

        private static bool PatternMatch(IReadOnlyList<int> patterns, CRacePattern racePattern)
        {
            var startIndex = racePattern.RaceGroup == 1 ? 0 : 5;
            var pc1 = (racePattern.Pattern1 & patterns[startIndex]) == racePattern.Pattern1;
            var pc2 = (racePattern.Pattern2 & patterns[startIndex + 1]) == racePattern.Pattern2;
            var pc3 = (racePattern.Pattern3 & patterns[startIndex + 2]) == racePattern.Pattern3;
            var pc4 = (racePattern.Pattern4 & patterns[startIndex + 3]) == racePattern.Pattern4;
            var pc5 = (racePattern.Pattern5 & patterns[startIndex + 4]) == racePattern.Pattern5;

            return pc1 && pc2 && pc3 && pc4 && pc5;
        }

        private List<int> FindPatterns(GamePlayResponse gamePlayResponse)
        {
            //Produce Pattern for each race from actual vs. selections
            return gamePlayResponse.RaceInfo.RaceData.Select(
                rd =>
                    Enumerable.Range(0, 8)
                        .Where(value => rd.HorseSelection.ElementAt(value) == rd.HorseActual.ElementAt(value))
                        .Aggregate(0, (mask, index) => mask + (1 << index))).ToList();
        }
    }
}