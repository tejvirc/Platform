namespace Aristocrat.Monaco.Bingo.UI.Models
{
    /// <summary>
    ///     An enum for when to start daubing bingo patterns
    /// </summary>
    public enum BingoDaubTime
    {
        /// <summary>
        ///     Pattern is shown and daubed at the end of the presentation
        /// </summary>
        PresentationEnd,

        /// <summary>
        ///     Pattern is shown and daubed at the start of the presentation
        /// </summary>
        PresentationStart,

        /// <summary>
        ///     Pattern is shown and daubed at the start of the win roll up presentation
        /// </summary>
        WinPresentationStart
    }
}