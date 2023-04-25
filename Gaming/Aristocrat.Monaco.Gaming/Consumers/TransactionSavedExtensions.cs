namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Extension methods for saving transactions.
    /// </summary>
    public static class TransactionSavedExtensions
    {
        private static readonly object TransactionLock = new object();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Handles the transactions that are part of various out transaction events.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="currencyHandler">The currency handler provider.</param>
        /// <param name="gameHistory">The game history provider.</param>
        /// <param name="history">The history provider.</param>
        /// <param name="meterFreeGames">The meter free games parameter.</param>
        public static void HandleOutTransaction(
            this ITransaction transaction,
            long amount,
            ICurrencyInContainer currencyHandler,
            IGameHistory gameHistory,
            ITransactionHistory history,
            bool meterFreeGames)
        {
            // Don't store bonus handpays because they don't affect the game credit balance
            var handpay = transaction as HandpayTransaction;
            if (handpay != null && handpay.HandpayType == HandpayType.BonusPay)
            {
                // The bonus handpays aren't stored, but they're still set to the Pending cash in amount. It needs
                // to be reset after the transaction is complete, or the value will remain in the Pending cash in label.
                currencyHandler.Reset();
                return;
            }

            long cashableAmount = 0L;
            long cashablePromoAmount = 0L;
            long nonCashablePromoAmount = 0L;

            switch (transaction)
            {
                case KeyedOffCreditsTransaction keyedOffCreditsTransaction:
                    cashableAmount = keyedOffCreditsTransaction.TransferredCashableAmount;
                    cashablePromoAmount = keyedOffCreditsTransaction.TransferredPromoAmount;
                    nonCashablePromoAmount = keyedOffCreditsTransaction.TransferredNonCashAmount;
                    break;
                case WatTransaction t:
                    cashableAmount = t.TransferredCashableAmount;
                    cashablePromoAmount = t.TransferredPromoAmount;
                    nonCashablePromoAmount = t.TransferredNonCashAmount;
                    break;
                case VoucherOutTransaction t:
                    if (t.TypeOfAccount == AccountType.NonCash) nonCashablePromoAmount = t.Amount;
                    break;
            }

            lock (TransactionLock)
            {
                if (gameHistory.CurrentLog != null)
                {
                    var transactions = gameHistory.CurrentLog.Transactions.ToList();

                    if (transactions.Any(t => t.TransactionId == transaction.TransactionId))
                    {
                        return;
                    }

                    foreach (var info in currencyHandler.Transactions)
                    {
                        if (!transactions.Exists(a => a.TransactionId == info.TransactionId))
                        {
                            transactions.Add(info);
                        }
                    }

                    Logger.Debug($"Current game log EndCredits={gameHistory.CurrentLog.EndCredits}, AmountIn={currencyHandler.AmountIn}, amount={amount}");

                    // Associate cashout transaction with game log, 
                    if (amount > 0)
                    {
                        Logger.Debug("Associate transaction with game history log");
                        transactions.Add(
                            new TransactionInfo
                            {
                                Amount = amount,
                                Time = transaction.TransactionDateTime,
                                TransactionType = transaction.GetType(),
                                TransactionId = transaction.TransactionId,
                                HandpayType = handpay?.HandpayType,
                                KeyOffType = handpay?.KeyOffType,
                                CashableAmount = cashableAmount,
                                CashablePromoAmount = cashablePromoAmount,
                                NonCashablePromoAmount = nonCashablePromoAmount
                            });

                        var orphaned = transactions.Where(
                            t => history.RecallTransactions().All(c => c.TransactionId != t.TransactionId)).ToList();
                        foreach (var orphan in orphaned)
                        {
                            transactions.Remove(orphan);
                        }

                        gameHistory.AssociateTransactions(transactions, meterFreeGames);
                    }
                    else
                    {
                        Logger.Debug("Reset currency handler");
                        currencyHandler.Reset();
                    }
                }
                else
                {
                    currencyHandler.Reset();
                }
            }
        }
    }
}