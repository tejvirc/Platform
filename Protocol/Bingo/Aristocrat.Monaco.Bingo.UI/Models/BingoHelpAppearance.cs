namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Windows;

    /// <summary>
    ///     Bingo help appearance details
    /// </summary>
    [Serializable]
    public class BingoHelpAppearance
    {
        /// <summary>
        ///     Box defining the location and size of the help workspace
        /// </summary>
        public Thickness HelpBox { get; set; }
    }
}
