namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    public class LongPollExtendedGameNInformationResponse : LongPollMultiDenomAwareResponse
    {
        /// <summary>
        ///     The Extended Game N Information Response
        /// </summary>
        /// <param name="maxBet">Max Bet</param>
        /// <param name="progressiveGroup">SAS Progressive Group</param>
        /// <param name="progressiveLevels">SAS Progressive levels, bit set with lsb=level 1, msb=level 32</param>
        /// <param name="numberOfWagerCategories">Number of Wager Categories</param>
        /// <param name="denominations">SAS Denomination Codes</param>
        public LongPollExtendedGameNInformationResponse(
            int maxBet,
            byte progressiveGroup,
            uint progressiveLevels,
            int numberOfWagerCategories,
            IReadOnlyCollection<byte> denominations)
            : this(
                maxBet,
                progressiveGroup,
                progressiveLevels,
                string.Empty,
                string.Empty,
                numberOfWagerCategories,
                denominations)
        {
        }

        /// <summary>
        ///     The Extended Game N Information Response
        /// </summary>
        /// <param name="maxBet">Max Bet</param>
        /// <param name="progressiveGroup">SAS Progressive Group</param>
        /// <param name="progressiveLevels">SAS Progressive levels, bit set with lsb=level 1, msb=level 32</param>
        /// <param name="gameName">Game Name (optional)</param>
        /// <param name="paytableName">Paytable Name (optional</param>
        /// <param name="numberOfWagerCategories">Number of Wager Categories</param>
        /// <param name="denominations">SAS Denomination Codes</param>
        public LongPollExtendedGameNInformationResponse(
            int maxBet,
            byte progressiveGroup,
            uint progressiveLevels,
            string gameName,
            string paytableName,
            int numberOfWagerCategories,
            IReadOnlyCollection<byte> denominations)
        {
            MaxBet = maxBet;
            ProgressiveGroup = progressiveGroup;
            ProgressiveLevels = progressiveLevels;
            GameName = gameName;
            PaytableName = paytableName;
            NumberOfWagerCategories = numberOfWagerCategories;
            Denominations = denominations;
        }

        /// <summary>Gets the max bet</summary>
        public int MaxBet { get; }

        /// <summary>Gets the SAS progressive group</summary>
        public byte ProgressiveGroup { get; }

        /// <summary>Gets the SAS progressive levels (lsb=level 1, msb=level 32)</summary>
        public uint ProgressiveLevels { get; }

        /// <summary>Gets the game name</summary>
        public string GameName { get; }

        /// <summary>Gets the paytable name</summary>
        public string PaytableName { get; }

        /// <summary>Gets the number of wager categories</summary>
        public int NumberOfWagerCategories { get; }

        /// <summary>Gets the SAS denomination codes</summary>
        public IReadOnlyCollection<byte> Denominations { get; }
    }
}
