namespace Aristocrat.Monaco.Hhr.UI
{
    using System;

    public static class HhrUiConstants
    {
        /// <summary>
        /// The number of positions (green containers) into which the horse numbers get placed
        /// </summary>
        public const int NumberOfHorsePickPositions = 8;

        /// <summary>
        /// The maximum horse number
        /// </summary>
        public const int MaxNumberOfHorses = 12;

        /// <summary>
        /// The number of races per race set
        /// </summary>
        public const int NumberOfRacesPerRaceSet = 5;

        /// <summary>
        ///     Lockup to show until RaceInformation is available from server.
        /// </summary>
        public static Guid WaitingForRaceInfoGuid = new Guid("95465F6A-2BA9-468E-A76E-8DAB08CCC47B");

        /// <summary>
        ///     Lockup to show until WinningCombinationsInfo is available from server.
        /// </summary>
        public static Guid WaitingForWinningCombinationInfo = new Guid("F542922F-35F9-48D8-8D5E-68EFBD2B7217");

        /// <summary>
        /// The number of seconds for which the placard (info regarding time for manual handicapping is expire ) flashes
        /// </summary>
        public const int TimerExpirePlacardTimeout = 5;
    }
}
