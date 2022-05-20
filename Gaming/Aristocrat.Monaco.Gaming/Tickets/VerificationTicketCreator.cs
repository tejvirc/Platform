namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Tickets;
    using Application.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     Definition of the VerificationTicketCreator class
    /// </summary>
    public class VerificationTicketCreator : IVerificationTicketCreator, IService
    {
        /// <inheritdoc />
        public string Name => "Verification Ticket Creator";

        public ICollection<Type> ServiceTypes => new[] { typeof(IVerificationTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public virtual void Initialize()
        {
            // _ticket = new VerificationTicket();
        }

        public Ticket Create(int pageNumber = 0, string titleOverride = null)
        {
            if (pageNumber > 2)
            {
                throw new ArgumentException("Valid page numbers are 0, 1, 2");
            }

            var transactions = ServiceManager.GetInstance().GetService<ITransactionHistory>()
                .RecallTransactions<VoucherOutTransaction>().OrderByDescending(log => log.LogSequence).ToList();

            var sequence = transactions.FirstOrDefault()?.LogSequence ?? 0;

            var ticket = new VerificationTicket(pageNumber, titleOverride, sequence)
            {
                RetailerOverride = titleOverride != null
            };

            return ticket.CreateTextTicket();
        }
    }
}