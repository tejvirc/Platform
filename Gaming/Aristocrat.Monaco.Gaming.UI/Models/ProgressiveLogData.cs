namespace Aristocrat.Monaco.Gaming.UI.Models
{
    /// <summary>
    ///     Definition of the ProgressiveLogData class.
    ///     This class stores data from one ProgressiveHitEvent.
    /// </summary>
    public class ProgressiveLogData 
    {
        /// <summary>
        ///     Gets or sets the date and time the progressive was hit.
        /// </summary>
        public string TransactionDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the amount of the progressive hit.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        ///     Gets or sets the level Id of the progressive hit.
        /// </summary>
        public string LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive Id of the progressive hit.
        /// </summary>
        public string ProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the game name of the progressive hit.
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        ///     Gets or sets the unique game Id of the progressive hit.
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        ///     Gets or sets the denom Id of the progressive hit.
        /// </summary>
        public string DenomId { get; set; }

        /// <summary>
        ///     Gets or sets the increment rate of the progressive hit.
        /// </summary>
        public string IncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the reset value of the progressive hit.
        /// </summary>
        public string ResetValue { get; set; }

        /// <summary>
        ///     Gets or sets the max value of the progressive hit.
        /// </summary>
        public string MaxValue { get; set; }
    }
}
