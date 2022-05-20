namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Event arguments for scoped transaction related events
    /// </summary>
    public class TransactionEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionEventArgs" /> class.
        /// </summary>
        /// <param name="committed">true if committed</param>
        public TransactionEventArgs(bool committed)
        {
            Committed = committed;
        }

        /// <summary>
        ///     Gets a value indicating whether or not the transaction was committed
        /// </summary>
        public bool Committed { get; }
    }
}