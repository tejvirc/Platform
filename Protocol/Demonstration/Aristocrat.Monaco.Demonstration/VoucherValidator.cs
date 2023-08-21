namespace Aristocrat.Monaco.Demonstration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Kernel;

    public class VoucherValidator : IVoucherValidator
    {
        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherValidator) };

        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool ReprintFailedVoucher => false;

        public bool CanValidateVouchersIn => false;

        public bool CanCombineCashableAmounts => false;

        public bool CanValidateVoucherOut(long amount, AccountType type)
        {
            return true;
        }

        public Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction)
        {
            return Task.FromResult<VoucherAmount>(null);
        }

        public Task StackedVoucher(VoucherInTransaction transaction)
        {
            return Task.CompletedTask;
        }

        public Task<VoucherOutTransaction> IssueVoucher(
            VoucherAmount amount,
            AccountType type,
            Guid transactionId,
            TransferOutReason reason)
        {
            return Task.Run(() => HandleVoucherOut(amount.Amount, type));
        }

        public void CommitVoucher(VoucherInTransaction transaction)
        {
        }

        private VoucherOutTransaction HandleVoucherOut(long amount, AccountType type)
        {
            var transaction = new VoucherOutTransaction(
                1,
                DateTime.UtcNow,
                amount,
                type,
                "0",
                0,
                string.Empty);

            return transaction;
        }

        /// <inheritdoc />
        public bool HostOnline => true;
    }
}