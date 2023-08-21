namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to credit an existing open session with a voucher. 
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell TODO: to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    ///     Implementing <see cref="ISessionId"/> 
    /// </remarks>
    public class CreditVoucher : Request, IInstanceId, ISessionId, ILocalTransactionId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the SessionId.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the VoucherBarcode.
        /// </summary>
        public string VoucherBarcode { get; set; }

        /// <summary>
        /// Gets or sets the LocalTransactionId.
        /// </summary>
        public int LocalTransactionId { get; set; }
    }
}
