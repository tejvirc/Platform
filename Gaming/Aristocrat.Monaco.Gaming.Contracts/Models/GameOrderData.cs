namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Model for game order.
    /// </summary>
    public class GameOrderData
    {
        /// <summary>
        ///     Gets or sets the game game Id.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the game theme Id.
        /// </summary>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the game theme name.
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        ///     Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the paytable name.
        /// </summary>
        public string Paytable { get; set; }

        /// <summary>
        ///     Gets or sets the game version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets the installation date
        /// </summary>
        public DateTime Installed { get; set; }

        /// <summary>
        ///     Gets or sets the game tag strings
        /// </summary>
        public ObservableCollection<string> GameTags { get; set; }

        /// <summary>
        ///     Gets or sets the TheoPaybackPct property
        /// </summary>
        public decimal TheoPaybackPct { get; set; }

        /// <summary>
        ///     Gets or sets the Theoretical Payback Percent display property
        /// </summary>
        public string TheoPaybackPctDisplay { get; set; }
    }
}
