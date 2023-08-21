namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Media Display Screen types
    /// </summary>
    public enum ScreenType
    {
        /// <summary>
        ///     The primary screen on the device. On an EGM, this would be the game screen
        /// </summary>
        Primary,

        /// <summary>
        ///     The secondary screen is a generic term used to define any/all of the screens other than the primary screen. Whenever
        ///     possible the more specific enumeration SHOULD be used
        /// </summary>
        Secondary,

        /// <summary>
        ///     The screen located above the primary screen (game screen) while still part of the main cabinet
        /// </summary>
        Glass,

        /// <summary>
        ///     The screen used as a button deck
        /// </summary>
        ButtonDeck
    }
}