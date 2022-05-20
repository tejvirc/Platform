namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Threading.Tasks;
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     An interface that handles validating a voucher received by a note acceptor
    ///     or one that needs to be printed for the customer.
    /// </summary>
    /// <remarks>
    ///     This interface provides two validation methods, one for validating a voucher in and one for
    ///     validating a voucher out.
    /// </remarks>
    public interface IVoucherValidator: IService
    {
        /// <summary>
        ///     Gets flag for can validate vouchers in.
        /// </summary>
        bool CanValidateVouchersIn { get; }

        /// <summary>
        ///     Gets whether or not cashable amounts should be combined into one cashout voucher
        /// </summary>
        bool CanCombineCashableAmounts { get; }

        /// <summary>
        ///     Checks whether or not the requested voucher validation can be validated
        /// </summary>
        /// <param name="amount">The amount to check if we can validate</param>
        /// <param name="type">The account type to check if we can validate</param>
        /// <returns>Whether or not you are able to validate vouchers out for this transfer</returns>
        bool CanValidateVoucherOut(long amount, AccountType type);

        /// <summary>
        ///     Method to validate a voucher-in request.  The asynchronous response
        ///     is a callback to the provided VoucherInResponse delegate.
        /// </summary>
        /// <param name="transaction">The associated voucher in transaction</param>
        Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction);

        /// <summary>
        ///    Singal the host that the voucher has been stacked.
        /// </summary>
        /// <param name="transaction">The associated voucher in transaction</param>
        Task StackedVoucher(VoucherInTransaction transaction);

        /// <summary>
        ///     Method to validate a voucher-out request
        /// </summary>
        /// <param name="amount">The amount of credits to validate.</param>
        /// <param name="type">The type of credits to validate.</param>
        /// <param name="transactionId">
        ///     The identifier to determine if this transaction is
        ///     new or a in-progress.
        /// </param>
        /// <param name="reason">Indicates the reason for the transfer</param>
        /// <returns></returns>
        Task<VoucherOutTransaction> IssueVoucher(VoucherAmount amount, AccountType type, Guid transactionId, TransferOutReason reason);

        /// <summary>
        ///     Indicates whether a voucher was accepted or not.  This method should NOT
        ///     be called, if the VoucherInResponse was a denial, indicated by a null
        ///     transaction argument.  This method should ALWAYS be called, if the
        ///     VoucherInResponse contained a non-null transaction.
        /// </summary>
        /// <param name="transaction">Voucher in transaction.</param>
        void CommitVoucher(VoucherInTransaction transaction);

        /// <summary>
        ///     Gets a value indicating whether a validating host is online or not
        /// </summary>
        bool HostOnline { get; }

        /// <summary>
        ///     Gets a value indicating whether a reprint voucher is required.
        /// </summary>
        bool ReprintFailedVoucher { get; }
    }
}
