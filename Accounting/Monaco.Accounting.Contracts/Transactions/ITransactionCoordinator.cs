namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     This interface is used to coordinate the transaction requests among the different
    ///     platform components.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A transaction is an interaction between a component and the accounting layer.
    ///         All transactions must be executed in a serial order. There must be only one
    ///         transaction allowed to run at any time in order to avoid the racing for commonly
    ///         used resource or service, for an example the metering service. If a transaction
    ///         runs, it must run in its entirety. When failures occur, it’s important to
    ///         avoid partially completed work. Transactions avoid partial completion by saving
    ///         state information and continuing execution upon restoration of the system.
    ///         The life cycle of a transaction starts when permission is obtained from the
    ///         transaction coordinator to begin a transaction, and the coordinator assigns
    ///         that transaction a unique identifier. The life cycle of the transaction can
    ///         be broken by abandoning forcefully. Once a transaction starts, it must be
    ///         terminated to allow subsequent transactions to begin. If a transaction request
    ///         cannot complete the transaction successfully, it must perform a necessary
    ///         rollback and terminate the transaction.
    ///     </para>
    ///     <para>
    ///         There are two types of transaction for coordination: write and read; the former is
    ///         involved in the meter value change and the later is only for reading the meter value.
    ///     </para>
    ///     <para>
    ///         There are two methods defined in the <c>ITransactionCoordinator</c> interface for
    ///         requesting a transaction. You can request a transaction in either synchronous
    ///         mode or in asynchronous mode.
    ///     </para>
    /// </remarks>
    public interface ITransactionCoordinator
    {
        /// <summary>
        ///     Gets a value indicating whether a transaction is active or not.
        /// </summary>
        bool IsTransactionActive { get; }

        /// <summary>
        ///     Allows a requestor to abandon a transaction or requests for a transaction based
        ///     on their id
        /// </summary>
        /// <param name="requestorId">The id of the requestor as provided when the request was made</param>
        /// <remarks>
        ///     <para>
        ///         By calling this method, you clear up the currently processing transaction or all
        ///         queued requests from the coordinator service. As a result, other requests can get
        ///         their chances to request a transaction from the service for processing.
        ///     </para>
        ///     <para>
        ///         One thing worthy to note is that multiple transaction requests with the same
        ///         requestor ID may be saved in the request queue of the coordinator service.
        ///         Once the <c>AbandonTransaction()</c> is called, all requests with the same
        ///         requestor ID will be erased.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     Here is an illustration:
        ///     <code>
        ///     Guid transactionGuid;
        ///     ...
        ///     // Once the “transactionGuid” is returned from the transaction coordinator service, you might start to
        ///     // create and process the transaction.
        ///     ...
        ///     // If the component encountered a problem and does not know if it had a transaction started
        ///     // or queued. Call the following to cancel it.
        ///     Coordinator.AbandonTransaction(_requestorGuid);
        ///     ...
        ///   </code>
        /// </example>
        void AbandonTransactions(Guid requestorId);

        /// <summary>
        ///     Release a transaction based on the transaction id
        /// </summary>
        /// <param name="transactionId">The transaction id of the transaction to be released</param>
        /// <remarks>
        ///     <para>
        ///         A transaction already started to execute must be released when the processing is
        ///         finished. It will block the next transaction requestor from starting its transaction
        ///         without doing so.
        ///     </para>
        ///     <para>
        ///         The release of any transaction is successful only when the transaction ID
        ///         provided matches with the currently processing transaction ID. The
        ///         <c>TransactionCompletedEvent</c> is posted whenever the transaction terminates.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     Illustrated below:
        ///     <code>
        ///      Guid transactionGuid;
        ///      ...
        ///      // Once the “transactionGuid” is returned by the transaction coordinator service, you might start to
        ///      // create and process the transaction.
        ///      ...
        ///      // Last step in transaction processing should be releasing the transaction from the service.
        ///      coordinator.ReleaseTransaction(transactionGuid);
        ///    </code>
        /// </example>
        void ReleaseTransaction(Guid transactionId);

        /// <summary>
        ///     Request a transaction and queue the request if one is not immediately available
        /// </summary>
        /// <param name="requestor">The requestor of the transaction</param>
        /// <param name="transactionType">The type of requested transaction</param>
        /// <returns>If a transaction is immediately available it will be the transaction id otherwise Guid.Empty is returned</returns>
        /// <remarks>
        ///     <para>
        ///         It works similarly with the synchronous approach when there is no transaction
        ///         pending in the service and there is no transaction request already queued. That
        ///         means the component can start a transaction immediately, and the
        ///         <c> TransactionStartedEvent</c> is posted at the same time.
        ///     </para>
        ///     <para>
        ///         The subtle difference of this method is: when the service is busy with any existing
        ///         transaction or request, it will queue the upcoming request for later approval.
        ///         When a queued request is handled, the service will notify the request owner by
        ///         calling the method <c>NotifyTransactionReady</c> of interface
        ///         <c>ITransactionRequestor</c>. The <c>TransactionStartedEvent</c> is posted right
        ///         after that callback method is finished.
        ///     </para>
        ///     <para>
        ///         This method provides a capability of execution concurrency. Your component can
        ///         continue to run while it is waiting for transaction request to be handled.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        ///       public class OneService : ITransactionRequestor
        ///       {
        ///         private static readonly  Guid _requestorId = new Guid(“{...}”);
        ///         private Guid _requestGuid;
        ///         private Guid _transactionGuid;
        ///         public void StartTransaction()
        ///         {
        ///           ITransactionCoordinator coordinator = ... // Get the service.
        ///           _transactionGuid = coordinator.RequestTransaction(this, TransactionType.Write);
        ///           if (_transactionGuid != Guid.Empty)
        ///           {
        ///             // Start to create a transaction and execute it.
        ///           }
        /// 
        ///           // When the RequestTransaction() returns Guid.Empty, that means the request is
        ///           // saved in the coordinator service. It will be notified after the request is handled.
        ///           // Your program can do other things without having to wait for the approval.
        ///         }
        /// 
        ///         public void NotifyTransactionReady(Guid requestId)
        ///         {
        ///           // At this point, you should save the “requestId” and use it
        ///           // to retrieve the transaction GUID for the later reference.
        ///           _requestGuid = requested;
        ///           ....
        ///           ITransactionCoordinator coordinator = ... // Get the service.
        ///           _transactionGuid = coordinator.RetrieveTransaction(_requestGuid);
        ///           // Now you can start to create a transaction and execute it.
        ///         }
        ///       }
        ///     </code>
        /// </example>
        Guid RequestTransaction(ITransactionRequestor requestor, TransactionType transactionType);

        /// <summary>
        ///     Request a transaction and queue the request if one is not immediately available
        /// </summary>
        /// <param name="requestor">The requestor of the transaction</param>
        /// <param name="transactionType">The type of requested transaction</param>
        /// <param name="topOfQueue">true to place this request at the top of the queue. This should normally be false...</param>
        /// <returns>If a transaction is immediately available it will be the transaction id otherwise Guid.Empty is returned</returns>
        /// <remarks>
        ///     <para>
        ///         It works similarly with the synchronous approach when there is no transaction
        ///         pending in the service and there is no transaction request already queued. That
        ///         means the component can start a transaction immediately, and the
        ///         <c> TransactionStartedEvent</c> is posted at the same time.
        ///     </para>
        ///     <para>
        ///         The subtle difference of this method is: when the service is busy with any existing
        ///         transaction or request, it will queue the upcoming request for later approval.
        ///         When a queued request is handled, the service will notify the request owner by
        ///         calling the method <c>NotifyTransactionReady</c> of interface
        ///         <c>ITransactionRequestor</c>. The <c>TransactionStartedEvent</c> is posted right
        ///         after that callback method is finished.
        ///     </para>
        ///     <para>
        ///         This method provides a capability of execution concurrency. Your component can
        ///         continue to run while it is waiting for transaction request to be handled.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        ///       public class OneService : ITransactionRequestor
        ///       {
        ///         private static readonly  Guid _requestorId = new Guid(“{...}”);
        ///         private Guid _requestGuid;
        ///         private Guid _transactionGuid;
        ///         public void StartTransaction()
        ///         {
        ///           ITransactionCoordinator coordinator = ... // Get the service.
        ///           _transactionGuid = coordinator.RequestTransaction(this, TransactionType.Write);
        ///           if (_transactionGuid != Guid.Empty)
        ///           {
        ///             // Start to create a transaction and execute it.
        ///           }
        /// 
        ///           // When the RequestTransaction() returns Guid.Empty, that means the request is
        ///           // saved in the coordinator service. It will be notified after the request is handled.
        ///           // Your program can do other things without having to wait for the approval.
        ///         }
        /// 
        ///         public void NotifyTransactionReady(Guid requestId)
        ///         {
        ///           // At this point, you should save the “requestId” and use it
        ///           // to retrieve the transaction GUID for the later reference.
        ///           _requestGuid = requested;
        ///           ....
        ///           ITransactionCoordinator coordinator = ... // Get the service.
        ///           _transactionGuid = coordinator.RetrieveTransaction(_requestGuid);
        ///           // Now you can start to create a transaction and execute it.
        ///         }
        ///       }
        ///     </code>
        /// </example>
        Guid RequestTransaction(ITransactionRequestor requestor, TransactionType transactionType, bool topOfQueue);

        /// <summary>
        ///     Request a transaction and do not queue the request
        /// </summary>
        /// <param name="requestorId">The id of the requestor</param>
        /// <param name="timeout">
        ///     This is the amount of time in milliseconds to block while waiting to be able to start a
        ///     transaction
        /// </param>
        /// <param name="transactionType">the type of the requested transaction</param>
        /// <returns>
        ///     If a transaction is available before the timeout it will be the transaction id otherwise Guid.Empty is
        ///     returned
        /// </returns>
        /// <remarks>
        ///     This is a blocking call that will wait if another transaction is in progress. It is like queuing for a transaction
        ///     with a
        ///     timeout value where the caller doesn't have to do anything except make the call.
        /// </remarks>
        /// <example>
        ///     <code>
        ///     public class OneService
        ///     {
        ///       private static readonly  Guid RequestorId = new Guid(“{...}”);
        ///       public bool CreateAndProcessTransaction()
        ///       {
        ///         // Get the service through the service manager
        ///         ITransactionCoordinator coordinator = ...;
        ///         Guid transactionGuid = coordinator.RequestTransaction(RequestorId, 50, TransactionType.Write));
        ///         if (transactionGuid == Guid.Empty)
        ///         {
        ///           return false;
        ///         }
        ///         ...
        ///         // Instantiate a transaction with the transactionGuid, and process it.
        ///         ...
        ///         // Last step in transaction processing should be Release or Abandon, depending on the status of
        ///         // transaction processing.
        ///         ...
        ///         return true;
        ///       }
        ///     }
        ///   </code>
        /// </example>
        Guid RequestTransaction(Guid requestorId, int timeout, TransactionType transactionType);

        /// <summary>
        ///     Request a transaction and do not queue the request
        /// </summary>
        /// <param name="requestorId">The id of the requestor</param>
        /// <param name="timeout">
        ///     This is the amount of time in milliseconds to block while waiting to be able to start a
        ///     transaction
        /// </param>
        /// <param name="transactionType">the type of the requested transaction</param>
        /// <param name="topOfQueue">true to place this request at the top of the queue. This should normally be false...</param>
        /// <returns>
        ///     If a transaction is available before the timeout it will be the transaction id otherwise Guid.Empty is
        ///     returned
        /// </returns>
        /// <remarks>
        ///     This is a blocking call that will wait if another transaction is in progress. It is like queuing for a transaction
        ///     with a
        ///     timeout value where the caller doesn't have to do anything except make the call.
        /// </remarks>
        /// <example>
        ///     <code>
        ///     public class OneService
        ///     {
        ///       private static readonly  Guid RequestorId = new Guid(“{...}”);
        ///       public bool CreateAndProcessTransaction()
        ///       {
        ///         // Get the service through the service manager
        ///         ITransactionCoordinator coordinator = ...;
        ///         Guid transactionGuid = coordinator.RequestTransaction(RequestorId, 50, TransactionType.Write));
        ///         if (transactionGuid == Guid.Empty)
        ///         {
        ///           return false;
        ///         }
        ///         ...
        ///         // Instantiate a transaction with the transactionGuid, and process it.
        ///         ...
        ///         // Last step in transaction processing should be Release or Abandon, depending on the status of
        ///         // transaction processing.
        ///         ...
        ///         return true;
        ///       }
        ///     }
        ///   </code>
        /// </example>
        Guid RequestTransaction(Guid requestorId, int timeout, TransactionType transactionType, bool topOfQueue);

        /// <summary>
        ///     Retrieve a transaction based on the request id received from a queued transaction request
        /// </summary>
        /// <param name="requestId">The id of the request.</param>
        /// <returns>The transaction id</returns>
        Guid RetrieveTransaction(Guid requestId);

        /// <summary>
        ///     Verify that the transaction id provided is the currently executing transaction
        /// </summary>
        /// <param name="transactionId">The transaction id in question</param>
        /// <returns>Whether the transaction id passed in is the current transaction</returns>
        bool VerifyCurrentTransaction(Guid transactionId);

        /// <summary>
        ///     Retrieve a transaction based on the request id
        /// </summary>
        /// <param name="requestId">The id of the request.</param>
        /// <returns>The transaction id</returns>
        Guid GetCurrent(Guid requestId);
    }
}