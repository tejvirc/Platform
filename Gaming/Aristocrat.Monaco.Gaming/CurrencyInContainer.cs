namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.Wat;
    using Bonus;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;

    public class CurrencyInContainer : ICurrencyInContainer
    {
        private readonly ITransactionHistory _history;
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string AmountInKey = @"AmountIn";
        private const string TransactionsBlobField = @"TransactionsBlob";

        private readonly IPersistentStorageAccessor _accessor;

        private long? _amountIn;
        private byte[] _transactionInfoData;
        private readonly IList<TransactionInfo> _transactionInfo;

        public CurrencyInContainer(IPersistentStorageManager storageManager, ITransactionHistory history)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            _history = history ?? throw new ArgumentNullException(nameof(history));

            var blockName = GetType().ToString();
            _accessor = storageManager.GetAccessor(Level, blockName);
            _transactionInfoData = (byte[])_accessor[TransactionsBlobField];
            _transactionInfo = StorageUtilities.GetListFromByteArray<TransactionInfo>(_transactionInfoData).ToList();
        }

        public long AmountIn
        {
            get => _amountIn ??= (long)_accessor[AmountInKey];
            private set
            {
                using var transaction = _accessor.StartTransaction();
                transaction[AmountInKey] = _amountIn = value;
                transaction.Commit();
            }
        }

        public IEnumerable<TransactionInfo> Transactions
        {
            get => new List<TransactionInfo>(_transactionInfo);
            private set
            {
                using var transaction = _accessor.StartTransaction();
                transaction[TransactionsBlobField] = _transactionInfoData = StorageUtilities.ToByteArray(value);
                transaction.Commit();
            }
        }

        public void Credit(ITransaction value, long paidAmount, long transactionId)
        {
            var currencyOut = false;
            var excludeFromPendingAmount = false;

            var transactionInfo = new TransactionInfo
            {
                Amount = paidAmount,
                Time = value.TransactionDateTime,
                TransactionType = value.GetType(),
                TransactionId = transactionId
            };

            switch (value)
            {
                case BillTransaction:
                case VoucherInTransaction:
                    break;
                case WatOnTransaction trans:
                    transactionInfo.CashableAmount = trans.TransferredCashableAmount;
                    transactionInfo.CashablePromoAmount = trans.TransferredPromoAmount;
                    transactionInfo.NonCashablePromoAmount = trans.TransferredNonCashAmount;
                    break;
                case BonusTransaction trans:
                    if (trans.Mode != BonusMode.GameWin && trans.PayMethod == PayMethod.Handpay && trans.IsAttendantPaid(_history))
                    {
                        transactionInfo.HandpayType = HandpayType.BonusPay;
                    }
                    excludeFromPendingAmount = true;
                    break;
                case VoucherOutTransaction:
                case HandpayTransaction:
                    break;
                case WatTransaction trans:
                    transactionInfo.CashableAmount = trans.TransferredCashableAmount;
                    transactionInfo.CashablePromoAmount = trans.TransferredPromoAmount;
                    transactionInfo.NonCashablePromoAmount = trans.TransferredNonCashAmount;
                    currencyOut = true;
                    break;
                default:
                    return;
            }

            _transactionInfo.Add(transactionInfo);
            Transactions = _transactionInfo;

            if (!excludeFromPendingAmount)
            {
                AmountIn += currencyOut ? -paidAmount : paidAmount;
            }
        }

        public void Credit(ITransaction value)
        {
            var currencyOut = false;
            long amount;
            var transactionInfo = new TransactionInfo
            {
                Time = value.TransactionDateTime,
                TransactionType = value.GetType(),
                TransactionId = value.TransactionId
            };

            var excludeFromPendingAmount = false;

            switch (value)
            {
                case BillTransaction trans:
                    amount = trans.Amount;
                    break;
                case VoucherInTransaction trans:
                    amount = trans.Amount;
                    transactionInfo.CashableAmount = trans.TypeOfAccount == AccountType.Cashable ? amount : 0;
                    transactionInfo.CashablePromoAmount = trans.TypeOfAccount == AccountType.Promo ? amount : 0;
                    transactionInfo.NonCashablePromoAmount = trans.TypeOfAccount == AccountType.NonCash ? amount : 0;
                    break;
                case WatOnTransaction trans:
                    amount = trans.TransactionAmount;
                    transactionInfo.CashableAmount = trans.TransferredCashableAmount;
                    transactionInfo.CashablePromoAmount = trans.TransferredPromoAmount;
                    transactionInfo.NonCashablePromoAmount = trans.TransferredNonCashAmount;
                    break;
                case BonusTransaction trans when trans.Mode != BonusMode.GameWin &&
                                                 trans.Mode != BonusMode.WagerMatch &&
                                                 trans.Mode != BonusMode.MultipleJackpotTime:
                    amount = trans.PaidAmount;
                    if (trans.PayMethod == PayMethod.Handpay && trans.IsAttendantPaid(_history))
                    {
                        transactionInfo.HandpayType = HandpayType.BonusPay;
                    }
                    excludeFromPendingAmount = true;

                    break;
                case VoucherOutTransaction trans:
                    amount = trans.Amount;
                    currencyOut = true;
                    break;
                case HandpayTransaction trans:
                    amount = trans.PromoAmount + trans.NonCashAmount + trans.CashableAmount;
                    currencyOut = true;
                    break;
                case KeyedCreditsTransaction trans:
                    amount = trans.Amount;
                    transactionInfo.CashableAmount = trans.TransferredCashableAmount;
                    transactionInfo.CashablePromoAmount = trans.TransferredPromoAmount;
                    transactionInfo.NonCashablePromoAmount = trans.TransferredNonCashAmount;
                    break;
                case WatTransaction trans:
                    amount = trans.TransactionAmount;
                    transactionInfo.CashableAmount = trans.TransferredCashableAmount;
                    transactionInfo.CashablePromoAmount = trans.TransferredPromoAmount;
                    transactionInfo.NonCashablePromoAmount = trans.TransferredNonCashAmount;
                    currencyOut = true;
                    break;
                default:
                    return;
            }

            transactionInfo.Amount = amount;

            _transactionInfo.Add(transactionInfo);
            Transactions = _transactionInfo;

            if (!excludeFromPendingAmount)
            {
                AmountIn += currencyOut ? -amount : amount;
            }
        }

        public long Reset()
        {
            if (!_transactionInfo.Any())
            {
                return 0;
            }

            var current = AmountIn;

            AmountIn = 0;
            _amountIn = null;
            Transactions = Enumerable.Empty<TransactionInfo>();
            _transactionInfo.Clear();

            return current;
        }
    }
}
