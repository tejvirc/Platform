namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Game asset tags used in the GSA Manifest to describe additional information about the assets.
    /// </summary>
    public static class GameAssetTags
    {
        /// <summary>
        ///     Indicates that the asset has a portrait aspect ratio.
        /// </summary>
        public const string PortraitTag = "portrait";

        /// <summary>
        ///     Indicates that the asset has a landscape aspect ratio.
        /// </summary>
        public const string LandscapeTag = "landscape";

        /// <summary>
        ///     Indicates that the asset belongs on the Topper Screen.
        ///     The Display order of cabinet is - Topper, Top, Main, Bottom, VBD
        /// </summary>
        public const string TopperTag = "topper";

        /// <summary>
        ///     Indicates that the asset belongs on the Top Screen.
        ///     The Display order of cabinet is - Topper, Top, Bottom
        /// </summary>
        public const string TopTag = "top";

        /// <summary>
        ///     Indicates that the asset belongs on the Bottom Screen.
        ///     The Display order of cabinet is - Topper, Top, Bottom
        /// </summary>
        public const string BottomTag = "bottom";

        /// <summary>
        ///     Indicates that the game's icon has
        ///     top pic banner.
        /// </summary>
        public const string TopPickTag = "toppick";

        /// <summary>
        ///     Indicates that the asset is background.
        /// </summary>
        public const string BackgroundTag = "background";

        /// <summary>
        ///     Indicates that the asset is image button.
        /// </summary>
        public const string ButtonTag = "button";

        /// <summary>
        ///     Indicates that the asset is dialog/screen.
        /// </summary>
        public const string ScreenTag = "screen";

        /// <summary>
        ///     Indicates that the asset is used for exit controls.
        /// </summary>
        public const string ExitTag = "exit";

        /// <summary>
        ///     Indicates that the asset is Game Info.
        /// </summary>
        public const string GameInfoTag = "gameinfo";

        /// <summary>
        ///     Indicates that the asset is Game Rules.
        /// </summary>
        public const string GameRulesTag = "gamerules";

        /// <summary>
        ///     Indicates that the asset is used for pressed state.
        /// </summary>
        public const string PressedTag = "pressed";

        /// <summary>
        ///     Indicates that the asset is used for normal state.
        /// </summary>
        public const string NormalTag = "normal";

        /// <summary>
        ///     Indicates that the asset belongs on the Player Information Display Menu screens.
        /// </summary>
        public const string PlayerInformationDisplayMenuTag = "pid_menu";

        /// <summary>
        ///     A collection of all known tags belong on the Player Information Display screens
        /// </summary>
        public static readonly HashSet<string> PlayerInformationDisplayScreensTags = new HashSet<string> { PlayerInformationDisplayMenuTag };

        /// <summary>
        ///     A collection of all known tags for lobby screen
        /// </summary>
        public static readonly HashSet<string> DisplayAndOrientationTags = new HashSet<string> { PortraitTag, LandscapeTag, TopperTag, TopTag, BottomTag};
    }
}