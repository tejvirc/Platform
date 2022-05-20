namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.Wat;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    public class TransactionReferenceProvider : ITransactionReferenceProvider
    {
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _transactionHistory;

        public TransactionReferenceProvider(ITransactionHistory transactionHistory, IGameHistory gameHistory)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        public IEnumerable<T> GetReferences<T>(ITransactionConnector connector)
            where T : c_transactionReference, new()
        {
            if (!connector.AssociatedTransactions.Any())
            {
                return Enumerable.Empty<T>();
            }

            var references = new List<T>();

            var transactions = _transactionHistory.RecallTransactions().ToList();

            var gameHistoryLog = _gameHistory.GetGameHistory().ToList();

            foreach (var transactionId in connector.AssociatedTransactions)
            {
                var log = gameHistoryLog.FirstOrDefault(t => t.TransactionId == transactionId);

                if (log != null)
                {
                    var handpay = connector as HandpayTransaction;

                    references.Add(
                        new T
                        {
                            deviceClass = @"G2S_gamePlay",
                            deviceId = log.GameId,
                            transactionId = log.TransactionId,
                            logSequence = log.LogSequence,
                            cashableAmt = handpay?.CashableAmount ?? 0,
                            promoAmt = handpay?.PromoAmount ?? 0,
                            nonCashAmt = handpay?.NonCashAmount ?? 0
                        });
                }
            }

            foreach (var transactionId in connector.AssociatedTransactions)
            {
                var transaction = transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                switch (transaction)
                {
                    case HandpayTransaction trans:
                        references.Add(
                            new T
                            {
                                deviceClass = @"G2S_handpay",
                                deviceId = trans.DeviceId,
                                transactionId = trans.TransactionId,
                                logSequence = trans.LogSequence,
                                cashableAmt = trans.KeyOffCashableAmount,
                                nonCashAmt = trans.KeyOffNonCashAmount,
                                promoAmt = trans.KeyOffPromoAmount
                            });
                        break;
                    case VoucherOutTransaction trans:
                        references.Add(
                            new T
                            {
                                deviceClass = @"G2S_voucher",
                                deviceId = trans.DeviceId,
                                transactionId = trans.TransactionId,
                                logSequence = trans.LogSequence,
                                cashableAmt = trans.TypeOfAccount == AccountType.Cashable ? trans.Amount : 0,
                                nonCashAmt = trans.TypeOfAccount == AccountType.NonCash ? trans.Amount : 0,
                                promoAmt = trans.TypeOfAccount == AccountType.Promo ? trans.Amount : 0
                            });
                        break;
                    case WatTransaction trans:
                        references.Add(
                            new T
                            {
                                deviceClass = @"G2S_wat",
                                deviceId = trans.DeviceId,
                                transactionId = trans.TransactionId,
                                logSequence = trans.LogSequence,
                                cashableAmt = trans.TransferredCashableAmount,
                                nonCashAmt = trans.TransferredNonCashAmount,
                                promoAmt = trans.TransferredPromoAmount
                            });
                        break;
                }
            }

            return references;
        }
    }
}