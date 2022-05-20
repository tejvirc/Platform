namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Interface for Aft information that multiple classes use
    /// </summary>
    public interface IAftTransferProvider
    {
        /// <summary> Gets the Asset Number </summary>
        uint AssetNumber { get; }

        /// <summary> Gets the Registration key </summary>
        byte[] RegistrationKey { get; }

        /// <summary> Gets or sets the Current Transfer </summary>
        AftResponseData CurrentTransfer { get; set; }

        /// <summary> Gets the AFT transfer limit amount in cents </summary>
        ulong TransferLimitAmount { get; }

        /// <summary> Transfer Amount in cents </summary>
        ulong TransferAmount { get; set; }

        /// <summary> Indicates a transfer failure </summary>
        bool TransferFailure { get; set; }

        /// <summary> Gets a value indicating whether a finished transfer has been acknowledged or not</summary>
        bool IsTransferAcknowledgedByHost { get; set; }

        /// <summary> Gets a value indicating whether a transfer has started but not finished</summary>
        bool IsTransferInProgress { get; }

        /// <summary> Gets a value indicating whether the transaction id is valid or not</summary>
        bool TransactionIdValid { get; }

        /// <summary> Gets a value indicating whether the transaction id is unique or not</summary>
        bool TransactionIdUnique { get; }

        /// <summary> Gets a value indicating whether Aft is locked or not</summary>
        bool IsLocked { get; }

        /// <summary> Gets a value indicating whether the registration key is all zeros or not</summary>
        bool IsRegistrationKeyAllZeros { get; }

        /// <summary> Gets a value indicating whether partial transfers are allowed or not</summary>
        bool PartialTransfersAllowed { get; }

        /// <summary> Gets a value indicating whether full transfers are allowed or not</summary>
        bool FullTransferRequested { get; }

        /// <summary> Indicates whether this is a debit transfer </summary>
        bool DebitTransfer { get; }

        /// <summary> Indicates whether this is a transfer off or not </summary>
        bool TransferOff { get; }

        /// <summary> Indicates whether this is a request to transfer funds </summary>
        /// <param name="data">The AftTransferData to check the transfer type on</param>
        bool TransferFundsRequest(AftTransferData data);

        /// <summary> Indicates whether required receipt data is missing </summary>
        bool MissingRequiredReceiptFields { get; }

        /// <summary> Indicates whether the printer is available </summary>
        bool IsPrinterAvailable { get; }

        /// <summary> Indicates the PosId is zero </summary>
        bool PosIdZero { get; }

        /// <summary> Gets the bank balance for all accounts </summary>
        ulong CurrentBankBalanceInCents { get; }

        /// <summary>
        ///     Called when SAS has been initialized
        /// </summary>
        void OnSasInitialized();

        /// <summary> Gets the bank balance for the specified account </summary>
        /// <param name="account">The account to get the balance for</param>
        ulong CurrentBankBalanceInCentsForAccount(AccountType account);

        /// <summary>
        ///     Creates a new Aft Transaction History entry using the current transaction
        /// </summary>
        void CreateNewTransactionHistoryEntry();

        /// <summary>
        ///     check that the transaction id is valid and different than the transaction id of
        ///     the most recently completed transfer with a non-zero total amount
        /// </summary>
        /// <param name="transactionId">the transaction id to check</param>
        void CheckTransactionId(string transactionId);

        /// <summary>
        ///     save the reason the transfer failed
        /// </summary>
        /// <param name="reason">The reason for the failure</param>
        void TransferFails(AftTransferStatusCode reason);

        /// <summary>
        ///     check if a transfer flag bit is set
        /// </summary>
        /// <param name="flag">The flag to check</param>
        /// <returns>True if the bit is set</returns>
        bool IsTransferFlagSet(AftTransferFlags flag);

        /// <summary>
        ///     check that the last entry in the history log has a transactionId that
        ///     is different than the passed in transactionId
        /// </summary>
        /// <param name="transactionId">The transactionId for the current transfer</param>
        /// <returns>true if transactionId is different</returns>
        bool LastTransactionIdDifferentInHistoryLog(string transactionId);

        /// <summary>
        ///     Update the host cashout flags
        /// </summary>
        /// <param name="data">The data to use for the update</param>
        void UpdateHostCashoutFlags(AftTransferData data);

        /// <summary>
        ///     check thru our list of error conditions to see if any hit
        /// </summary>
        /// <param name="errorConditions">The error conditions to check</param>
        void CheckForErrorConditions(Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> errorConditions);

        /// <summary>
        ///     Perform an AFT off operation
        /// </summary>
        Task DoAftOff();

        /// <summary>
        ///     Perform an AFT on operation
        /// </summary>
        Task DoAftOn();

        /// <summary>
        ///     Perform an AFT bonus operation for the current transfer
        /// </summary>
        Task DoBonus();

        /// <summary>
        ///     Perform an AFT to ticket operation
        /// </summary>
        Task DoAftToTicket();

        /// <summary>
        ///     Update the Aft response data when the transaction is marked as complete
        /// </summary>
        /// <param name="data">The AftData for the transaction</param>
        void UpdateFinalAftResponseData(AftData data);

        /// <summary>
        ///     Update the Aft response data when the transaction is marked as complete
        /// </summary>
        /// <param name="data">The AftData for the transaction</param>
        /// <param name="statusCode">The status code to update the current transfer with</param>
        /// <param name="failed">Indicates whether or not the transfer failed</param>
        void UpdateFinalAftResponseData(AftData data, AftTransferStatusCode statusCode, bool failed);

        /// <summary>
        ///     Sets the transfer amounts to zero, the receipt to NoReceiptRequested,
        ///     and the TransferInProgress flag to false.
        /// </summary>
        void AftTransferFailed();
    }
}