namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using TransferOut;

    /// <summary>
    ///     An interface by which a request to transfer credits out of system can be handled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service implementing this interface should follow a transactional flow of
    ///         transferring, voucher out and cancel credits such that any remaining credits
    ///         from the transfer will be a voucher and finally any remaining will be canceled.
    ///         Transactions can be started with or without a transaction Guid depending on the method.
    ///         The amounts that the ITransferOutHandler service deals with are expected to be in
    ///         milli-units such that 1000 millicents is equivalent to 1 cent.  The AccountType
    ///         refers to the type of credits affected and is described in the usage document
    ///         for the <c>IBank</c>.
    ///     </para>
    ///     <para>
    ///         Transferring out credits using the ITransferOutHandler service is supported by full or
    ///         partial transfers and with or without providing a transaction Guid.  This allows
    ///         a component to control the amount of credits to withdraw and can place the burden
    ///         of the transaction Guid handling to this service.  The ITransferOutHandler service
    ///         will emit a TransferOutStartedEvent when a transfer out has successfully started and
    ///         a TransferOutCompletedEvent when a transfer out has successfully completed.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     Full transfer is illustrated below:
    ///     <code>
    ///      public class OneComponent
    ///      {
    ///        // ...
    ///        private const Guid TransactionGuid = {"..."};
    ///        private void TransferAll()
    ///        {
    ///          ITransactionCoordinator coordinator =  ...;
    ///          Guid transactionId = coordinator.RequestTransaction(TransactionGuid);
    ///          if (Guid.Empty == transactionId)
    ///          {
    ///            // A transaction is currently active
    ///            return;
    ///          }
    ///          ITransferOutHandler handler = ...;
    ///          Guid activeId = handler.TransferOut(transactionId);
    ///          if (Guid.Empty == activeId)
    ///          {
    ///            // A transaction is currently active
    ///            return;
    ///          }
    ///          // ...
    ///          coordinator.ReleaseTransaction(TransactionGuid);
    ///        }
    ///      }
    ///    </code>
    ///     Partial transfer is illustrated below:
    ///     <code>
    ///      public class AnotherComponent
    ///      {
    ///        // ...
    ///        private void TransferPart()
    ///        {
    ///          ITransferOutHandler handler = ...;
    ///          bool success = handler.TransferOut(5000, AccountType.Cashable);
    ///          if (!success)
    ///          {
    ///            // Transaction was not started and may need to be retried later.
    ///            return;
    ///          }
    ///          // ...
    ///        }
    ///      }
    ///    </code>
    /// </example>
    public interface ITransferOutHandler
    {
        /// <summary>
        ///     Gets a value indicating whether a transfer is currently in progress.
        /// </summary>
        bool InProgress { get; }

        /// <summary>
        ///     Gets a value indicating whether a transfer has been queued.
        /// </summary>
        bool Pending { get; }

        /// <summary>
        ///     Method to transfer all remaining credits in the bank out of the system.
        ///     The transaction will be handled internally.
        /// </summary>
        /// <param name="reason">The reason for transfer out.</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut(TransferOutReason reason = TransferOutReason.CashOut);

        /// <summary>
        ///     Method to transfer an amount of credits from a specific account in the
        ///     bank out of the system.  The transaction will be handled internally.
        /// </summary>
        /// <param name="account">Account to transfer credits from.</param>
        /// <param name="amount">Amount of credits to transfer.</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut(AccountType account, long amount, TransferOutReason reason);

        /// <summary>
        ///     Method to transfer an amount of credits from a specific account in the bank
        ///     out of the system.  It is the caller's responsibility to retain the Guid and
        ///     release the transaction with the transaction coordinator, once the transfer
        ///     completes.
        /// </summary>
        /// <typeparam name="TProvider">The transfer out provider type</typeparam>
        /// <param name="account">Account to transfer credits from.</param>
        /// <param name="amount">Amount of credits to transfer.</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut<TProvider>(AccountType account, long amount, TransferOutReason reason)
            where TProvider : ITransferOutProvider;

        /// <summary>
        ///     Method to transfer all remaining credits in the bank out of the system.
        ///     It is the caller's responsibility to retain the Guid and release the
        ///     transaction with the transaction coordinator, once the transfer completes.
        /// </summary>
        /// <param name="transactionId">Guid.Empty or the current transaction guid.</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut(
            Guid transactionId,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId);

        /// <summary>
        ///     Method to transfer an amount of credits from a specific account in the bank
        ///     out of the system.  It is the caller's responsibility to retain the Guid and
        ///     release the transaction with the transaction coordinator, once the transfer
        ///     completes.
        /// </summary>
        /// <param name="transactionId">Guid.Empty or the current transaction guid.</param>
        /// <param name="account">Account to transfer credits from.</param>
        /// <param name="amount">Amount of credits to transfer.</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut(
            Guid transactionId,
            AccountType account,
            long amount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId);

        /// <summary>
        ///     Method to transfer an amount of credits from a specific account in the bank
        ///     out of the system.  It is the caller's responsibility to retain the Guid and
        ///     release the transaction with the transaction coordinator, once the transfer
        ///     completes.
        /// </summary>
        /// <typeparam name="TProvider">The transfer out provider type</typeparam>
        /// <param name="transactionId">Guid.Empty or the current transaction guid.</param>
        /// <param name="account">Account to transfer credits from.</param>
        /// <param name="amount">Amount of credits to transfer.</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        bool TransferOut<TProvider>(
            Guid transactionId,
            AccountType account,
            long amount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId)
            where TProvider : ITransferOutProvider;

        /// <summary>
        ///     Method to transfer an amount of credits out of the system.
        ///     It is the caller's responsibility to retain the Guid and
        ///     release the transaction with the transaction coordinator, once the transfer
        ///     completes.
        /// </summary>
        /// <typeparam name="TProvider">The transfer out provider type</typeparam>
        /// <param name="transactionId">Guid.Empty or the current transaction guid.</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A unique Id that can be used to track the transfer</param>
        /// <returns>
        ///     True if the transfer will take place, false if another transfer or
        ///     transaction is in progress.
        /// </returns>
        /// <remarks>
        ///     This method starts with the specified provider and will move to the next provider if there is a failure
        /// </remarks>
        bool TransferOutWithContinuation<TProvider>(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId)
            where TProvider : ITransferOutProvider;

        /// <summary>
        ///     Initiates recovery if necessary, and returns traceId of the recovered transfer and any
        ///     other transfer that's currently pending
        /// </summary>
        /// <returns>
        ///     A list of Guids for transfers that are being recovered or are still pending
        /// </returns>
        IReadOnlyCollection<Guid> Recover();

        /// <summary>
        ///     Initiates recovery for the provided transaction if necessary, and returns its traceId
        /// </summary>
        /// <param name="transactionId">The transaction to recover</param>
        /// <returns>
        ///     The traceId of the current transfer if it matches the transactionId and is being
        ///     recovered, otherwise Guid.Empty
        /// </returns>
        Guid Recover(Guid transactionId);
    }
}