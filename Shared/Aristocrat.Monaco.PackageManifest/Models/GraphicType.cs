namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Values that represent a graphic type
    /// </summary>
    public enum GraphicType
    {
        /// <summary>
        ///     Other graphics associated with a package.
        /// </summary>
        Other,

        /// <summary>
        ///     Icon that represents the package
        /// </summary>
        Icon,

        /// <summary>
        ///     Image that appears in the upper section of an end-point above the main screen.
        /// </summary>
        Glass,

        /// <summary>
        ///     Image that appears in the lower section of an end-point below the main screen.
        /// </summary>
        Graphic,

        /// <summary>
        ///     Image that appears on the main screen of an end-point.
        /// </summary>
        Screen,

        /// <summary>
        ///     Image that appears on the main screen when the game is loading.
        /// </summary>
        LoadingScreen,

        /// <summary>
        ///     Video graphics associated with a package.
        /// </summary>
        AttractVideo,

        /// <summary>
        ///     Image that represents the theme of a game.
        /// </summary>
        Theme,

        /// <summary>
        ///     Image that can be used for previewing different background colors.
        /// </summary>
        BackgroundPreview,

        /// <summary>
        ///     Image used as the background for a lobby denomination button.
        /// </summary>
        DenomButton,

        /// <summary>
        ///     Image used as the background panel for the lobby denomination buttons.
        /// </summary>
        DenomPanel
    }
}