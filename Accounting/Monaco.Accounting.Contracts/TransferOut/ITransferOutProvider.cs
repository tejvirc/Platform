namespace Aristocrat.Monaco.Accounting.Contracts.TransferOut
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Transactions;

    /// <summary>
    ///     Provides a standard API to transfer funds off of the EGM
    /// </summary>
    public interface ITransferOutProvider
    {
        /// <summary>
        ///     Gets flag for determining if the provider has an active transfer.
        /// </summary>
        bool Active { get; }

        /// <summary>
        ///     Transfer an amount of credits from a specific account in the bank out of the system.
        /// </summary>
        /// <param name="transactionId">The transaction Id for the transfer.</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A reference Id that should be associated with the cash out</param>
        /// <param name="cancellationToken">A cancellation token used to end the transfer</param>
        /// <returns>a <see cref="TransferResult"/></returns>
        Task<TransferResult> Transfer(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Allows the handler to determine if the specified transaction can be recovered
        /// </summary>
        /// <param name="transactionId">The transaction Id for the transfer.</param>
        /// <returns>true if the transaction can be recovered/completed, else false</returns>
        bool CanRecover(Guid transactionId);

        /// <summary>
        ///     Allows the provider to recover the transaction Id. The provider may not have a matching transaction or may not need
        ///     to recover the transaction.
        /// </summary>
        /// <param name="transaction">The recovery transaction for the transfer.</param>
        /// <param name="cancellationToken">A cancellation token used to end the transfer</param>
        /// <returns>true if the transaction was recovered/completed, else false</returns>
        Task<bool> Recover(IRecoveryTransaction transaction, CancellationToken cancellationToken);
    }
}