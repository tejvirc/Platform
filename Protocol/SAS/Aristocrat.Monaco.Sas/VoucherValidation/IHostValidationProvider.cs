namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System.Threading.Tasks;
    using Contracts.Client;

    /// <summary>
    ///     Handler for host validation
    /// </summary>
    public interface IHostValidationProvider
    {
        /// <summary>
        ///     Gets the current validation state
        /// </summary>
        ValidationState CurrentState { get; }

        /// <summary>
        ///     Gets the validation results from the host
        /// </summary>
        /// <param name="amount">The amount that needs validation</param>
        /// <param name="ticketType">The ticket type for this validation</param>
        /// <returns>A <see cref="Task"/> that for getting the validation results</returns>
        Task<HostValidationResults> GetValidationResults(ulong amount, TicketType ticketType);

        /// <summary>
        ///     Sets the validation information received from the host
        /// </summary>
        /// <param name="validationResults">The validation results received from the host</param>
        /// <returns>Whether or not the validation information was set</returns>
        bool SetHostValidationResult(HostValidationResults validationResults);

        /// <summary>
        ///     Gets the current validation data being requested
        /// </summary>
        /// <returns>The current validation data</returns>
        HostValidationData GetPendingValidationData();

        /// <summary>
        ///     Handles when the system gets disabled
        /// </summary>
        void OnSystemDisabled();
    }
}