namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using Contracts.Client;

    /// <summary>
    ///     The validation data required by the host
    /// </summary>
    public class HostValidationData
    {
        /// <summary>
        ///     Creates and instance of the HostValidationData
        /// </summary>
        /// <param name="amount">the amount the host needs to validate</param>
        /// <param name="ticketType">the type of ticket the host is to validate</param>
        public HostValidationData(ulong amount, TicketType ticketType)
        {
            Amount = amount;
            TicketType = ticketType;
        }

        /// <summary>
        ///     Gets the amount used for the validation
        /// </summary>
        public ulong Amount { get; }

        /// <summary>
        ///     Gets the ticket type used for validation
        /// </summary>
        public TicketType TicketType { get; }
    }
}