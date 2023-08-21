namespace Aristocrat.Monaco.Gaming.Payment
{
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Contracts.Bonus;
    using Contracts.Payment;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     This the default handler for determining the bonus pay method i.e how the amount is paid
    /// </summary>
    public class BonusPaymentDeterminationHandler : IBonusPaymentDeterminationHandler
    {
        private readonly IPropertiesManager _properties;
        private readonly IBank _bank;

        /// <summary>
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="bank"></param>
        public BonusPaymentDeterminationHandler(IPropertiesManager properties, IBank bank)
        {
            _properties = properties;
            _bank = bank;
        }

        /// <inheritdoc />
        public PayMethod GetBonusPayMethod(BonusTransaction transaction, long bonusAmountInMillicents)
        {
            var largeWinLimit = _properties.GetValue(
                AccountingConstants.LargeWinLimit,
                AccountingConstants.DefaultLargeWinLimit);
            var maxCreditLimit = _properties.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            if (bonusAmountInMillicents >= largeWinLimit)
            {
                if (transaction.PayMethod != PayMethod.Any && transaction.PayMethod != PayMethod.Handpay)
                {
                    transaction.Message = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoBonusPayExceededLargeWinLimit), transaction.PayMethod);
                    throw new TransactionException(
                        $"Pay method not supported. Handpay required. Requested: {transaction.PayMethod}");
                }

                transaction.PayMethod = PayMethod.Handpay;
            }
            else if (transaction.PayMethod != PayMethod.Handpay && bonusAmountInMillicents + _bank.QueryBalance() > maxCreditLimit)
            {
                if (transaction.PayMethod != PayMethod.Any && transaction.PayMethod != PayMethod.Voucher)
                {
                    transaction.Message = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoBonusPayExceededMaxCreditLimit), transaction.PayMethod);
                    throw new TransactionException(
                        $"Pay method not supported. Voucher required. Requested: {transaction.PayMethod}");
                }

                transaction.PayMethod = PayMethod.Voucher;
            }

            return transaction.PayMethod;
        }
    }
}