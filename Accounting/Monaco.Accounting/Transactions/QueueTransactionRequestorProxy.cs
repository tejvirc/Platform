namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Threading;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Used to serve as an ITransactionRequestor proxy to any transaction requestor that
    ///     wants to have the TransactionCoordinator block for them. This allows a requestor
    ///     to not worry about implementing the ITransactionRequestor interface or the necessary
    ///     steps to make it happen and instead just get a transaction Guid.
    /// </summary>
    public class QueueTransactionRequestorProxy : ITransactionRequestor
    {
        private readonly object _isReadyLock = new object();
        private readonly AutoResetEvent _readyReset;
        private readonly Guid _transactionGuid = Guid.Empty;

        private bool _isReady;
        private Guid _transactionRequestId = Guid.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueTransactionRequestorProxy" /> class.
        /// </summary>
        /// <param name="transactionRequestorId">The Guid of the requestor</param>
        /// <param name="transactionType">The type of the transaction to queue a request for</param>
        /// <param name="resetEvent">The AutoResetEvent to set when a transaction guid is available</param>
        /// <param name="topOfQueue">true to place the request at the top of the queue. Should normally be false</param>
        public QueueTransactionRequestorProxy(
            Guid transactionRequestorId,
            TransactionType transactionType,
            AutoResetEvent resetEvent,
            bool topOfQueue)
        {
            _readyReset = resetEvent;
            RequestorGuid = transactionRequestorId;

            var coordinator = ServiceManager.GetInstance().GetService<ITransactionCoordinator>();
            var transactionGuid = coordinator.RequestTransaction(this, transactionType, topOfQueue);

            if (transactionGuid != Guid.Empty)
            {
                lock (_isReadyLock)
                {
                    _transactionGuid = transactionGuid;
                    _isReady = true;
                    _readyReset.Set();
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the proxy is ready to get the transaction Guid
        /// </summary>
        public bool IsReady
        {
            get
            {
                lock (_isReadyLock)
                {
                    return _isReady;
                }
            }
        }

        /// <inheritdoc />
        public Guid RequestorGuid { get; }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
            lock (_isReadyLock)
            {
                _isReady = true;
                _transactionRequestId = requestId;
                _readyReset.Set();
            }
        }

        /// <summary>
        ///     Gets the transaction Guid
        /// </summary>
        /// <returns>The transaction Guid</returns>
        public Guid GetTransactionGuid()
        {
            lock (_isReadyLock)
            {
                if (_isReady)
                {
                    if (_transactionGuid == Guid.Empty)
                    {
                        var coordinator = ServiceManager.GetInstance().GetService<ITransactionCoordinator>();

                        return coordinator.RetrieveTransaction(_transactionRequestId);
                    }

                    return _transactionGuid;
                }
            }

            return Guid.Empty;
        }
    }
}