namespace Aristocrat.Monaco.Sas.Voucher
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Base;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using VoucherValidation;

    /// <summary>
    ///     The ticket printed handler for SAS
    /// </summary>
    public class SasTicketPrintedHandler : ISasTicketPrintedHandler, IDisposable
    {
        private const long NoTransactionId = -1;
        private const double TicketPrintedTimeout = 15000.0; // 15 seconds

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly ITransactionHistory _transactionHistory;
        private readonly ISasDisableProvider _disableProvider;
        private readonly SasExceptionTimer _exceptionTimer;
        private readonly int _queueFullSize;
        private readonly bool _isNoneValidationType;

        private bool _disposed;

        /// <summary>
        ///     Creates the SasTicketPrintedHandler Instance
        /// </summary>
        /// <param name="exceptionHandler">The exception handler</param>
        /// <param name="transactionHistory">The transaction history</param>
        /// <param name="disableProvider">The sas disable provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        public SasTicketPrintedHandler(
            ISasExceptionHandler exceptionHandler,
            ITransactionHistory transactionHistory,
            ISasDisableProvider disableProvider,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _exceptionTimer = new SasExceptionTimer(_exceptionHandler, GetException, ResetTimer, TicketPrintedTimeout);

            // Allow for at least one additional cashout for each cashout type when we are locked up so we don't drop transactions
            _queueFullSize = _transactionHistory.GetHostAcknowledgedQueueSize() - VoucherValidationConstants.AccountTypeToTicketTypeMap.Count;

            _isNoneValidationType =
                (propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager)))
                .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType == SasValidationType.None;

            // if we have pending transactions at power up when using NONE validation, assume we've already queued the
            // 3D/3E exception and the exceptions could have already been sent to the host. Since we won't get call backs
            // to acknowledge the transactions due to the power cycle, we must acknowledge pending transactions here.
            if (_isNoneValidationType)
            {
                AcknowledgePendingNoneValidationTransactions();
            }
        }

        /// <inheritdoc />
        public void TicketPrintedAcknowledged()
        {
            var pendingTransactionId = PendingTransactionId;
            if (pendingTransactionId == NoTransactionId)
            {
                return;
            }

            PendingTransactionId = NoTransactionId;

            // Stop the timer so we can correctly acknowledge the right transaction and don't want SAS to poll us again
            _exceptionTimer.StopTimer();
            // Clear both exceptions as we will resend if needed and it could even change
            _exceptionHandler.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.CashOutTicketPrinted));
            _exceptionHandler.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.HandPayValidated));
            _exceptionHandler.RemoveHandler(GeneralExceptionCode.CashOutTicketPrinted);
            _exceptionHandler.RemoveHandler(GeneralExceptionCode.HandPayValidated);

            Task.Run(() => AcknowledgeTransaction(pendingTransactionId));
        }

        /// <inheritdoc />
        public void ClearPendingTicketPrinted() => PendingTransactionId = NoTransactionId;

        /// <inheritdoc />
        public void ProcessPendingTickets()
        {
            var pendingHostAcknowledgedCount = _transactionHistory.GetPendingHostAcknowledgedCount();
            if (pendingHostAcknowledgedCount == 0)
            {
                return;
            }

            if (_isNoneValidationType)
            {
                ProcessValidationNotRequiredPendingTickets();
                return;
            }

            if (pendingHostAcknowledgedCount >= _queueFullSize)
            {
                // If we have reach our max unacknowledged count we need to lock up and prevent anymore possible cashout situations.
                _disableProvider.Disable(SystemDisablePriority.Immediate, DisableState.ValidationQueueFull).FireAndForget();
            }

            _exceptionTimer.StartTimer();
        }

        /// <inheritdoc />
        public long PendingTransactionId { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes the object
        /// </summary>
        /// <param name="disposing">Whether or not the dispose the resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _exceptionTimer.Dispose();
            }

            _disposed = true;
        }

        private void AcknowledgeTransaction(long pendingTransactionId)
        {
            var acknowledgedTransaction = _transactionHistory.RecallTransactions()
                .FirstOrDefault(x => x.TransactionId == pendingTransactionId);
            if (acknowledgedTransaction != null)
            { 
                switch (acknowledgedTransaction)
                {
                    case VoucherOutTransaction voucherOutTransaction:
                        voucherOutTransaction.HostAcknowledged = true;
                        break;
                    case HandpayTransaction handpayTransaction:
                        handpayTransaction.State = HandpayState.Acknowledged;
                        break;
                }

                _transactionHistory.UpdateTransaction(acknowledgedTransaction);
                if (_disableProvider.IsDisableStateActive(DisableState.ValidationQueueFull) &&
                    _transactionHistory.GetPendingHostAcknowledgedCount() < _queueFullSize)
                {
                    _disableProvider.Enable(DisableState.ValidationQueueFull);
                }
            }

            UpdateExceptionTimer();
        }

        private void UpdateExceptionTimer()
        {
            if (ResetTimer())
            {
                // We need to let SAS know know they just read and we have another
                _exceptionTimer.StartTimer(true);
            }
            else
            {
                // No need to let this run we are done now
                _exceptionTimer.StopTimer();
            }
        }

        private GeneralExceptionCode? GetException()
        {
            var transaction = _transactionHistory.GetNextNeedingHostAcknowledgedTransaction();
            return GetException(transaction);
        }

        private static GeneralExceptionCode? GetException(ITransaction transaction)
        {
            switch (transaction)
            {
                case VoucherOutTransaction _:
                    return GeneralExceptionCode.CashOutTicketPrinted;
                case HandpayTransaction handpayTransaction:
                    return handpayTransaction.Printed
                        ? GeneralExceptionCode.CashOutTicketPrinted
                        : GeneralExceptionCode.HandPayValidated;
                default:
                    return null;
            }
        }

        private bool ResetTimer() => _transactionHistory.AnyPendingHostAcknowledged();

        private void ProcessValidationNotRequiredPendingTickets()
        {
            var (pending, transactionId) = CheckForPendingTransactions();
            if (pending)
            {
                var exceptionType = GetException();
                if (exceptionType.HasValue)
                {
                    // add the handler for the exception
                    _exceptionHandler.AddHandler(exceptionType.Value, () => AcknowledgeTransaction(transactionId));

                    // Report exceptions for having no validation type or if printing cashout tickets.
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(exceptionType.Value));
                }
            }
        }

        private (bool pending, long transactionId) CheckForPendingTransactions()
        {
            var lastValidatedTransaction = _transactionHistory.RecallTransactions()
                .FirstOrDefault(x => x is HandpayTransaction || x is VoucherOutTransaction);

            var isUnacknowledgedVoucherOut = lastValidatedTransaction is VoucherOutTransaction voucherOut && !voucherOut.HostAcknowledged;
            var isCommittedHandpay = lastValidatedTransaction is HandpayTransaction handpay && handpay.State == HandpayState.Committed;

            return (isUnacknowledgedVoucherOut || isCommittedHandpay, lastValidatedTransaction?.TransactionId ?? -1);
        }

        private void AcknowledgePendingNoneValidationTransactions()
        {
            var (pending, transactionId) = CheckForPendingTransactions();
            while (pending)
            {
                AcknowledgeTransaction(transactionId);
                (pending, transactionId) = CheckForPendingTransactions();
            }
        }
    }
}