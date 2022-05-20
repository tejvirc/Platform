namespace Aristocrat.Monaco.Accounting
{
    /// <summary>
    ///     Defines metadata used during the transfer out process.
    /// </summary>
    public class TransferOutContext : ITransferOutContext
    {
        /// <inheritdoc />
        public string Barcode { get; set; }
    }
}
