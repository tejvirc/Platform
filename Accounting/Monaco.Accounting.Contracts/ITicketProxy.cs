namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the ITicketProxy interface.
    /// </summary>
    public interface ITicketProxy
    {
        /// <summary>
        ///     Gets the collection of transactions this ticket proxy supports
        /// </summary>
        ICollection<Type> TransactionTypes { get; }

        /// <summary>
        ///     Create a reprint ticket for the given type of transaction.
        /// </summary>
        /// <param name="transaction">The transaction to reprint</param>
        /// <returns>A filled in reprint ticket for the transaction</returns>
        Ticket CreateTicket(ITransaction transaction);
    }
}