namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System.Threading;
    using Contracts.Persistence;

    /// <summary> A scoped transaction holder. </summary>
    public static class ScopedTransactionHolder
    {
        /// <summary> Gets or sets the transaction. </summary>
        /// <value> The transaction. </value>
        private static readonly ThreadLocal<ScopedTransaction> Transaction =
            new ThreadLocal<ScopedTransaction>(() => null);

        /// <summary> Gets the transaction. </summary>
        /// <returns> The transaction. </returns>
        public static ScopedTransaction CreateTransaction(IKeyAccessor accessor)
        {
            var transaction = ActiveTransaction();
            if (transaction != null)
            {
                return transaction;
            }

            Transaction.Value = new ScopedTransaction(accessor);
            Transaction.Value.Completed += (_, _) => { Transaction.Value = null; };
            return Transaction.Value;
        }

        /// <summary> Active transaction. </summary>
        /// <returns> A ScopedTransaction. </returns>
        public static ScopedTransaction ActiveTransaction()
        {
            if (!Transaction.IsValueCreated || Transaction.Value == null)
            {
                return null;
            }

            return Transaction.Value;
        }
    }
}