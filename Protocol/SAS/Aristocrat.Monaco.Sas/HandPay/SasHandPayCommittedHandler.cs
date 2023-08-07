namespace Aristocrat.Monaco.Sas.HandPay
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Progressive;

    /// <summary>
    ///     The Handpay commit handler for SAS
    /// </summary>
    public class SasHandPayCommittedHandler : ISasHandPayCommittedHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const double HandpayPendingTimer = 15000.0;
        private readonly ITransactionHistory _transactionHistory;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBank _bank;

        private readonly ConcurrentDictionary<byte, (IHandpayQueue queue, SasExceptionTimer exceptionTimer)>
            _handpayHandlers = new ConcurrentDictionary<byte, (IHandpayQueue, SasExceptionTimer)>();

        private bool _disposed;

        /// <summary>
        ///     Creates the SasHandPayCommittedHandler Instance
        /// </summary>
        /// <param name="transactionHistory">The transaction history</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="exceptionHandler">The exception handler</param>
        /// <param name="gameHistory">An instance of IGameHistory</param>
        /// <param name="winDetailsProvider">An instance of <see cref="IProgressiveWinDetailsProvider"/></param>
        /// <param name="bank">The bank</param>
        public SasHandPayCommittedHandler(
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager,
            ISasExceptionHandler exceptionHandler,
            IGameHistory gameHistory,
            IProgressiveWinDetailsProvider winDetailsProvider,
            IBank bank)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _progressiveWinDetailsProvider = winDetailsProvider ?? throw new ArgumentNullException(nameof(winDetailsProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            var sasSettings = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            LegacyHandpayReporting = sasSettings.HandpayReportingType == SasHandpayReportingType.LegacyHandpayReporting;
        }

        /// <inheritdoc />
        public async Task HandpayPending(HandpayTransaction transaction)
        {
            if (LegacyHandpayReporting)
            {
                foreach (var handler in _handpayHandlers)
                {
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.HandPayIsPending), handler.Key);
                }

                return;
            }

            foreach (var handler in _handpayHandlers)
            {
                await handler.Value.queue.Enqueue(
                    transaction.ToHandpayDataResponse(
                        _propertiesManager,
                        _bank,
                        _gameHistory,
                        _progressiveWinDetailsProvider));
                handler.Value.exceptionTimer.StartTimer();
            }
        }

        /// <inheritdoc />
        public void HandPayReset(HandpayTransaction transaction)
        {
            foreach (var handler in _handpayHandlers.Where(x => LegacyHandpayReporting || x.Value.queue.Count == 0))
            {
                ReportHandPayReset(transaction, handler.Key);
            }

            transaction.Read = true;
            _transactionHistory.UpdateTransaction(transaction);
        }

        /// <inheritdoc />
        public LongPollHandpayDataResponse GetNextUnreadHandpayTransaction(byte clientNumber)
        {
            if (LegacyHandpayReporting)
            {
                return _transactionHistory.RecallTransactions<HandpayTransaction>()
                    .FirstOrDefault(x => x.State != HandpayState.Committed && x.State != HandpayState.Acknowledged)
                    ?.ToHandpayDataResponse(
                        _propertiesManager,
                        _bank,
                        _gameHistory,
                        _progressiveWinDetailsProvider);
            }

            if (!_handpayHandlers.TryGetValue(clientNumber, out var handQueue))
            {
                return null;
            }

            var dataResponse = handQueue.queue.GetNextHandpayData();
            return dataResponse;
        }

        /// <inheritdoc />
        public void RegisterHandpayQueue(IHandpayQueue handpayQueue, byte clientNumber)
        {
            handpayQueue.OnAcknowledged += HandpayQueue_OnAcknowledged;
            _handpayHandlers.GetOrAdd(
                clientNumber,
                client => (handpayQueue,
                    new SasExceptionTimer(
                        _exceptionHandler,
                        GetException,
                        () => ResetTimer(client),
                        HandpayPendingTimer,
                        client)));
        }

        /// <inheritdoc />
        public void UnRegisterHandpayQueue(IHandpayQueue handpayQueue, byte clientNumber)
        {
            handpayQueue.OnAcknowledged -= HandpayQueue_OnAcknowledged;
            if (_handpayHandlers.TryRemove(clientNumber, out var handler))
            {
                handler.exceptionTimer.Dispose();
            }
        }

        /// <inheritdoc />
        public void Recover()
        {
            var transaction = _transactionHistory.RecallTransactions<HandpayTransaction>().FirstOrDefault(
                x => !x.Read && (x.State == HandpayState.Committed || x.State == HandpayState.Acknowledged));
            if (transaction != null)
            {
                Logger.Debug("Powered down before SAS read the handpay reset need to post the reset exception if applicable");
                HandPayReset(transaction);
            }

            if (LegacyHandpayReporting)
            {
                if (_transactionHistory.RecallTransactions<HandpayTransaction>().All(x => x.State != HandpayState.Requested))
                {
                    return;
                }

                foreach (var handler in _handpayHandlers)
                {
                    _exceptionHandler.ReportException(
                        new GenericExceptionBuilder(GeneralExceptionCode.HandPayIsPending),
                        handler.Key);
                }
            }
            else
            {
                foreach (var handler in _handpayHandlers.Where(x => x.Value.queue.Count > 0))
                {
                    Logger.Debug($"Powered down with items in handpay queue.  Handpay queue count={handler.Value.queue.Count} for clientId={handler.Key}");
                    handler.Value.exceptionTimer.StartTimer();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of resources
        /// </summary>
        /// <param name="disposing">True if disposing the first time</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var handler in _handpayHandlers)
                {
                    handler.Value.queue.OnAcknowledged -= HandpayQueue_OnAcknowledged;
                    handler.Value.exceptionTimer.Dispose();
                }

                _handpayHandlers.Clear();
            }

            _disposed = true;
        }

        private void HandpayQueue_OnAcknowledged(byte clientNumber, long transactionId)
        {
            if (!_handpayHandlers.TryGetValue(clientNumber, out var handler))
            {
                return;
            }

            if (ResetTimer(clientNumber))
            {
                handler.exceptionTimer.StartTimer(true);
            }
            else
            {
                handler.exceptionTimer.StopTimer();
                _exceptionHandler.RemoveException(
                    new GenericExceptionBuilder(GeneralExceptionCode.HandPayIsPending),
                    clientNumber);
            }

            var transaction = _transactionHistory.RecallTransaction<HandpayTransaction>(transactionId);
            if (transaction?.State == HandpayState.Acknowledged || transaction?.State == HandpayState.Committed)
            {
                ReportHandPayReset(transaction, clientNumber);
            }
        }

        private void ReportHandPayReset(HandpayTransaction transaction, byte clientNumber)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.HandPayWasReset), clientNumber);

            if (transaction.HandpayType == HandpayType.GameWin && transaction.IsCreditType())
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.JackpotHandpayKeyedOffToMachinePay), clientNumber);
            }
        }

        private bool LegacyHandpayReporting { get; }

        private static GeneralExceptionCode? GetException() => GeneralExceptionCode.HandPayIsPending;

        private bool ResetTimer(byte clientId)
        {
            return !LegacyHandpayReporting && // Legacy Report does not re-issue exception 51
                   _handpayHandlers.TryGetValue(clientId, out var handler) && handler.queue.Count > 0;
        }
    }
}