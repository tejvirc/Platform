namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Amalgam of window-specific bingo definitions.
    /// </summary>
    [Serializable]
    public class BingoWindowSettings
    {
        /// <summary>
        ///     Ball Call title.
        /// </summary>
        public string BallCallTitle { get; set; }

        /// <summary>
        ///     Card title.
        /// </summary>
        public string CardTitle { get; set; }

        /// <summary>
        ///     Default Scene
        /// </summary>
        public string InitialScene { get; set; }

        /// <summary>
        ///     Disclaimer text
        /// </summary>
        public List<string> DisclaimerText { get; set; }

        /// <summary>
        ///     Get or set the appearance details.
        /// </summary>
        public string FreeSpaceCharacter { get; set; }

        /// <summary>
        ///     Enable 0 padding for bingo card numbers
        /// </summary>
        public bool Allow0PaddingBingoCard { get; set; }

        /// <summary>
        ///     Enable 0 padding for ball call numbers
        /// </summary>
        public bool Allow0PaddingBallCall { get; set; }

        /// <summary>
        ///     Css path for the game
        /// </summary>
        public string CssPath { get; set; }

        /// <summary>
        ///     Pattern cycle period (ms)
        /// </summary>
        public int PatternCyclePeriod { get; set; }

        /// <summary>
        ///     Gets or sets The minimum time in milliseconds to have the card undaubed before showing any daubs
        /// </summary>
        public int MinimumPreDaubedTimeMs { get; set; }

        /// <summary>
        ///     Gets or sets the waiting for game message
        /// </summary>
        public string WaitingForGameMessage { get; set; }

        /// <summary>
        ///     Gets or sets the waiting for game timeout message
        /// </summary>
        public string WaitingForGameTimeoutMessage { get; set; }

        /// <summary>
        ///     Gets or sets the number of seconds to wait before showing the waiting for game message
        /// </summary>
        public double WaitingForGameDelaySeconds { get; set; }

        /// <summary>
        ///     Gets or sets the number of seconds to display the waiting for game timeout message
        /// </summary>
        public double WaitingForGameTimeoutDisplaySeconds { get; set; }

        /// <summary>
        ///     Gets or sets the daub time for the bingo patterns
        /// </summary>
        public BingoDaubTime PatternDaubTime { get; set; }
    }
}
