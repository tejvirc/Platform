namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent to the VLT Service upon successful printing of a voucher. 
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell TODO: to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class VoucherPrinted : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the VoucherBarcode.
        /// </summary>
        public string VoucherBarcode { get; set; }
    }
}
