namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    /// <summary>
    ///     An interface used to state that a transaction requires acknowledgement from the host
    /// </summary>
    public interface IAcknowledgeableTransaction : ITransaction
    {
        /// <summary>
        ///     Gets or sets the host sequence ID
        /// </summary>
        long HostSequence { get; set; }
    }
}