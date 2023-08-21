namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the TestVoucherValidator class.
    /// </summary>
    public class TestVoucherValidator : IVoucherValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool PerformValidations { get; set; }

        public long IssueAmount { get; set; }

        public AccountType IssueAccountType { get; set; }

        public string Name => typeof(IVoucherValidator).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherValidator) };

        public void Initialize()
        {
            PerformValidations = true;
            IssueAccountType = AccountType.Cashable;
            IssueAmount = 1000;
        }

        public bool CanValidateVouchersIn => true;

        public bool CanCombineCashableAmounts => false;

        /// <inheritdoc />
        public bool ReprintFailedVoucher => false;

        /// <inheritdoc />
        public bool HostOnline => true;

        public bool CanValidateVoucherOut(long amount, AccountType type)
        {
            return true;
        }

        public Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction)
        {
            if (!PerformValidations)
            {
                transaction.Amount = 0;
                return Task.FromResult<VoucherAmount>(null);
            }

            return Task.Run(() => RespondToVoucherIn(transaction));
        }

        /// <inheritdoc />
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
            if (PerformValidations)
            {
                Log.Info("Starting voucher-out validation processing...");
                IssueAmount = amount.Amount;
                IssueAccountType = type;
                if(reason == TransferOutReason.CashOut)
                {
                    ResetLaundryLimit();
                }

                return Task.Run(() => RespondToVoucherOut());
            }

            Log.Info("Ignoring voucher-out validation request");

            return Task.FromResult<VoucherOutTransaction>(null);
        }

        public void CommitVoucher(VoucherInTransaction transaction)
        {
            Task.Run(
                () =>
                {
                    transaction.CommitAcknowledged = true;

                    ServiceManager.GetInstance().GetService<ITransactionHistory>().UpdateTransaction(transaction);
                });
        }

        private VoucherAmount RespondToVoucherIn(VoucherInTransaction transaction)
        {
            if (!string.IsNullOrEmpty(transaction.Barcode))
            {
                transaction.Amount = IssueAmount;
                transaction.TypeOfAccount = AccountType.Cashable;
                transaction.VoucherSequence = Guid.NewGuid().GetHashCode();
            }

            return null;
        }
        
        private VoucherOutTransaction RespondToVoucherOut()
        {
            // Lame, but basically random and sufficient for testing
            var barcode = DateTime.UtcNow.ToString("yyyyMMddHHmmssfssf");
            var expirationDays = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.VoucherOutExpirationDays, 0);

            var transaction = new VoucherOutTransaction(
                0,
                DateTime.UtcNow,
                IssueAmount,
                IssueAccountType,
                barcode,
                expirationDays,
                string.Empty) { HostAcknowledged = true};

            return transaction;
        }

        private static void ResetLaundryLimit()
        {
            if (!ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return;
            }

            ServiceManager.GetInstance().GetService<IPropertiesManager>().SetProperty(
                AccountingConstants.CashInLaundry,
                0L);

            ServiceManager.GetInstance().GetService<IPropertiesManager>().SetProperty(
                AccountingConstants.VoucherInLaundry,
                0L);
        }
    }
}