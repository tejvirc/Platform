namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to the site controller to begin a session with a known SessionID.
    ///     This should be used in off-nominal cases where the VLT has disconnected and needs to resume play with an
    ///     existing session.  If VoucherPrintedOffLine is true, the Site Controller will perform an implicit EndSession
    ///     (VLT does not have to issue an EndSession message), and thus require the VLT to then issue a BeginSessionWithCash/Voucher
    ///     if a new session is desired. 
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Voucher.BeginSessionWithSessionID"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class BeginSessionWithSessionId : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the Existing Session Id.
        /// </summary>
        public int ExistingSessionId { get; set; }

        /// <summary>
        /// Gets or sets the VoucherPrintedOffLine.
        /// </summary>
        public bool VoucherPrintedOffLine { get; set; }

        /// <summary>
        /// Gets or sets the PrintedOffLineVoucherBarcode.
        /// </summary>
        public string PrintedOffLineVoucherBarcode { get; set; }
    }
}
