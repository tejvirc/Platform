namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     Transaction Types
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        ///     Type of a transaction that may involve writing meter values
        /// </summary>
        Write,

        /// <summary>
        ///     Type of a transaction which only reads meter values
        /// </summary>
        Read
    }
}