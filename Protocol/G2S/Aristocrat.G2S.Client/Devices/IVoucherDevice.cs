namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Defines the IVoucherDevice interface
    /// </summary>
    public interface IVoucherDevice : IDevice, ISingleDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets a value for the Id Reader Id.
        /// </summary>
        int IdReaderId { get; }

        /// <summary>
        ///     Gets a value indicating whether Combine Cashable Out.
        /// </summary>
        bool CombineCashableOut { get; }

        /// <summary>
        ///     Gets a value for the Max Val Id.
        /// </summary>
        int MaxValueIds { get; }

        /// <summary>
        ///     Gets a value for the Min Level Val Ids.
        /// </summary>
        int MinLevelValueIds { get; }

        /// <summary>
        ///     Gets a value for the Val Id List Refresh.
        /// </summary>
        int ValueIdListRefresh { get; }

        /// <summary>
        ///     Gets or sets a value for the ValId List Life.
        /// </summary>
        int ValueIdListLife { get; set; }

        /// <summary>
        ///     Gets a value for the Voucher Hold Time.
        /// </summary>
        int VoucherHoldTime { get; }

        /// <summary>
        ///     Gets a value indicating whether the Print Off Line.
        /// </summary>
        bool PrintOffLine { get; }

        /// <summary>
        ///     Gets a value for the Expire Cash Promo.
        /// </summary>
        int ExpireCashPromo { get; }

        /// <summary>
        ///     Gets a value indicating whether the Print Exp Cash Promo.
        /// </summary>
        bool PrintExpirationCashPromo { get; }

        /// <summary>
        ///     Gets a value for the Expire Non Cash.
        /// </summary>
        int ExpireNonCash { get; }

        /// <summary>
        ///     Gets a value indicating whether the Print Exp Non Cash.
        /// </summary>
        bool PrintExpirationNonCash { get; }

        /// <summary>
        ///     Gets a value for the Max On Line Pay Out.
        /// </summary>
        long MaxOnLinePayOut { get; }

        /// <summary>
        ///     Gets a value for the Max Off Line Pay Out.
        /// </summary>
        long MaxOffLinePayOut { get; }

        /// <summary>
        ///     Gets a value indicating whether the Print Non Cash Off Line.
        /// </summary>
        bool PrintNonCashOffLine { get; }

        /// <summary>
        ///     Gets a value indicating whether the Cash Out To Voucher.
        /// </summary>
        bool CashOutToVoucher { get; }

        /// <summary>
        ///     Gets a value for minimum script log entries.
        /// </summary>
        int MinLogEntries { get; }

        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Sends issueVoucher command to host.
        /// </summary>
        /// <param name="voucher">Issued voucher command.</param>
        /// <param name="checkAcknowledgedCallBack">Check transaction for acknowledged by host.</param>
        /// <param name="acknowledgedCallBack">Callback with transfer Id</param>
        /// <returns>Task</returns>
        Task SendIssueVoucher(
            issueVoucher voucher,
            Func<long, bool> checkAcknowledgedCallBack,
            Action<long> acknowledgedCallBack);

        /// <summary>
        ///     Sends redeemVoucher command to host.
        /// </summary>
        /// <param name="voucher">Redeem voucher command.</param>
        /// <param name="log">Voucher log.</param>
        /// <param name="triggerEvent">Flag to create event report.</param>
        /// <param name="endDateTime">Voucher redeem end time.</param>
        /// <returns>authorizeVoucher host command.</returns>
        Task<authorizeVoucher> SendRedeemVoucher(
            redeemVoucher voucher,
            voucherLog log,
            bool triggerEvent = true,
            DateTime endDateTime = default(DateTime));

        /// <summary>
        ///     Sends commitVoucher command to host.
        /// </summary>
        /// <param name="voucher">Commit voucher command.</param>
        /// <param name="log">Voucher log</param>
        /// <param name="ackCallback">Ack call back.</param>
        /// <param name="getMetersCallBack">Gets meterList.</param>
        /// <param name="triggerEvent">Flag to report events.</param>
        /// <returns>Task</returns>
        Task SendCommitVoucher(
            commitVoucher voucher,
            voucherLog log,
            Action<commitVoucher> ackCallback,
            Func<IVoucherDevice, meterList> getMetersCallBack,
            bool triggerEvent = true);

        /// <summary>
        ///     Gets session with new validation data from host.
        /// </summary>
        /// <param name="numberValidationIds">Number of validation Ids.</param>
        /// <param name="isValidationIdListExpired">Flag for validation Id expired.</param>
        /// <param name="validationListId">Validation list Id</param>
        /// <param name="result">Validation data result.</param>
        /// <returns>Success</returns>
        bool GetValidationData(
            int numberValidationIds,
            bool isValidationIdListExpired,
            long validationListId,
            Action<validationData, IVoucherDevice> result);
    }
}
