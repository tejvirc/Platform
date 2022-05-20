namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class LongPollSendCashOutTicketInformationResponse : LongPollResponse
    {
        /// <summary>
        ///     Initializes a new instance of the LongPollSendCashOutTicketInformationResponse class.
        /// </summary>
        /// <param name="validationNumber">Standard validation number (calculated by the gaming machine).</param>
        /// <param name="ticketAmount">Ticket amount in units of cents.</param>
        public LongPollSendCashOutTicketInformationResponse(long validationNumber, long ticketAmount)
        {
            TicketAmount = ticketAmount;
            ValidationNumber = validationNumber;
        }

        /// <summary>
        ///     Ticket amount in units of cents.
        /// </summary>
        public long TicketAmount { get; }

        /// <summary>
        ///     Standard validation number (calculated by the gaming machine).
        /// </summary>
        public long ValidationNumber { get; }
    }
}
