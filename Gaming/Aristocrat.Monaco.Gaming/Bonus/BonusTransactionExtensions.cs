namespace Aristocrat.Monaco.Gaming.Bonus
{
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Contracts.Bonus;

    public static class BonusTransactionExtensions
    {
        public static bool IsAttendantPaid(this BonusTransaction transaction, ITransactionHistory history)
        {
            var lastPaidHandpay = history.RecallTransactions<HandpayTransaction>().OrderByDescending(t => t.TransactionId)
                .FirstOrDefault(t => t.AssociatedTransactions.Contains(transaction.TransactionId));
            return lastPaidHandpay != null && !lastPaidHandpay.IsCreditType();
        }

        public static bool IsAttendantPaidGameWin(this BonusTransaction transaction, ITransactionHistory history)
        {
            var lastPaidHandpay = history.RecallTransactions<HandpayTransaction>()
                .OrderByDescending(t => t.TransactionId)
                .FirstOrDefault(t => t.AssociatedTransactions.Contains(transaction.TransactionId));
            return lastPaidHandpay != null &&
                   lastPaidHandpay.Reason is TransferOutReason.LargeWin or TransferOutReason.BonusPay &&
                   !lastPaidHandpay.IsCreditType();
        }
    }
}