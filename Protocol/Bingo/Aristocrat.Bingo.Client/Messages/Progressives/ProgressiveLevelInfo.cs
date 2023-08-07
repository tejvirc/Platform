namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Definition of ProgressiveLevelInfo
    /// </summary>
    public class ProgressiveLevelInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveLevelInfo"/> class.
        /// </summary>
        /// <param name="progressiveLevel">Progressive level id</param>
        /// <param name="sequenceNumber">Progressive 1-based sequence number</param>
        /// <param name="gameTitleId">The game title id</param>
        /// <param name="denomination">the game denomination</param>
        public ProgressiveLevelInfo(
            long progressiveLevel,
            int sequenceNumber,
            int gameTitleId,
            long denomination)
        {
            ProgressiveLevel = progressiveLevel;
            SequenceNumber = sequenceNumber;
            GameTitleId = gameTitleId;
            Denomination = denomination;
        }

        /// <summary>
        ///     The progressive level id
        /// </summary>
        public long ProgressiveLevel { get; }

        /// <summary>
        ///     The progressive 1-based sequence number
        /// </summary>
        public int SequenceNumber { get; }

        /// <summary>
        ///     The game title id that uses the progressive
        /// </summary>
        public int GameTitleId { get;  }

        /// <summary>
        ///     The game denomination that uses the progressive
        /// </summary>
        public long Denomination { get; }
    }
}
