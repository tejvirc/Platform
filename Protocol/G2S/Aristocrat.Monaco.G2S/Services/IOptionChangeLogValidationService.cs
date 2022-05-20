namespace Aristocrat.Monaco.G2S.Services
{
    /// <summary>
    ///     Validate OptionChangeLog
    /// </summary>
    public interface IOptionChangeLogValidationService
    {
        /// <summary>
        ///     Validates the specified configuration identifier.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>Return error code or empty string if change log is valid.</returns>
        string Validate(long transactionId);
    }
}