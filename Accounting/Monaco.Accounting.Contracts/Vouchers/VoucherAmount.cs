namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     Defines voucher amount.
    /// </summary>
    public class VoucherAmount
    {
        private readonly long _cashableAmount;
        private readonly long _promoAmount;
        private readonly long _nonCash;

        /// <summary>
        ///     VoucherAmount constructor. 
        /// </summary>
        /// <param name="cashableAmount">cashable Amount</param>
        /// <param name="promoAmount">promo Amount</param>
        /// <param name="nonCash">non Cash</param>
        public VoucherAmount(long cashableAmount, long promoAmount, long nonCash)
        {
            _cashableAmount = cashableAmount;
            _promoAmount = promoAmount;
            _nonCash = nonCash;
        }

        /// <summary>
        ///     Gets total Amount.
        /// </summary>
        public long Amount => _cashableAmount + _promoAmount + _nonCash;

        /// <summary>
        ///     Gets cashable amount.
        /// </summary>
        public long CashAmount => _cashableAmount;

        /// <summary>
        ///     Gets promo amount.
        /// </summary>
        public long PromoAmount => _promoAmount;
        
        /// <summary>
        ///     Gets non cash amount.
        /// </summary>
        public long NonCashAmount => _nonCash;

        /// <summary>
        ///     Deposit amounts.
        /// </summary>
        /// <param name="bank">Bank.</param>
        /// <param name="transactionId">Transaction Id.</param>
        public void Deposit(IBank bank, Guid transactionId)
        {
            if (_cashableAmount > 0)
            {
                bank.Deposit(AccountType.Cashable, _cashableAmount, transactionId);
            }

            if (_promoAmount > 0)
            {
                bank.Deposit(AccountType.Promo, _promoAmount, transactionId);
            }

            if (_nonCash > 0)
            {
                bank.Deposit(AccountType.NonCash, _nonCash, transactionId);
            }
        }

        /// <summary>
        ///     Check balance.
        /// </summary>
        /// <param name="bank">Bank.</param>
        /// <returns></returns>
        public bool CheckBankBalance(IBank bank)
        {
            return (_cashableAmount <= 0 || bank.QueryBalance(AccountType.Cashable) >= _cashableAmount) &&
                   (_promoAmount <= 0 || bank.QueryBalance(AccountType.Promo) >= _promoAmount) &&
                   (_nonCash <= 0 || bank.QueryBalance(AccountType.NonCash) >= _nonCash);
        }

        /// <summary>
        ///     Withdraw amounts.
        /// </summary>
        /// <param name="bank">Bank.</param>
        /// <param name="transactionId">Transaction Id.</param>
        public void Withdraw(IBank bank, Guid transactionId)
        {
            if (_cashableAmount > 0)
            {
                bank.Withdraw(AccountType.Cashable, _cashableAmount, transactionId);
            }

            if (_promoAmount > 0)
            {
                bank.Withdraw(AccountType.Promo, _promoAmount, transactionId);
            }

            if (_nonCash > 0)
            {
                bank.Withdraw(AccountType.NonCash, _nonCash, transactionId);
            }
        }
    }

}