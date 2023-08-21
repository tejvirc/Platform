namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    /// <summary>Interface for transaction reports.</summary>
    public interface ITransactionSource
    {
        /// <summary>Gets or sets the transaction ID.</summary>
        /// <value>The transaction ID.</value>
        byte TransactionId { get; set; }
    }
}
