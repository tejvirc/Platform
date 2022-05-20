namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     All display configuration data for Bingo.
    /// </summary>
    [Serializable]
    public class BingoDisplayConfiguration
    {
        /// <summary>
        ///     Bingo Info settings for each window.
        /// </summary>
        public List<BingoWindowSettings> BingoInfoWindowSettings { get; set; }

        /// <summary>
        ///     Bingo help appearance.
        /// </summary>
        public BingoHelpAppearance HelpAppearance { get; set; }

        /// <summary>
        ///     Bingo attract settings
        /// </summary>
        public BingoAttractSettings BingoAttractSettings { get; set; }
    }
}
