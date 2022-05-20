namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Game result command
    /// </summary>
    public class SetGameResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetGameResult" /> class.
        /// </summary>
        /// <param name="win">The final win amount</param>
        public SetGameResult(long win)
        {
            Win = win;
        }

        /// <summary>
        ///     Gets the final win amount
        /// </summary>
        public long Win { get; }
    }
}
