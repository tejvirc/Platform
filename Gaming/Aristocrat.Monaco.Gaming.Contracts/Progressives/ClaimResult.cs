namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     The result of a progressive claim
    /// </summary>
    public class ClaimResult
    {
        /// <summary>
        ///     Gets or sets the game id of the progressive claim result
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the denom of the progressive claim result
        /// </summary>
        public long DenomId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name of the progressive claim result
        /// </summary>
        public string ProgressivePackName { get; set; }

        /// <summary>
        ///     Gets or sets the level id of the progressive claim result
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the win amount of the progressive claim result
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the win text for the progressive claim result
        /// </summary>
        public string WinText { get; set; }
    }
}