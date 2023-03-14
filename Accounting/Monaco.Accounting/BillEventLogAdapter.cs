namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Bill events/transactions.
    /// </summary>
    public class BillEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter, ILogTicketPrintable

    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(BillEventLogAdapter));

        public string LogType => EventLogType.BillIn.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var dateFormat = propertiesManager.GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateTimeFormat);
            var dateTimeFormat = $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}";

            var timeService = ServiceManager.GetInstance().GetService<ITime>();

            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var billTransactions = transactionHistory.RecallTransactions<BillTransaction>()
                .OrderByDescending(x => x.TransactionId);
            var events = (from transaction in billTransactions
                          let additionalInfo = new[]{
                              (ResourceKeys.DenominationHeader, transaction.Denomination.FormattedCurrencyString()),
                              GetDateAndTimeHeader(ResourceKeys.InsertedTimeHeader, transaction.TransactionDateTime),
                              (ResourceKeys.AcceptedTimeHeader, transaction.State == CurrencyState.Accepted ? timeService.GetFormattedLocationTime(TimeZoneInfo.ConvertTimeFromUtc(transaction.Accepted, timeService.TimeZoneInformation), dateTimeFormat) : "N/A"),
                              (ResourceKeys.AmountCreditedHeader,transaction.State == CurrencyState.Accepted  ? transaction.Amount.MillicentsToDollars().FormattedCurrencyString() : "N/A"),
                              (ResourceKeys.StatusHeader,  CurrencyAccountingExtensions.GetStatusText(transaction.State)),
                              (ResourceKeys.DetailsHeader, CurrencyAccountingExtensions.GetDetailsMessage(transaction.Exception))}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillIn),
                              transaction.Amount.MillicentsToDollars().FormattedCurrencyString(),
                              CurrencyAccountingExtensions.GetStatusText(transaction.State))
                          select new EventDescription(
                              name,
                              "info",
                              LogType,
                              transaction.TransactionId,
                              transaction.TransactionDateTime,
                              additionalInfo)).ToList();
            return events;
        }

        public IEnumerable<Ticket> GenerateLogTickets(IEnumerable<long> transactionIDs)
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var allBillTransactions = transactionHistory.RecallTransactions<BillTransaction>()
                .OrderByDescending(e => e.TransactionId)
                .ToList();

            var printBillTransactions =
                allBillTransactions.Where(e => transactionIDs.Any(id => id == e.TransactionId)).ToList();
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IBillEventLogTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            var printNumberOfPages = (int)Math.Ceiling((double)printBillTransactions.Count / ticketCreator.EventsPerPage);
            var tickets = new List<Ticket>();
            for (var page = 0; page < printNumberOfPages; page++)
            {
                var singlePageLogs =
                    printBillTransactions.Skip(page * ticketCreator.EventsPerPage).Take(ticketCreator.EventsPerPage).ToList();
                var ticket = ticketCreator.Create(singlePageLogs);
                tickets.Add(ticket);
            }

            return tickets;
        }

        public Ticket GetSelectedTicket(EventDescription selectedRow)
        {
            if (selectedRow == null)
            {
                return null;
            }

            return GetTransactionTicket(selectedRow.TransactionId);
        }

        public bool IsReprintSupported()
        {
            return false;
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var billTransactions = transactionHistory.RecallTransactions<BillTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return billTransactions.Any() ? billTransactions.First().LogSequence : -1;
        }

        public Ticket GenerateReprintTicket(long transactionId)
        {
            return GetTransactionTicket(transactionId);
        }

        private Ticket GetTransactionTicket(long transactionId)
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var record = (from r in transactionHistory.RecallTransactions<BillTransaction>()
                          where r.TransactionId == transactionId
                          select r).SingleOrDefault();

            if (record == null || record.TransactionId == 0)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IBillEventLogTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            Logger.Info(
                "Audit Menu/Bill Event Log - Transaction Id: " + record.TransactionId + " Credited Date: " +
                record.Accepted + " Amount: " + record.Amount);

            return ticketCreator.Create(new List<BillTransaction> { record });
        }
    }
}
