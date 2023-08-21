namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Describes the behavior of the device in relation to what is already being displayed on the screen
    /// </summary>
    public enum DisplayType
    {
        /// <summary>
        ///     The media display is displayed while the existing content is scaled to fit in the remaining area on the screen not
        ///     taken up by the media display. Everything that was showing on the screen before the media display was added should
        ///     still be visible (and playable) after the media display is added to the screen
        /// </summary>
        Scale,

        /// <summary>
        ///     The media display is displayed in front of any game content already displayed on the screen. This type occludes
        ///     anything being displayed by the game in the area that the media display takes up including anything critical (game
        ///     outcomes, meters, etc.)
        /// </summary>
        Overlay
    }
}