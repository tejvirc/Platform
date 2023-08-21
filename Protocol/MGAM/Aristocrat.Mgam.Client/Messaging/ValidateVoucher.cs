namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to determine whether or not a voucher bar code represents a
    ///     valid voucher.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Voucher.VoucherService"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class ValidateVoucher : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the VoucherBarcode.
        /// </summary>
        public string VoucherBarcode { get; set; }
    }
}
