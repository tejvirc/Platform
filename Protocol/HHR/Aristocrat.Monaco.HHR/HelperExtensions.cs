namespace Aristocrat.Monaco.Hhr
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using AutoMapper.Internal;
    using Client.Data;
    using Client.Messages;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Services;

    /// <summary>
    ///     Helper extension functions.
    /// </summary>
    public static class HelperExtensions
    {
        private const int RaceSetSize = 5;
        private static readonly IEnumerable<Guid> FilteredLockupsForGamePlay = new List<Guid>
        {
            ApplicationConstants.OperatorKeyNotRemovedDisableKey,
            ApplicationConstants.DisabledByHost0Key,
            ApplicationConstants.DisabledByHost1Key,
        };
        /// <summary>
        ///     Helper function to extract progressive hit information from progressive message
        /// </summary>
        /// <param name="progressiveInfo"> string representing progressive hit info</param>
        /// <returns>A Collection of tuple (level, times) containing level hit and how many times it was hit.</returns>
        public static IReadOnlyCollection<(int levels, int count)> ParseProgressiveHitInfo(this string progressiveInfo)
        {
            const int individualProgressiveHitSize = 2;
            var progressiveHits = new List<(int levels, int count)>();

            if (string.IsNullOrEmpty(progressiveInfo))
            {
                return progressiveHits;
            }

            var totalHitInfo = progressiveInfo.Length / individualProgressiveHitSize;

            for (var hitLevel = 0; hitLevel < totalHitInfo; ++hitLevel)
            {
                var hitCount = int.Parse(
                    string.Join(
                        "",
                        progressiveInfo.Skip(hitLevel * individualProgressiveHitSize)
                            .Take(individualProgressiveHitSize)));

                if (hitCount > 0)
                {
                    progressiveHits.Add((hitLevel, hitCount));
                }
            }

            return progressiveHits;
        }

        /// <summary>
        ///     Helper function to create progressive level from progressive response from server
        /// </summary>
        /// <param name="progResponse"> Progressive info response from server</param>
        /// <returns> A <see cref="LinkedProgressiveLevel" /> representation for progressive response</returns>
        public static LinkedProgressiveLevel ToProgressiveLevel(this ProgressiveInfoResponse progResponse)
        {
            return new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.HHR,
                ProgressiveGroupId = (int)progResponse.ProgressiveId, /* We are using the ProgressiveGroupId to store the unique HHR server side progressive ID*/
                LevelId = (int)progResponse.ProgLevel,
                Amount = progResponse.ProgCurrentValue,
                WagerCredits = progResponse.ProgCreditsBet,
                Expiration = DateTime.MaxValue, /*: Protocol has no concept of expiration*/
                CurrentErrorStatus = ProgressiveErrors.None
            };
        }

        /// <summary>
        ///     Helper function to get the Game map Id
        /// </summary>
        /// <param name="gameDataService"> Game data service</param>
        /// <returns>Game map id for the game if it exists, else default value</returns>
        public static async Task<uint> GetDefaultGameMapIdAsync(this IGameDataService gameDataService)
        {
            var gameInfo = (await gameDataService.GetGameInfo()).ToList();
            return gameInfo.FirstOrDefault()?.GameId ?? 0u;
        }

        /// <summary>
        ///  This gives the list of progressive levels that matches the wager in the prize information
        /// </summary>
        /// <param name="prizeInformation">Prize Information</param>
        /// <param name="protocolLinkedProgressiveAdapter"> Protocol Adapter</param>
        /// <param name="propertiesManager">Properties Manager</param>
        /// <returns></returns>
        public static IList<IViewableProgressiveLevel> GetActiveProgressiveLevelsForWager(
            this PrizeInformation prizeInformation,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter = null,
            IPropertiesManager propertiesManager = null)
        {
            if (protocolLinkedProgressiveAdapter == null)
            {
                protocolLinkedProgressiveAdapter =
                    ServiceManager.GetInstance().GetService<IProtocolLinkedProgressiveAdapter>();
            }

            if (propertiesManager == null)
            {
                propertiesManager =
                    ServiceManager.GetInstance().GetService<IPropertiesManager>();
            }

            var (_, currentDenom) = propertiesManager.GetActiveGame();

            return protocolLinkedProgressiveAdapter
                .GetActiveProgressiveLevels().Where(
                    x =>
                        x.WagerCredits * currentDenom.Value.MillicentsToCents() ==
                        prizeInformation.RaceSet1Wager + prizeInformation.RaceSet2Wager)
                .ToList();
        }

        /// <summary>
        ///     Helper function to extract outcomes from incoming prize information
        /// </summary>
        /// <param name="prizeInformation"> Prize information </param>
        /// <returns> An enumeration of Outcomes corresponding to prize information.</returns>
        public static IEnumerable<Outcome> ExtractOutcomes(this PrizeInformation prizeInformation)
        {
            var outcomes = new List<Outcome>();
            var normalAmountWon = prizeInformation.RaceSet1AmountWon + prizeInformation.RaceSet2AmountWonWithoutProgressives;
            var progressiveHits = prizeInformation.ProgressiveLevelsHit;
            var progressiveLevels = prizeInformation.ProgressiveLevelsHit.Any() ? prizeInformation.GetActiveProgressiveLevelsForWager() : new List<IViewableProgressiveLevel>();

            AddStandardGameOutcome();

            AddProgressiveOutcomes();

            AddExtraWinnings();

            return outcomes;

            void AddStandardGameOutcome()
            {
                outcomes.Add(
                    new Outcome(
                        prizeInformation.ReplyId,
                        (long)prizeInformation.ScratchTicketSetId,
                        (long)prizeInformation.ScratchTicketId,
                        OutcomeReference.Direct,
                        OutcomeType.Standard,
                        normalAmountWon.CentsToMillicents(),
                        0,
                        string.Empty)
                );
            }

            void AddProgressiveOutcomes()
            {
                progressiveHits.ForAll(
                    progHit =>
                    {
                        var index = 0;

                        var (levelId, count) = progHit;
                        // Server level IDs start from 1, Game Level IDs start from 0. Here, levelId from progressive Hit is used as both,
                        // the levelId and index in the win array. So, levelId 0 (in progressive hit list) corresponds to Server Level 1 and so on.
                        // Hence Game Level Id can be compared directly to levelId in progressive hit list.
                        var progressiveLevel = progressiveLevels.First(x => x.LevelId == levelId);
                        outcomes.AddRange(
                            Enumerable.Range(1, count).Select(
                                _ => new Outcome(
                                    prizeInformation.ReplyId,
                                    (long)prizeInformation.ScratchTicketSetId,
                                    (long)prizeInformation.ScratchTicketId,
                                    OutcomeReference.Direct,
                                    OutcomeType.Progressive,
                                    index++ == 0 ? ((long)prizeInformation.ProgressiveWin.ElementAt(levelId))
                                    .CentsToMillicents() : progressiveLevel.ResetValue, // Progressive won amount
                                    levelId, // Level Hit
                                    levelId.ToString(CultureInfo.InvariantCulture))));
                    });
            }

            void AddExtraWinnings()
            {
                var totalExtraWinnings = prizeInformation.RaceSet1ExtraWinnings + prizeInformation.RaceSet2ExtraWinnings;

                if (totalExtraWinnings > 0)
                {
                    outcomes.Add(
                        new Outcome(
                            prizeInformation.ReplyId,
                            (long)prizeInformation.ScratchTicketSetId,
                            (long)prizeInformation.ScratchTicketId,
                            OutcomeReference.Direct,
                            OutcomeType.Fractional,
                            ((long)totalExtraWinnings).CentsToMillicents(),
                            0,
                            string.Empty)
                    );
                }
            }
        }

        /// <summary>
        ///     Helper function to convert Race data into string representation
        /// </summary>
        /// <param name="prizeInformation"> Prize information </param>
        /// <returns> An enumeration of Outcomes corresponding to prize information.</returns>
        public static string ExtractGameRoundInfo(this PrizeInformation prizeInformation)
        {
            var info = new List<string>
            {
                $"Race Ticket Set = {prizeInformation.ScratchTicketSetId}",
                $"Race Ticket ID = {prizeInformation.ScratchTicketId}",
                $"Handicap: {(prizeInformation.RaceInfo.HandicapData == 0 ? "No" : "Yes")}"
            };

            if (prizeInformation.BOverride)
            {
                info.Add("Game overriden: Yes");
            }

            if (prizeInformation.RaceInfo.RaceData != null)
            {
                AddRaceSetData(prizeInformation.RaceInfo.RaceData.Take(RaceSetSize), 1);

                AddRaceSetData(prizeInformation.RaceInfo.RaceData.Skip(RaceSetSize), 2);
            }

            void AddRaceSetData(IEnumerable<CRaceData> raceData, int raceGroup)
            {
                info.Add($"Race Set {raceGroup}");

                var races = raceData.ToList();
                for (var i = 0; i < races.Count; i++)
                {
                    var raceIdx = (raceGroup - 1) * races.Count + i + 1;
                    info.Add($"RaceIndex: {raceIdx}, RaceNumber: {races[i].RaceNumber}, Track: {races[i].TrackDescription}, Date: {races[i].RaceDate}");
                    info.Add($"  Actual: {races[i].HorseActual}");
                    info.Add($"  Picked: {races[i].HorseSelection}");
                }

                info.Add($"Prize Take down: {(raceGroup == 1 ? prizeInformation.RaceInfo.PariPrize1 : prizeInformation.RaceInfo.PariPrize2)}");

                info.Add($"Prize: {(raceGroup == 1 ? prizeInformation.RaceInfo.PrizeRaceSet1 : prizeInformation.RaceInfo.PrizeRaceSet2)}");
            }

            return string.Join("\n", info);
        }

        /// <summary>
        ///     Helper function to get whether game play is allowed even when system is disabled (in background).
        /// </summary>
        /// <param name="systemDisableManager"> System disabled manager </param>
        /// <returns> True if game play allowed during system disabled, false otherwise.</returns>
        public static bool IsGamePlayAllowed(this ISystemDisableManager systemDisableManager)
        {
            if (systemDisableManager == null)
            {
                systemDisableManager =
                    ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            }
            return systemDisableManager.CurrentDisableKeys.Intersect(FilteredLockupsForGamePlay).Any();
        }
        
    }
}