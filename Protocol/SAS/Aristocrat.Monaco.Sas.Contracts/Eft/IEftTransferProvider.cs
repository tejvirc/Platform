namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using Accounting.Contracts;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client.EFT;

    /// <summary>
    /// Definition of IEftTransferProvider
    /// </summary>
    public interface IEftTransferProvider
    {
        /// <summary>
        /// Process an EFT off request from a single account
        /// </summary>
        /// <returns>True if successful</returns>
        bool DoEftOff(string transactionID, AccountType accountType, ulong amount);

        /// <summary>
        /// Process an EFT off request from a list of accounts
        /// </summary>
        /// <returns>True if successful</returns>
        bool DoEftOff(string transactionID, AccountType[] accountTypes, ulong amount);

        /// <summary>
        /// Process an EFT on request
        /// </summary>
        /// <returns>True if successful</returns>
        bool DoEftOn(string transactionID, AccountType accountType, ulong amount);

        /// <summary>
        /// Get the accepted transfer in amount the provider allows, TransferAmountExceeded is true if partial amount is accepted.
        /// </summary>
        (ulong Amount, bool LimitExceeded) GetAcceptedTransferInAmount(ulong amount);

        /// <summary>
        /// Get the accepted transfer out amount allowed by provider for a single account, LimitExceeded is true if partial amount is accepted.
        /// </summary>
        (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType accountType);

        /// <summary>
        /// Get the accepted transfer out amount the provider allows, LimitExceeded is true if partial amount is accepted.
        /// </summary>
        (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType[] accountTypes);

        /// <summary>
        /// Check if the transfer has already been processed by the gaming machine.
        /// If YES, it will respond to the host with a zero transfer amount and status O. 
        /// </summary>
        /// <returns>True if the transfer has already been processed by the gaming machine</returns>
        bool CheckIfProcessed(string transactionNumber, EftTransferType transferType);

        /// <summary>
        /// Return all cashable, non-cashable, and promotional credits transferred to the gaming machine from the host
        /// and all credits transferred to the host from the gaming machine in cumulative meters. 
        /// </summary>
        CumulativeEftMeterData QueryBalanceAmount();

        /// <summary>
        /// Request Current EFT Promotional Credits (NonCash in Monaco platform).
        /// The host can request the total number of promotional credits currently on a gaming machine's credit meter
        /// by issuing a type R long poll with command code 27.
        /// </summary>
        /// <returns>Current promotional credits</returns>
        long GetCurrentPromotionalCredits();

        /// <summary>
        /// Send Available EFT Transfer.
        /// When the host needs to know what transfers the gaming machine can accept,
        /// it issues a type 4 long poll with command code 6A.
        /// The gaming machine indicates whether it can accept transfers from the host or transfers to the host. 
        /// </summary>
        /// <returns></returns>
        (bool SupportEftTransferOn, bool SupportEftTransferOff) GetSupportedTransferTypes();

        /// <summary>
        /// Restart the timer of cashout provider.
        /// </summary>
        void RestartCashoutTimer();
    }
}