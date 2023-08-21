namespace Aristocrat.Monaco.Bingo.UI.Models
{
    /// <summary>
    ///     Bingo appearance details
    /// </summary>
    /// <remarks>
    ///     All color names are from Unix X11 standard, see
    ///     https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors
    /// </remarks>
    public class BingoAppearance
    {
        /// <summary>
        ///     Constructor sets defaults.
        /// </summary>
        public BingoAppearance()
        {
        }

        /// <summary>
        ///     Color name of bingo card numbers, initially
        /// </summary>
        public string CardInitialNumberColor { get; set; }

        /// <summary>
        ///     Color name of bingo card numbers and ball call numbers when daubed
        /// </summary>
        public string DaubNumberColor { get; set; }

        /// <summary>
        ///     Color name of ball call numbers in initial draw
        /// </summary>
        public string BallsEarlyNumberColor { get; set; }

        /// <summary>
        ///     Color name of ball call numbers after game end
        /// </summary>
        public string BallsLateNumberColor { get; set; }
    }
}