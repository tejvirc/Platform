namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     TransactionCoordinator provides a mechanism to tie multiple parts of the system to a single controller and verify
    ///     the validity of operations based on a unique transaction id
    /// </summary>
    public sealed class TransactionCoordinator : BaseRunnable, ITransactionCoordinator, IService
    {
        private const PersistenceLevel Level = PersistenceLevel.Transient;
        private const string CurrentRequestIdKey = "CurrentRequestId";
        private const string CurrentTransactionIdKey = "CurrentTransactionId";
        private const string CurrentTransactionRequestorIdKey = "CurrentTransactionRequestorId";
        private const string CurrentRequestorIdKey = "CurrentRequestorId";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPersistentStorageAccessor _block;
        private readonly object _lockObject = new object();
        private readonly Stopwatch _transactionCreationStopwatch = new Stopwatch();
        private readonly Stopwatch _transactionStopwatch = new Stopwatch();
        private readonly AutoResetEvent _waitForTransaction = new AutoResetEvent(false);

        private Guid _currentRequestId;
        private Guid _currentRequestorId;
        private Guid _currentTransactionId;
        private Guid _currentTransactionRequestorId;

        private Queue<KeyValuePair<ITransactionRequestor, TransactionType>> _queuedRequestors =
            new Queue<KeyValuePair<ITransactionRequestor, TransactionType>>();

        public TransactionCoordinator()
        {
            // Get the PersistentStorage service from the ServiceManager
            var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            // Create or retrieve the raw block from persistent storage
            var blockName = GetType().ToString();
            if (storageService.BlockExists(blockName))
            {
                Logger.Debug($"Block: {blockName} exists");
                _block = storageService.GetBlock(blockName);
            }
            else
            {
                Logger.Debug($"Block: {blockName} does not exist, creating");
                _block = storageService.CreateBlock(Level, blockName, 1);
            }

            // Restore the persisted values
            _currentTransactionId = (Guid)_block[CurrentTransactionIdKey];
            Logger.Debug($"Restoring currentTransactionId with a value of: {_currentTransactionId}");

            _currentTransactionRequestorId = (Guid)_block[CurrentTransactionRequestorIdKey];
            Logger.Debug($"Restoring currentTransactionRequestorId with a value of: {_currentTransactionRequestorId}");

            _currentRequestId = (Guid)_block[CurrentRequestIdKey];
            Logger.Debug($"Restoring currentRequestId with a value of: {_currentRequestId}");

            _currentRequestorId = (Guid)_block[CurrentRequestorIdKey];
            Logger.Debug($"Restoring currentRequestorId with a value of: {_currentRequestorId}");
        }

        /// <inheritdoc />
        public string Name => "TransactionCoordinator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ITransactionCoordinator) };

        /// <inheritdoc />
        public bool IsTransactionActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentTransactionId != Guid.Empty;
                }
            }
        }

        /// <inheritdoc />
        public void AbandonTransactions(Guid requestorId)
        {
            Logger.Debug($"Requestor {requestorId} abandoning transactions");

            lock (_lockObject)
            {
                // If the requestorId equals the currentTransactionRequestorId then it means that the
                // requestor already retrieved the Guid after it was notified by the TransactionCoordinator
                // that it was ready. If however the requestorId equals the currentRequestorId then the
                // requestor has not retrieved the Guid yet after being notified by the TransactionCoordinator
                // that it was ready.
                if (_currentTransactionRequestorId == requestorId)
                {
                    Logger.Debug($"Clearing transactions for requestor {requestorId} as current transaction requestor");
                    PostEvent(new TransactionCompletedEvent(_currentTransactionId));

                    using (var transaction = _block.StartTransaction())
                    {
                        ClearAndPersistGuid(
                            transaction,
                            CurrentTransactionRequestorIdKey,
                            ref _currentTransactionRequestorId);
                        ClearAndPersistGuid(transaction, CurrentTransactionIdKey, ref _currentTransactionId);

                        transaction.Commit();
                    }

                    _queuedRequestors = CreateNewQueueWithoutRequestor(requestorId, _queuedRequestors);

                    // let any queued transactions run
                    _waitForTransaction.Set();
                }
                else if (_currentRequestorId == requestorId)
                {
                    Logger.Debug(
                        $"Clearing transactions for requestor {requestorId} as current queued transaction requestor");
                    PostEvent(new TransactionCompletedEvent(_currentTransactionId));

                    using (var transaction = _block.StartTransaction())
                    {
                        ClearAndPersistGuid(transaction, CurrentTransactionIdKey, ref _currentTransactionId);
                        ClearAndPersistGuid(transaction, CurrentRequestorIdKey, ref _currentRequestorId);
                        ClearAndPersistGuid(transaction, CurrentRequestIdKey, ref _currentRequestId);

                        transaction.Commit();
                    }

                    _queuedRequestors = CreateNewQueueWithoutRequestor(requestorId, _queuedRequestors);

                    // let any queued transactions run
                    _waitForTransaction.Set();
                }
                else if (RequestorExistsInQueue(requestorId, _queuedRequestors))
                {
                    Logger.Debug($"Clearing transactions for requestor {requestorId} as queued transaction requestor");
                    _queuedRequestors = CreateNewQueueWithoutRequestor(requestorId, _queuedRequestors);

                    // let any queued transactions run
                    _waitForTransaction.Set();
                }
                else
                {
                    Logger.DebugFormat("No transactions found for requestor {0}", requestorId);
                }
            }
        }

        /// <inheritdoc />
        public void ReleaseTransaction(Guid transactionId)
        {
            Logger.Debug($"Release requested for transaction id {transactionId}");

            lock (_lockObject)
            {
                if (_currentTransactionId == transactionId && transactionId != Guid.Empty)
                {
                    using (var transaction = _block.StartTransaction())
                    {
                        ClearAndPersistGuid(
                            transaction,
                            CurrentTransactionRequestorIdKey,
                            ref _currentTransactionRequestorId);
                        ClearAndPersistGuid(transaction, CurrentTransactionIdKey, ref _currentTransactionId);

                        transaction.Commit();
                    }

                    PostEvent(new TransactionCompletedEvent(transactionId));

                    Logger.Debug(
                        $"Transaction id {transactionId} released, transaction duration was {_transactionStopwatch.ElapsedMilliseconds} ms");

                    // let any queued transactions run
                    _waitForTransaction.Set();
                }
                else
                {
                    Logger.Debug($"Transaction id {transactionId} not found and can not be released.");
                }
            }
        }

        /// <inheritdoc />
        public Guid RequestTransaction(Guid requestorId, int timeout, TransactionType transactionType)
        {
            return RequestTransaction(requestorId, timeout, transactionType, false);
        }

        /// <inheritdoc />
        public Guid RequestTransaction(Guid requestorId, int timeout, TransactionType transactionType, bool topOfQueue)
        {
            if (timeout == 0)
            {
                return DoTransactionRequest(requestorId, transactionType);
            }

            using (var readyReset = new AutoResetEvent(false))
            {
                var proxy = new QueueTransactionRequestorProxy(requestorId, transactionType, readyReset, topOfQueue);

                readyReset.WaitOne(timeout);

                // Either the timeout has occured or there is a valid transaction
                // available.  Check to see if there is a valid transaction
                // id available.  If not then abandon the request.
                var transactionGuid = proxy.GetTransactionGuid();
                if (transactionGuid == Guid.Empty)
                {
                    AbandonTransactions(requestorId);
                }

                return transactionGuid;
            }
        }

        /// <inheritdoc />
        public Guid RequestTransaction(ITransactionRequestor requestor, TransactionType transactionType)
        {
            return RequestTransaction(requestor, transactionType, false);
        }

        /// <inheritdoc />
        public Guid RequestTransaction(
            ITransactionRequestor requestor,
            TransactionType transactionType,
            bool topOfQueue)
        {
            var requestedTransactionGuid = DoTransactionRequest(requestor.RequestorGuid, transactionType);

            if (requestedTransactionGuid != Guid.Empty)
            {
                return requestedTransactionGuid;
            }

            // Queue the request
            lock (_lockObject)
            {
                Logger.Debug(
                    $"Transaction id {_currentTransactionId} is currently in progress for requestor {_currentTransactionRequestorId}, queueing request");

                if (!topOfQueue)
                {
                    _queuedRequestors.Enqueue(
                        new KeyValuePair<ITransactionRequestor, TransactionType>(requestor, transactionType));
                }
                else
                {
                    _queuedRequestors = InsertRequestAtTop(
                        new KeyValuePair<ITransactionRequestor, TransactionType>(requestor, transactionType),
                        _queuedRequestors);
                }

                // allow the Run loop to process the transaction
                _waitForTransaction.Set();
            }

            return Guid.Empty;
        }

        /// <inheritdoc />
        public Guid RetrieveTransaction(Guid requestId)
        {
            lock (_lockObject)
            {
                if (_currentTransactionId != Guid.Empty)
                {
                    if (_currentRequestId == requestId)
                    {
                        _currentTransactionRequestorId = _currentRequestorId;

                        using (var transaction = _block.StartTransaction())
                        {
                            PersistGuid(transaction, CurrentTransactionRequestorIdKey, _currentTransactionRequestorId);

                            ClearAndPersistGuid(transaction, CurrentRequestIdKey, ref _currentRequestId);
                            ClearAndPersistGuid(transaction, CurrentRequestorIdKey, ref _currentRequestorId);

                            transaction.Commit();
                        }

                        Logger.Debug(
                            $"Transaction id {_currentTransactionId} for requestor {_currentTransactionRequestorId} retrieved");
                        return _currentTransactionId;
                    }

                    throw CreateTransactionException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The request id supplied: {0} does not match the current request id: {1}",
                            requestId,
                            _currentRequestId));
                }

                throw CreateTransactionException(
                    "The transaction id is currently not set. Ensure you did not call RetrieveTransaction from NotifyTransactionReady.");
            }
        }

        /// <inheritdoc />
        public bool VerifyCurrentTransaction(Guid transactionId)
        {
            lock (_lockObject)
            {
                return _currentTransactionId != Guid.Empty && transactionId == _currentTransactionId;
            }
        }

        /// <inheritdoc />
        public Guid GetCurrent(Guid requestId)
        {
            lock (_lockObject)
            {
                return _currentTransactionRequestorId == requestId ? _currentTransactionId : Guid.Empty;
            }
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _transactionStopwatch.Start();
            _transactionCreationStopwatch.Start();
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            while (RunState == RunnableState.Running)
            {
                Logger.Debug("About to WaitOne until a transaction is completed");

                // block the thread until we have a transaction to deal with
                _waitForTransaction.WaitOne();

                Logger.Debug("Processing transactions now, checking for queued requests");

                lock (_lockObject)
                {
                    if (_queuedRequestors.Count > 0 && _currentTransactionId == Guid.Empty &&
                        _currentRequestId == Guid.Empty)
                    {
                        _transactionCreationStopwatch.Reset();
                        _transactionCreationStopwatch.Start();

                        Logger.Debug("Queued requestors being processed");
                        var requestorAndType = _queuedRequestors.Dequeue();
                        var requestor = requestorAndType.Key;

                        _currentRequestorId = requestor.RequestorGuid;
                        _currentRequestId = Guid.NewGuid();

                        using (var transaction = _block.StartTransaction())
                        {
                            PersistGuid(transaction, CurrentRequestorIdKey, _currentRequestorId);

                            PersistGuid(transaction, CurrentRequestIdKey, _currentRequestId);

                            transaction.Commit();
                        }

                        Logger.Debug(
                            $"Notifying requestor {_currentRequestorId} of ready transaction with request id {_currentRequestId}");

                        requestor.NotifyTransactionReady(_currentRequestId);

                        // After we've notified the requestor of their transaction we start tracking the amount of time the whole transaction takes
                        _transactionStopwatch.Reset();
                        _transactionStopwatch.Start();

                        // Only after the call has returned do we assign the transaction id
                        _currentTransactionId = Guid.NewGuid();

                        using (var transaction = _block.StartTransaction())
                        {
                            PersistGuid(transaction, CurrentTransactionIdKey, _currentTransactionId);

                            transaction.Commit();
                        }

                        PostEvent(new TransactionStartedEvent(requestorAndType.Value));

                        Logger.DebugFormat(
                            CultureInfo.InvariantCulture,
                            "Transaction id {0} generated in {3} ms for requestor {1} with request id {2} to retrieve",
                            _currentTransactionId,
                            _currentRequestorId,
                            _currentRequestId,
                            _transactionCreationStopwatch.ElapsedMilliseconds);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Logger.Debug("Stopped called.");

            // let the Run loop finish
            _waitForTransaction.Set();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                _waitForTransaction.Close();
            }
        }

        private static void PostEvent<T>(T theEvent)
            where T : IEvent
        {
            ServiceManager.GetInstance().GetService<IEventBus>().Publish(theEvent);
        }

        private static TransactionException CreateTransactionException(string message)
        {
            Logger.Fatal(message);
            return new TransactionException(message);
        }

        private static Queue<KeyValuePair<ITransactionRequestor, TransactionType>> CreateNewQueueWithoutRequestor(
            Guid requestorId,
            Queue<KeyValuePair<ITransactionRequestor, TransactionType>> queueOfRequestors)
        {
            var temp = new Queue<KeyValuePair<ITransactionRequestor, TransactionType>>();

            while (queueOfRequestors.Count > 0)
            {
                var requestorAndType = queueOfRequestors.Dequeue();
                var requestor = requestorAndType.Key;
                if (requestor.RequestorGuid != requestorId)
                {
                    temp.Enqueue(requestorAndType);
                }
            }

            return temp;
        }

        private static Queue<KeyValuePair<ITransactionRequestor, TransactionType>> InsertRequestAtTop(
            KeyValuePair<ITransactionRequestor, TransactionType> requestor,
            Queue<KeyValuePair<ITransactionRequestor, TransactionType>> queueOfRequestors)
        {
            var newQueue = new Queue<KeyValuePair<ITransactionRequestor, TransactionType>>();

            newQueue.Enqueue(requestor);
            while (queueOfRequestors.Count > 0)
            {
                var existing = queueOfRequestors.Dequeue();
                newQueue.Enqueue(existing);
            }

            return newQueue;
        }

        private static bool RequestorExistsInQueue(
            Guid requestorId,
            IEnumerable<KeyValuePair<ITransactionRequestor, TransactionType>> queueOfRequestors)
        {
            return queueOfRequestors.Any(r => r.Key.RequestorGuid == requestorId);
        }

        private static void ClearAndPersistGuid(
            IPersistentStorageTransaction transaction,
            string guidKey,
            ref Guid guidToPersist)
        {
            guidToPersist = Guid.Empty;
            PersistGuid(transaction, guidKey, guidToPersist);
        }

        private static void PersistGuid(IPersistentStorageTransaction transaction, string guidKey, Guid guidToPersist)
        {
            transaction[guidKey] = guidToPersist;
        }

        private Guid DoTransactionRequest(Guid requestorId, TransactionType transactionType)
        {
            _transactionCreationStopwatch.Reset();
            _transactionCreationStopwatch.Start();

            lock (_lockObject)
            {
                if (requestorId == Guid.Empty)
                {
                    CreateTransactionException("Guid requestor id is empty.");
                }
                else if (_currentTransactionId != Guid.Empty && (_currentRequestId == requestorId || _currentTransactionRequestorId == requestorId))
                {
                    Logger.Debug($"Transaction id {_currentTransactionId} is currently in progress");
                    return _currentTransactionId;
                }
                else if (_currentTransactionId != Guid.Empty)
                {
                    Logger.Debug(
                        $"Transaction id {_currentTransactionId} is currently in progress for requestor {_currentTransactionRequestorId}");
                }
                else if (_queuedRequestors.Count > 0)
                {
                    Logger.Debug($"There are {_queuedRequestors.Count} requestors in queue.");
                }
                else
                {
                    _currentTransactionId = Guid.NewGuid();
                    _currentTransactionRequestorId = requestorId;

                    using (var transaction = _block.StartTransaction())
                    {
                        PersistGuid(transaction, CurrentTransactionIdKey, _currentTransactionId);

                        PersistGuid(transaction, CurrentTransactionRequestorIdKey, _currentTransactionRequestorId);

                        transaction.Commit();
                    }

                    PostEvent(new TransactionStartedEvent(transactionType));

                    Logger.Debug(
                        $"Transaction id {_currentTransactionId} created for requestor {requestorId} in {_transactionCreationStopwatch.ElapsedMilliseconds} ms");

                    // When we return the GUID we start tracking the time the whole transaction takes
                    _transactionStopwatch.Reset();
                    _transactionStopwatch.Start();

                    return _currentTransactionId;
                }
            }

            return Guid.Empty;
        }
    }
}
