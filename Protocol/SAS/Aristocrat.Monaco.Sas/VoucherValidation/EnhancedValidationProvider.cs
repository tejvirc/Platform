namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     Provides the enhanced validation data required by the host
    /// </summary>
    public class EnhancedValidationProvider : IEnhancedValidationProvider
    {
        private readonly HostAcknowledgementHandler _processingHandlers;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISasTicketPrintedHandler _sasTicketPrintedHandler;
        private readonly IStorageDataProvider<EnhancedValidationItem> _storageDataProvider;
        private readonly ITransactionHistory _transactionHistory;

        private ConcurrentQueue<EnhancedValidationData> _data;

        /// <summary>
        ///     Creates the <see cref="EnhancedValidationProvider"/> instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="sasTicketPrintedHandler">The ticket printed handler</param>
        /// <param name="storageDataProvider">The persistent data provider</param>
        /// <param name="transactionHistory">the transaction history provider</param>
        public EnhancedValidationProvider(
            IPropertiesManager propertiesManager,
            ISasTicketPrintedHandler sasTicketPrintedHandler,
            IStorageDataProvider<EnhancedValidationItem> storageDataProvider,
            ITransactionHistory transactionHistory)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _sasTicketPrintedHandler = sasTicketPrintedHandler ??
                                       throw new ArgumentNullException(nameof(sasTicketPrintedHandler));
            _storageDataProvider = storageDataProvider ?? throw new ArgumentNullException(nameof(storageDataProvider));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));

            _processingHandlers = new HostAcknowledgementHandler
            {
                ImpliedAckHandler = () =>
                {
                    AcknowledgeTransaction(_sasTicketPrintedHandler.PendingTransactionId);
                    _sasTicketPrintedHandler.TicketPrintedAcknowledged();
                },
                ImpliedNackHandler = () => { _sasTicketPrintedHandler.ClearPendingTicketPrinted(); }
            };

            LoadFromPersistence();
            RecoverTransactions();
        }

        private void RecoverTransactions()
        {
            var transaction = _transactionHistory.RecallTransactions(true).OfType<IAcknowledgeableTransaction>().FirstOrDefault();
            if (transaction is not null && _data.LastOrDefault()?.TransactionId != transaction.TransactionId)
            {
                switch (transaction)
                {
                    case VoucherOutTransaction voucher:
                        HandleTicketOutCompleted(voucher);
                        break;
                    case HandpayTransaction handpay:
                        HandPayReset(handpay);
                        break;
                }
            }

            var acknowledgedTransaction = _transactionHistory.GetNextNeedingHostAcknowledgedTransaction();
            if (acknowledgedTransaction is not null &&
                _data.Any(x => x.Acknowledged && x.TransactionId == acknowledgedTransaction.TransactionId))
            {
                _sasTicketPrintedHandler.TicketPrintedAcknowledged();
            }
        }

        /// <summary>
        ///     Adds data to the queue based on an ITransaction
        /// </summary>
        /// <param name="transaction"></param>
        public void AddTransaction(ITransaction transaction)
        {
            _data.Enqueue(new EnhancedValidationData(transaction));
            while (_data.Count > SasConstants.MaxValidationIndex)
            {
                _data.TryDequeue(out _);
            }

            SaveToPersistence();
        }

        /// <inheritdoc />
        public SendEnhancedValidationInformationResponse GetResponseFromInfo(SendEnhancedValidationInformation data)
        {
            var validationMode = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ValidationType;
            if (validationMode != SasValidationType.SecureEnhanced && validationMode != SasValidationType.System)
            {
                return EnhancedValidationData.FailedResponse();
            }

            return GetValidationBufferReadResults(data);
        }

        private EnhancedValidationData GetNextNeedingHostAcknowledgedTransaction()
        {
            foreach (var data in _data)
            {
                if (!data.Acknowledged)
                {
                    return data;
                }
            }

            return new EnhancedValidationData();
        }

        private SendEnhancedValidationInformationResponse ResponseAtFunctionCode(int functionCode)
        {
            if (functionCode < 1 || functionCode > SasConstants.MaxValidationIndex)
            {
                return new EnhancedValidationData().Response;
            }

            return _data.FirstOrDefault(x => x.IsAtIndex(functionCode))?.Response ?? new EnhancedValidationData().Response;
        }

        private SendEnhancedValidationInformationResponse GetValidationBufferReadResults(SendEnhancedValidationInformation data)
        {
            switch (data.FunctionCode)
            {
                case SasConstants.CurrentValidation:
                    var readResults = GetNextNeedingHostAcknowledgedTransaction();
                    if (readResults.Valid)
                    {
                        _sasTicketPrintedHandler.PendingTransactionId = readResults.TransactionId;
                        readResults.Response.Handlers = _processingHandlers;
                    }

                    return readResults.Response;
                case SasConstants.LookAhead:
                    return GetNextNeedingHostAcknowledgedTransaction().Response;
                default:
                    return ResponseAtFunctionCode(data.FunctionCode);
            }
        }

        /// <inheritdoc />
        public void HandPayReset(HandpayTransaction transaction)
        {
            if (!((transaction?.IsCreditType() ?? false) &&
                  (bool)_propertiesManager.GetProperty(AccountingConstants.ValidateHandpays, false)))
            {
                AddTransaction(transaction);
            }
        }

        /// <inheritdoc />
        public void HandleTicketOutCompleted(VoucherOutTransaction transaction)
        {
            AddTransaction(transaction);
        }

        private void AcknowledgeTransaction(long transactionId)
        {
            var dataToAcknowledge = _data.FirstOrDefault(t => t.TransactionId == transactionId);
            if (dataToAcknowledge == null || dataToAcknowledge.Acknowledged)
            {
                return;
            }

            dataToAcknowledge.Acknowledge();
            SaveToPersistence();
        }

        private void SaveToPersistence()
        {
            var history = _storageDataProvider.GetData();
            history.EnhancedValidationDataLog = StorageHelpers.Serialize(_data.ToList());

            _storageDataProvider.Save(history).FireAndForget();
        }

        private void LoadFromPersistence()
        {
            _data = new ConcurrentQueue<EnhancedValidationData>(
                StorageHelpers.Deserialize(
                    _storageDataProvider.GetData().EnhancedValidationDataLog,
                    () => new List<EnhancedValidationData>(
                        Enumerable.Range(0, SasConstants.MaxValidationIndex - 1)
                            .Select(_ => new EnhancedValidationData()))));

            foreach (var data in _data)
            {
                if (data.NeedsProcessingHandlers)
                {
                    data.Response.Handlers = _processingHandlers;
                }
            }
        }
    }
}
