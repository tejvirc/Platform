namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts;

    /// <summary>
    ///     An interface that handles the ticketing transactions for LP 70/71
    /// </summary>
    public interface ISasVoucherInProvider
    {
        /// <summary>
        ///     Gets the Current state of the state machine
        /// </summary>
        SasVoucherInState CurrentState { get; }

        /// <summary>
        ///     Gets a clone of the current ticket info
        /// </summary>
        TicketInInfo CurrentTicketInfo { get; }


        /// <summary>
        ///     Gets a clone of the current redemption status
        /// </summary>
        RedemptionStatusCode CurrentRedemptionStatus { get; }

        /// <summary>
        ///     Gets a flag indicating if the redemption is enabled
        /// </summary>
        bool RedemptionEnabled { get; }

        /// <summary>
        ///     Handles in inserted voucher transaction by sending exception 67 to the host.
        /// </summary>
        /// <param name="transaction">The transaction that reflects the inserted ticket</param>
        /// <returns>A task containing the validation information of the ticket</returns>
        Task<TicketInInfo> ValidationTicket(VoucherInTransaction transaction);

        /// <summary>
        ///     Saves the valid ticket info recieved from LP 71.
        /// </summary>
        /// <param name="data">The data recieved from LP 71</param>
        /// <param name="statusCode">The status of the accepted ticket</param>
        void AcceptTicket(RedeemTicketData data, RedemptionStatusCode statusCode);

        /// <summary>
        ///     Denies the ticket in escrow.
        /// <param name="statusCode">The status of the rejected ticket</param>
        /// /// <param name="transferCode">The transfer code for ticket reject reason</param>
        /// </summary>
        void DenyTicket(RedemptionStatusCode statusCode, TicketTransferCode? transferCode = null);

        /// <summary>
        ///     Denies the ticket in escrow without setting the status.
        /// </summary>
        void DenyTicket();

        /// <summary>
        ///     Begins the redemption cycle in response to receiving a LP 70.
        /// </summary>
        /// <returns>The data needed for the LP 70 response</returns>
        SendTicketValidationDataResponse RequestValidationData();

        /// <summary>
        ///     Respond to the host that is using LP 71 to get the status of the last ticket.
        /// </summary>
        /// <returns>The response data for the most recent ticket</returns>
        RedeemTicketResponse RequestValidationStatus();

        /// <summary>
        ///     Marks the ticket status as redeemed.
        /// </summary>
        /// <param name="accountType">The account type of the ticket that was stacked</param>
        void OnTicketInCompleted(AccountType accountType);

        /// <summary>
        ///     Update the ticket status and notify the host using exception 68.
        /// </summary>
        /// <param name="barcode">The barcode of the failed ticket</param>
        /// <param name="statusCode">The status code of the failed ticket</param>
        /// <param name="transactionId">The transaction Id of the failed ticket</param>

        void OnTicketInFailed(string barcode, RedemptionStatusCode statusCode, long transactionId);

        /// <summary>
        ///     Acknowledges the transaction as complete when the host Acks
        /// </summary>
        void RedemptionStatusAcknowledged();

        /// <summary>
        /// Sets the redemption status of the ticket in memory.
        /// </summary>
        /// <param name="code">The new redpemption status of the ticket</param>
        void SetRedemptionStatusCode(RedemptionStatusCode code);

        /// <summary>
        /// Denies the current ticket in escrow when the system is disabled.
        /// </summary>
        void OnSystemDisabled();
    }
}
