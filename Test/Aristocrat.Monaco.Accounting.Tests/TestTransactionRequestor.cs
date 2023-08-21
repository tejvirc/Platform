namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Definition of the TransactionRequestor class.
    /// </summary>
    public class TestTransactionRequestor : ITransactionRequestor
    {
        /// <summary>
        ///     Initializes a new instance of the TestTransactionRequestor class
        /// </summary>
        public TestTransactionRequestor()
        {
            KeepRunning = true;
            RequestorGuid = Guid.NewGuid();
            RequestGuid = Guid.Empty;
        }

        /// <summary>
        ///     Gets or sets the request id for the transaction
        /// </summary>
        public Guid RequestGuid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this Runnable should keep running
        /// </summary>
        public bool KeepRunning { get; set; }

        /// <summary>
        ///     Gets or sets the transaction id for this transaction
        /// </summary>
        public Guid TransactionGuid { get; set; }

        /// <summary>
        ///     Gets the id of this requestor
        /// </summary>
        public Guid RequestorGuid { get; }

        /// <summary>
        ///     Callback from the TranasctionCooridnator that the queued transaction is ready to be retrieved
        /// </summary>
        /// <param name="requestId">The request id for the transaction</param>
        public void NotifyTransactionReady(Guid requestId)
        {
            RequestGuid = requestId;
        }

        /// <summary>
        ///     Retrieves the transaction id from the TransactionCoordinator
        /// </summary>
        /// <param name="useActualRequestId">
        ///     To simulate an invalid request id this test class allows you to speicfy whether it should use the request id it was
        ///     given
        ///     in the callback or whether it should simulate an invalid request id when it asks for the transcation id
        /// </param>
        public void RetrieveTransactionGuid(bool useActualRequestId)
        {
            if (RequestGuid != Guid.Empty)
            {
                IServiceManager serviceManager = ServiceManager.GetInstance();
                ITransactionCoordinator transactionCoordinator = serviceManager.GetService<ITransactionCoordinator>();

                TransactionGuid =
                    transactionCoordinator.RetrieveTransaction(useActualRequestId ? RequestGuid : Guid.NewGuid());
            }
        }

        /// <summary>
        ///     Releases the transaction id from the TranasctionCoordinator
        /// </summary>
        public void ReleaseTransactionGuid()
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();
            ITransactionCoordinator transactionCoordinator = serviceManager.GetService<ITransactionCoordinator>();

            transactionCoordinator.ReleaseTransaction(TransactionGuid);
        }
    }
}