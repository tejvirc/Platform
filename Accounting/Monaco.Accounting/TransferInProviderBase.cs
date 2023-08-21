namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Reflection;
    using Application.Contracts;
    using Contracts;
    using Kernel;
    using log4net;

    public abstract class TransferInProviderBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBank _bank;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;

        protected abstract void Recover();

        protected TransferInProviderBase(IBank bank, IMeterManager meters, IPropertiesManager properties)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
        }

        protected bool CheckMaxCreditMeter(long amount)
        {
            if (_properties.GetValue(AccountingConstants.AllowCreditsInAboveMaxCredit, false))
            {
                return true;
            }

            var balance = _bank.QueryBalance();
            if (balance + amount <= _bank.Limit)
            {
                return true;
            }

            Logger.Warn($"Balance {balance} + amount {amount} exceeds MaxCreditMeter {_bank.Limit}");

            return false;
        }

        protected bool CheckLaundry(long amount)
        {
            if (!_properties.GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return true;
            }

            var limit = _properties.GetValue(
                AccountingConstants.MaxTenderInLimit,
                AccountingConstants.DefaultMaxTenderInLimit);

            var cashInLaundry = _properties.GetValue(AccountingConstants.CashInLaundry, 0L);
            var voucherInLaundry = _properties.GetValue(AccountingConstants.VoucherInLaundry, 0L);

            var amountToCheck = voucherInLaundry + cashInLaundry + amount;

            // If the current value plus amount is less than or equal the limit, then we accept the document
            var underLimit = amountToCheck <= limit;
            if (!underLimit)
            {
                Logger.Warn($"Check laundry amount {amountToCheck} exceeds MaxTenderInLimit {limit}");
            }

            return underLimit;
        }

        protected void UpdateLaundryLimit(BaseTransaction transaction)
        {
            if (!_properties.GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return;
            }

            switch (transaction)
            {
                case VoucherInTransaction voucher:
                    var voucherInLaundry = _properties.GetValue(AccountingConstants.VoucherInLaundry, 0L);
                    _properties.SetProperty(AccountingConstants.VoucherInLaundry, voucherInLaundry + voucher.Amount);
                    break;
                case BillTransaction note:
                    var cashInLaundry = _properties.GetValue(AccountingConstants.CashInLaundry, 0L);
                    _properties.SetProperty(AccountingConstants.CashInLaundry, cashInLaundry + note.Amount);
                    break;
            }
        }
    }
}