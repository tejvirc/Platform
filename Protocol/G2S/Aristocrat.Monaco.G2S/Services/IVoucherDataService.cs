namespace Aristocrat.Monaco.G2S.Services
{
    using Accounting.Contracts;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;

    /// <summary>
    ///     Service for maintaining voucher validation data.
    /// </summary>
    public interface IVoucherDataService
    {
        /// <summary>
        ///     Gets the host online flag.
        /// </summary>
        bool HostOnline { get; }

        /// <summary>
        ///     Starts the service
        /// </summary>
        void Start();

        /// <summary>
        ///     Checks Host transport state for the voucher device owner.
        /// </summary>
        /// <param name="hostId">Host If</param>
        /// <param name="online">Online flag.</param>
        void CommunicationsStateChanged(int hostId, bool online);

        /// <summary>
        ///     Gets voucher data.
        /// </summary>
        /// <returns>Voucher data.</returns>
        VoucherData GetVoucherData();

        /// <summary>
        ///     Reads last voucher data.
        /// </summary>
        /// <returns>Voucher data.</returns>
        VoucherData ReadVoucherData();

        /// <summary>
        ///     Gets number of available voucher Ids.
        /// </summary>
        /// <returns>Number of available voucher Ids.</returns>
        int VoucherIdAvailable();

        /// <summary>
        ///     Send redeem voucher request to host.
        /// </summary>
        /// <param name="transactionId">Transaction Id.</param>
        /// <param name="validationId">Validation Id.</param>
        /// <param name="logSequence">Log sequence.</param>
        /// <param name="amount">Amount.</param>
        /// <returns>Authorized voucher and the voucher log</returns>
        (authorizeVoucher, voucherLog) SendRedeemVoucher(long transactionId, string validationId, long logSequence, long amount);

        /// <summary>
        ///     Send commit voucher command to host.
        /// </summary>
        /// <param name="voucherInTransaction">Voucher transaction.</param>
        void SendCommitVoucher(VoucherInTransaction voucherInTransaction);

        /// <summary>
        ///     Reset the Voucher Data service for the host state.
        /// </summary>
        void VoucherStateChanged();
    }
}