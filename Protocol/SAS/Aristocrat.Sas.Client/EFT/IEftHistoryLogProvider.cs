using System.Collections.Generic;

namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    /// Adds, updates and fetches EFT transaction logs for the EGM
    /// </summary>
    public interface IEftHistoryLogProvider
    {
        /// <summary>
        /// Add an entry for the response sent for EFT transaction request.
        /// Update the existing entry's Transaction Status, ReportedAmount and TransferStatus.
        /// </summary>
        /// <param name="requestData">EFT long poll request received from host</param>
        /// <param name="responseData">EFT long poll response sent to host</param>
        void AddOrUpdateEntry(EftTransferData requestData, EftTransactionResponse responseData);

        /// <summary>
        /// Set Ack flag to true and update transfer status for the log entry to request completed.
        /// </summary>
        /// <param name="longPollCommand">Long poll for which the log is updated</param>
        /// <param name="transactionNumber">Eft transaction number for which the log is added</param>
        /// <param name="requestedAmount">Eft command's requested transfer amount for which the log is added</param>
        void UpdateLogEntryForRequestCompleted(LongPoll longPollCommand, int transactionNumber, ulong requestedAmount);

        /// <summary>
        /// Set ToBeProcessed flag to false when final Nack received.
        /// </summary>
        /// <param name="longPollCommand">Long poll for which the log is updated</param>
        /// <param name="transactionNumber">Eft transaction number for which the log is added</param>
        /// <param name="requestedAmount">Eft command's requested transfer amount for which the log is added</param>
        void UpdateLogEntryForNackedLP(LongPoll longPollCommand, int transactionNumber, ulong requestedAmount);

        /// <summary>
        /// Returns last EFT trasaction.
        /// </summary>
        /// <returns></returns>
        IEftHistoryLogEntry GetLastTransaction();

        /// <summary>
        /// Returns last five EFT transactions received from host on this machine.
        /// </summary>
        /// <returns>Transaction history logs</returns>
        IEftHistoryLogEntry[] GetHistoryLogs();
    }
}
