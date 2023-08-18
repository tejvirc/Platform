namespace Aristocrat.Monaco.Common
{
    /// <summary>
    ///     Common constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>The default screen height in pixels.</summary>
        public static readonly string DefaultWindowedHeight = "1080";

        /// <summary>The default screen width in pixels.</summary>
        public static readonly string DefaultWindowedWidth = "1920";

        /// <summary>
        ///     The property name from command line arguments for the windowed screen width
        /// </summary>
        public static readonly string WindowedScreenWidthPropertyName = "width";

        /// <summary>
        ///     The property name from command line arguments for the windowed screen height
        /// </summary>
        public static readonly string WindowedScreenHeightPropertyName = "height";

        /// <summary>
        ///     Key for the display property in property manager.
        /// </summary>
        public const string DisplayPropertyKey = "display";

        /// <summary>
        ///     Key for the allowWindowedScaling property in property manager, which allows override
        ///     of the default behavior where windowed displays don't scale to the window size.
        /// </summary>
        public const string AllowWindowedScalingKey = "allowWindowedScaling";

        /// <summary>
        ///     Default value for the display property in property manager.
        /// </summary>
        public const string DisplayPropertyFullScreen = "FULLSCREEN";

        /// <summary>
        ///     Key for the showTestTool property in property manager.
        /// </summary>
        public const string ShowTestTool = "showTestTool";

        /// <summary>
        ///     Key for the showMouseCursor property in property manager.
        /// </summary>
        public const string ShowMouseCursor = "showMouseCursor";

        /// <summary>
        ///     False property value.
        /// </summary>
        public const string False = "FALSE";

        /// <summary>
        ///     False property value.
        /// </summary>
        public const string True = "TRUE";
    }
}