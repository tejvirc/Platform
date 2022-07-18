namespace Aristocrat.Monaco.Sas.Eft
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Common;
    using log4net;
    using Storage;
    using Storage.Models;
    using Storage.Repository;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <inheritdoc />
    public class EftHistoryLogProvider : IEftHistoryLogProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //See SAS version 5.02 section 8.6 - Gaming machines are required to keep a log of last 5 transfers
        private readonly FixedSizedBuffer<IEftHistoryLogEntry> _buffer;
        private readonly IStorageDataProvider<EftHistoryItem> _historyDataProvider;

        /// <summary>
        /// Instantiates and initializes the EFT history log provider.
        /// </summary>
        /// <param name="historyDataProvider"></param>
        public EftHistoryLogProvider(IStorageDataProvider<EftHistoryItem> historyDataProvider)
        {
            _historyDataProvider = historyDataProvider ?? throw new ArgumentNullException(nameof(historyDataProvider));
            _buffer = new FixedSizedBuffer<IEftHistoryLogEntry>(SasConstants.EftHistoryLogsSize);
            _buffer.Add(
                StorageHelpers.Deserialize(_historyDataProvider.GetData().EftHistoryLog,
                () =>
                {
                    return new EftHistoryLogEntry[0];
                }));
        }

        private void UpdateLogEntriesInDatabase()
        {
            var history = _historyDataProvider.GetData();
            history.EftHistoryLog = StorageHelpers.Serialize(_buffer.Items);
            _historyDataProvider.Save(history);
        }

        /// <inheritdoc />
        public void AddOrUpdateEntry(EftTransferData requestData, EftTransactionResponse responseData)
        {
            var lastItem = GetLastTransaction();
            if (lastItem != null
                && lastItem.TransactionNumber == requestData.TransactionNumber
                && lastItem.Command == requestData.Command
                && lastItem.RequestedTransactionAmount == requestData.TransferAmount)
            {
                if(responseData.Acknowledgement &&
                    (responseData.Status == TransactionStatus.OperationSuccessful ||
                    responseData.Status == TransactionStatus.TransferAmountExceeded))
                {
                    lastItem.ToBeProcessed = true;

                    Logger.Debug(
                        $"EFT request with LP={requestData.Command}, " +
                        $"Transaction No={requestData.TransactionNumber} and Amount={requestData.TransferAmount} " +
                        $"is approved for processing and will be processed after 800ms or on receiving IAck.");
                }
                else
                {
                    lastItem.Acknowledgement = requestData.Acknowledgement;
                    lastItem.ReportedTransactionStatus = responseData.Status;

                    Logger.Debug(
                        $"Updated Ack and Status for EFT request with LP={requestData.Command}, " +
                        $"Transaction No={requestData.TransactionNumber} and Amount={requestData.TransferAmount}");
                }
            }
            else
            {
                var newEntry = new EftHistoryLogEntry()
                {
                    Command = requestData.Command,
                    TransferType = requestData.TransferType,
                    TransactionNumber = requestData.TransactionNumber,
                    RequestedTransactionAmount = requestData.TransferAmount,
                    ReportedTransactionAmount = responseData.TransferAmount,
                    Acknowledgement = requestData.Acknowledgement,
                    ReportedTransactionStatus = responseData.Status,
                    TransactionDateTime = DateTime.Now,
                    ToBeProcessed = false
                };

                _buffer.Add(newEntry);
                Logger.Debug(
                    $"Entry added for EFT request with LP={requestData.Command}, " +
                    $"Transaction No={requestData.TransactionNumber} and Amount={requestData.TransferAmount}");
            }

            UpdateLogEntriesInDatabase();
        }

        /// <inheritdoc />
        public void UpdateLogEntryForRequestCompleted(LongPoll longPollCommand, int transactionNumber, ulong requestedAmount)
        {
            var lastItem = GetLastTransaction();
            if (lastItem != null
                && lastItem.TransactionNumber == transactionNumber
                && lastItem.Command == longPollCommand
                && lastItem.RequestedTransactionAmount == requestedAmount)
            {
                //Ack flag will be set to true after processing the transaction
                lastItem.Acknowledgement = true;
                lastItem.ToBeProcessed = false;

                UpdateLogEntriesInDatabase();
            }
            else
            {
                Logger.Error(
                    $"Attempted to finalize a log entry for EFT long poll without adding request log entry first. " +
                    $"LP={longPollCommand}, Transaction No={transactionNumber}");
                throw new InvalidOperationException(
                    "Finalize a log entry is not allowed if previous request is not logged.");
            }
        }

        /// <inheritdoc />
        public void UpdateLogEntryForNackedLP(LongPoll longPollCommand, int transactionNumber, ulong requestedAmount)
        {
            var lastItem = GetLastTransaction();
            if (lastItem != null
                && lastItem.TransactionNumber == transactionNumber
                && lastItem.Command == longPollCommand
                && lastItem.RequestedTransactionAmount == requestedAmount)
            {
                Logger.Debug("Nacked a transaction for EFT " +
                    $"LP={longPollCommand}, Transaction No={transactionNumber} and Amount={requestedAmount}");

                lastItem.ToBeProcessed = false;
                UpdateLogEntriesInDatabase();
            }
            else
            {
                Logger.Error(
                    $"Attempted to finalize a log entry for EFT long poll without adding request log entry first. " +
                    $"LP={longPollCommand}, Transaction No={transactionNumber}");
                throw new InvalidOperationException(
                    "Finalize a log entry is not allowed if previous request is not logged.");
            }
        }

        /// <inheritdoc />
        public IEftHistoryLogEntry[] GetHistoryLogs()
        {
            var items = _buffer.Items;
            int skipCount = items.Length > SasConstants.EftHistoryLogsSize ? items.Length - SasConstants.EftHistoryLogsSize : 0;
            return items.Skip(skipCount).Reverse().ToArray();
        }

        /// <inheritdoc />
        public IEftHistoryLogEntry GetLastTransaction()
        {
            return _buffer.LastItem();
        }
    }
}
