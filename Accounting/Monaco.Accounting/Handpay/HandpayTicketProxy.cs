namespace Aristocrat.Monaco.Accounting.Handpay
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using Contracts;
    using Contracts.Handpay;
    using Hardware.Contracts.Ticket;
    using log4net;

    /// <summary>
    ///     This class provides methods to fill in ticket information
    /// </summary>
    public class HandpayTicketProxy : ITicketProxy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public ICollection<Type> TransactionTypes => new[] { typeof(HandpayTransaction) };

        /// <inheritdoc />
        public Ticket CreateTicket(ITransaction transaction)
        {
            var handpayTransaction = transaction as HandpayTransaction;

            if (handpayTransaction == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "A {0} transaction can not be converted to a handpay transaction",
                    transaction.GetType().Name);
                Logger.Fatal(message);
                throw new ArgumentException(message);
            }

            switch (handpayTransaction.HandpayType)
            {
                case HandpayType.GameWin:
                    return HandpayTicketsCreator.CreateGameWinReprintTicket(handpayTransaction);
                default:
                    return HandpayTicketsCreator.CreateCanceledCreditsReprintTicket(handpayTransaction);
            }
        }
    }
}