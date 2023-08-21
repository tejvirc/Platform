namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     The lobby game graphics for a specific locale.
    /// </summary>
    public interface ILocaleGameGraphics
    {
        /// <summary>
        ///     Gets the language code for these graphics.
        /// </summary>
        string LocaleCode { get; }

        /// <summary>
        ///     Gets the file path for the large lobby icon for the game.
        /// </summary>
        string LargeIcon { get; }

        /// <summary>
        ///     Gets the file path for the small lobby icon for the game.
        /// </summary>
        string SmallIcon { get; }

        /// <summary>
        ///     Gets the file path for the large lobby top pick icon for the game.
        /// </summary>
        string LargeTopPickIcon { get; set; }

        /// <summary>
        ///     Gets the file path for the small lobby top pick icon for the game.
        /// </summary>
        string SmallTopPickIcon { get; set; }

        /// <summary>
        ///     Gets the file path for the topper attract video for the game.
        /// </summary>
        string TopperAttractVideo { get; set; }

        /// <summary>
        ///     Gets the file path for the top attract video for the game.
        /// </summary>
        string TopAttractVideo { get; }

        /// <summary>
        ///     Gets the file path for the bottom attract video for the game.
        /// </summary>
        string BottomAttractVideo { get; }

        /// <summary>
        ///     Gets the file path for the loading screen for the game.
        /// </summary>
        string LoadingScreen { get; }

        /// <summary>
        ///     Gets the file path for the background preview images.
        /// </summary>
        IReadOnlyCollection<(string Color, string BackgroundFilePath)> BackgroundPreviewImages { get; set; }

        /// <summary>
        ///      Gets the file path for the Player Information Display images.
        /// </summary>
        IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> PlayerInfoDisplayResources { get; set; }
    }
}
