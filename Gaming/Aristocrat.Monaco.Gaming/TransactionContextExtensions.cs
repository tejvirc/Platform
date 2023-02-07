namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.Wat;

    public static class TransactionContextExtensions
    {
        public static (long cashable, long promo, long nonCash) GetTransactionAmounts(this ITransactionContext @this)
        {
            switch (@this)
            {
                case HandpayTransaction handpay:
                    return (handpay.KeyOffCashableAmount, handpay.KeyOffPromoAmount, handpay.KeyOffNonCashAmount);
                case VoucherOutTransaction voucherOut:
                    switch (voucherOut.TypeOfAccount)
                    {
                        case AccountType.Cashable:
                            return (voucherOut.Amount, 0, 0);
                        case AccountType.Promo:
                            return (0, voucherOut.Amount, 0);
                        case AccountType.NonCash:
                            return (0, 0, voucherOut.Amount);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case WatTransaction wat:
                    return (wat.TransferredCashableAmount, wat.TransferredPromoAmount, wat.TransferredNonCashAmount);
                default:
                    throw new ArgumentException();
            }
        }
    }
}