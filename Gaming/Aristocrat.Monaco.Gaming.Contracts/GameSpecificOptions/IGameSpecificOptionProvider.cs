namespace Aristocrat.Monaco.Gaming.Contracts.GameSpecificOptions
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides an interface to save and retrieve game custom options
    ///     In order to use this interface in the protocol layer, use the IExtraSettingsConfigurationProvider
    /// </summary>
    public interface IGameSpecificOptionProvider
    {
        /// <summary>
        ///     Check if a key of Theme Id exists.
        /// </summary>
        /// <param name="themeId">themeId from gsaManifest</param>
        /// <returns>none</returns>
        bool HasThemeId(string themeId);

        /// <summary>
        ///     Get game specific options name/value pairs for GDK Runtime.
        /// </summary>
        /// <param name="themeId">themeId from gsaManifest</param>
        /// <returns>The resulting assignment operation</returns>
        public string GetCurrentOptionsForGDK(string themeId);

        /// <summary>
        ///     Get game specific options defined by a game.
        /// </summary>
        /// <param name="themeId">themeId from gsaManifest</param>
        /// <returns>The resulting assignment operation</returns>
        IEnumerable<GameSpecificOption> GetGameSpecificOptions(string themeId);

        /// <summary>
        ///     Set game specific options with key Theme Id.
        /// </summary>
        /// <param name="themeId">themeId from gsaManifest</param>
        /// <param name="options">The Game option list</param>
        /// <returns>none</returns>
        void InitGameSpecificOptionsCache(string themeId, IList<GameSpecificOption> options);

        /// <summary>
        ///     Set game specific options with key Theme Id.
        /// </summary>
        /// <param name="themeId">themeId from gsaManifest</param>
        /// <param name="options">The Game option list</param>
        /// <returns>none</returns>
        void UpdateGameSpecificOptionsCache(string themeId, IList<GameSpecificOption> options);
    }
}