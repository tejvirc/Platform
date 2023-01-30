namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Common;
    using Services.Reporting;

    public static class EgmPaidReportingExtensions
    {
        public static void ReportEgmPaidTransactions(
            this IReportTransactionQueueService transactionQueue,
            IEnumerable<HandpayTransaction> egmPaidLargeWins,
            long totalPaidAmount,
            uint gameTitleId,
            int denominationId,
            long gameSerial,
            int paytableId)
        {
            if (transactionQueue == null)
            {
                throw new ArgumentNullException(nameof(transactionQueue));
            }

            if (egmPaidLargeWins == null)
            {
                throw new ArgumentNullException(nameof(egmPaidLargeWins));
            }

            var handpayAmount = 0L;
            foreach (var handpayTransaction in egmPaidLargeWins)
            {
                var amount = handpayTransaction.TransactionAmount;
                transactionQueue.AddNewTransactionToQueue(
                    TransactionType.LargeWin,
                    amount.MillicentsToCents(),
                    gameTitleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    handpayTransaction.Barcode);
                handpayAmount += amount;
            }

            var nonHandpaid = totalPaidAmount - handpayAmount;
            if (nonHandpaid > 0)
            {
                transactionQueue.AddNewTransactionToQueue(
                    TransactionType.CashWon,
                    nonHandpaid.MillicentsToCents(),
                    gameTitleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty);
            }
        }
    }
}