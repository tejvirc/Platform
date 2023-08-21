namespace Aristocrat.Monaco.G2S.Services
{
    using Data.Model;

    /// <summary>
    ///     Provides a mechanism to change the configuration for comms
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        ///     Apply communication changes
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        void Apply(long transactionId);

        /// <summary>
        ///     Cancels the communication changes
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        void Cancel(long transactionId);

        /// <summary>
        ///     Authorizes the communications changes for the specified host
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        /// <param name="hostId">The host identified authorizing the changes</param>
        /// <param name="timeout">Indicates whether the authorization timed out</param>
        void Authorize(long transactionId, int hostId, bool timeout = false);

        /// <summary>
        ///     Aborts the communications changes
        /// </summary>
        /// <param name="transactionId">The transaction identifier</param>
        /// <param name="exception">The exception code describing the reason the configuration is being aborted.</param>
        void Abort(long transactionId, ChangeExceptionErrorCode exception);
    }
}