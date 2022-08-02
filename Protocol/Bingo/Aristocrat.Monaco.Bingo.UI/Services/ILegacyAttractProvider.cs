namespace Aristocrat.Monaco.Bingo.UI.Services
{
    using System;
    using OverlayServer.Data.Bingo;

    /// <summary>
    ///     The provider for the legacy attract mode handling for bingo
    /// </summary>
    public interface ILegacyAttractProvider
    {
        /// <summary>
        ///     Gets the legacy attract URI if one is enabled otherwise null be returned
        /// </summary>
        /// <param name="attractSettings">The settings to use for attract</param>
        /// <returns>The legacy attract URI or null if attract is disabled</returns>
        Uri GetLegacyAttractUri(BingoDisplayConfigurationBingoAttractSettings attractSettings);
    }
}