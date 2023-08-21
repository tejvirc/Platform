namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Contracts;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     This class provides methods to fill in ticket information
    /// </summary>
    public class VoucherOutTicketProxy : ITicketProxy
    {
        /// <summary>
        ///     Gets the collection of transactions this ticket proxy supports
        /// </summary>
        public ICollection<Type> TransactionTypes => new[] { typeof(VoucherOutTransaction) };

        /// <summary>
        ///     Create a reprint ticket for the given type of transaction.
        /// </summary>
        /// <param name="transaction">The transaction to reprint</param>
        /// <returns>A filled in reprint ticket for the transaction</returns>
        public Ticket CreateTicket(ITransaction transaction)
        {
            var voucherOutTransaction = transaction as VoucherOutTransaction;
            if (voucherOutTransaction == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "A {0} transaction can not be converted to a Voucher out transaction",
                    transaction.GetType().Name);
                throw new ArgumentException(message);
            }

            return voucherOutTransaction.TypeOfAccount == AccountType.Cashable || voucherOutTransaction.TypeOfAccount == AccountType.Promo
                ? VoucherTicketsCreator.CreateCashOutReprintTicket(voucherOutTransaction)
                : VoucherTicketsCreator.CreateCashOutRestrictedReprintTicket(voucherOutTransaction);
        }
    }
}