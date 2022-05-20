namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using SASProperties;

    /// <summary>
    ///     The handler for the different validation types
    /// </summary>
    public interface IValidationHandler
    {
        /// <summary>
        ///     Gets the validation type used for this handler
        /// </summary>
        SasValidationType ValidationType { get; }

        /// <summary>
        ///     Checks whether or not the validation handler can validate the requested ticketed out
        /// </summary>
        /// <param name="amount">The amount to check</param>
        /// <param name="ticketType">The ticket type to check</param>
        /// <returns>Whether or not the validation handler can validated the ticket out request</returns>
        bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType);

        /// <summary>
        ///     Handles the ticket out validation
        /// </summary>
        /// <param name="amount">The amount that is trying to be ticked out</param>
        /// <param name="ticketType">The ticket type to validate</param>
        /// <returns>The validation request task to be preformed</returns>
        Task<TicketOutInfo> HandleTicketOutValidation(ulong amount, TicketType ticketType);

        /// <summary>
        ///     Handles the ticket out completed
        /// </summary>
        /// <param name="transaction">The transaction that was completed</param>
        Task HandleTicketOutCompleted(VoucherOutTransaction transaction);

        /// <summary>
        ///     Handles the handpay completed
        /// </summary>
        /// <param name="transaction">The transaction that was completed</param>
        Task HandleHandpayCompleted(HandpayTransaction transaction);

        /// <summary>
        ///     Handles the handpay validation
        /// </summary>
        /// <param name="amount">The amount that is trying to be hand paid</param>
        /// <param name="type">The handpay type</param>
        /// <returns>The validation request task to be preformed</returns>
        Task<TicketOutInfo> HandleHandPayValidation(ulong amount, HandPayType type);

        /// <summary>
        ///     Initializes the validation handler
        /// </summary>
        void Initialize();
    }
}