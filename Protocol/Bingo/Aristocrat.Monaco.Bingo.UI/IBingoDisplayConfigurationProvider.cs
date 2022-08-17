namespace Aristocrat.Monaco.Bingo.UI
{
    using System.Collections.Generic;
    using System.Windows;
    using Models;
    using OverlayServer.Data.Bingo;

    /// <summary>
    ///     Provide bingo display configuration data
    /// </summary>
    public interface IBingoDisplayConfigurationProvider
    {
        /// <summary>
        ///     Gets the presentation override message formats
        /// </summary>
        /// <returns>The presentation override message formats</returns>
        List<BingoDisplayConfigurationPresentationOverrideMessageFormat> GetPresentationOverrideMessageFormats();

        /// <summary>
        ///     Get help appearance
        /// </summary>
        /// <returns>Config data for help</returns>
        BingoDisplayConfigurationHelpAppearance GetHelpAppearance();

        /// <summary>
        ///     Gets the bingo attract settings
        /// </summary>
        /// <returns>The attract settings for this game</returns>
        BingoDisplayConfigurationBingoAttractSettings GetAttractSettings();

        /// <summary>
        ///     Get config data for one window enum
        /// </summary>
        /// <param name="window">Which window</param>
        /// <returns>Config data for one window enum</returns>
        BingoDisplayConfigurationBingoWindowSettings GetSettings(BingoWindow window);

        /// <summary>
        ///     Get <see cref="Window"/> for one window enum
        /// </summary>
        /// <param name="window">Which window</param>
        /// <returns><see cref="Window"/> for a window enum</returns>
        Window GetWindow(BingoWindow window);

        /// <summary>
        ///     The current window enum
        /// </summary>
        BingoWindow CurrentWindow { get; set; }

        /// <summary>
        ///     Load config data from a file.
        /// </summary>
        /// <param name="path">File path</param>
        void LoadFromFile(string path);

#if !RETAIL
        /// <summary>
        ///     Temporarily override help config data.
        /// </summary>
        /// <param name="helpAppearance">Config data for help</param>
        void OverrideHelpAppearance(BingoDisplayConfigurationHelpAppearance helpAppearance);

        /// <summary>
        ///     Temporarily override config data for one window.
        /// </summary>
        /// <param name="window">Which window</param>
        /// <param name="settings">Config data for one window</param>
        void OverrideSettings(BingoWindow window, BingoDisplayConfigurationBingoWindowSettings settings);

        /// <summary>
        ///     Restore original config data for one window.
        /// </summary>
        /// <param name="window">Which window</param>
        void RestoreSettings(BingoWindow window);

        /// <summary>
        ///     Save config data to a file.
        /// </summary>
        /// <param name="path">File path</param>
        void SaveToFile(string path);
#endif

        /// <summary>
        ///     Called when the lobby is initialized
        /// </summary>
        void LobbyInitialized();

        /// <summary>
        ///     Gets the version of the BingoDisplayConfiguration xml
        /// </summary>
        /// <returns>The version</returns>
        int GetVersion();
    }
}
