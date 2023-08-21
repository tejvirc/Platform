namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Describes the behavior of the evice in relation to what is already being displayed on the screen
    /// </summary>
    public enum DisplayPosition
    {
        /// <summary>
        ///     The media display is positioned on the left side of the screen
        /// </summary>
        Left,

        /// <summary>
        ///     The media display is positioned on the right side of the screen
        /// </summary>
        Right,

        /// <summary>
        ///     The media display is positioned on the top section of the screen
        /// </summary>
        Top,

        /// <summary>
        ///     The media display is positioned on the bottom section of the screen
        /// </summary>
        Bottom,

        /// <summary>
        ///     The media display is positioned to cover the entire screen
        /// </summary>
        FullScreen,

        /// <summary>
        ///     The media display is positioned in the center of the screen
        /// </summary>
        CenterScreen,

        /// <summary>
        ///     The media display is positioned somewhere on the screen but cannot be accurately described by one of the above
        ///     options
        /// </summary>
        Floating
    }
}
