namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using Accounting.Contracts;

    /// <summary>
    /// Definition of IEftOffTransferProvider
    /// </summary>
    public interface IEftOffTransferProvider
    {
        /// <summary>
        /// Process an EFT off request
        /// </summary>
        /// <returns>True if successful</returns>
        bool EftOffRequest(string requestID, AccountType[] accountTypes, ulong amount);

        /// <summary>
        /// Gets a value indicating whether or not the provider can accept the Off transfers
        /// </summary>
        bool CanTransfer { get; }

        /// <summary>
        /// Gets the transfer out amount the provider allows, LimitExceeded is true if partial amount is accepted.
        /// </summary>
        (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType[] accountTypes);

        /// <summary>
        /// Restart the timer of cashout provider.
        /// </summary>
        void RestartCashoutTimer();
    }
}