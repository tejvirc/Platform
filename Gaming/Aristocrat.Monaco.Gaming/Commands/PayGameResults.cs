namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Pay game result command
    /// </summary>
    public class PayGameResults
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PayGameResults" /> class.
        /// </summary>
        /// <param name="win">The final win amount</param>
        public PayGameResults(long win)
        {
            Win = win;
        }

        /// <summary>
        ///     Gets the final win amount
        /// </summary>
        public long Win { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not there is a transaction associated with the execution of this
        ///     command.  Transactions include (cash outs, bonuses, hoppers, etc.)
        /// </summary>
        public bool PendingTransaction { get; set; }
    }
}