namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.Handpay;
    using Contracts.Models;
    using Contracts.Tickets;
    using Contracts.Vouchers;
    using Handpay;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Handpay events/transactions.
    /// </summary>
    public class HandpayEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter, ILogTicketPrintable
    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(HandpayEventLogAdapter));

        public string LogType => EventLogType.Handpay.GetDescription(typeof(EventLogType));

        private readonly double _multiplier;

        public HandpayEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var handpayTransactions = transactionHistory.RecallTransactions<HandpayTransaction>()
                .OrderByDescending(x => x.LogSequence);
            var events = from transaction in handpayTransactions
                         let combinedAmount = transaction.CashableAmount + transaction.PromoAmount + transaction.NonCashAmount
                         let additionalInfo = new[]{
                             (ResourceKeys.TicketNumberHeader, transaction.Printed ? transaction.ReceiptSequence.ToString() : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)),
                             GetDateAndTimeHeader(transaction.TransactionDateTime),
                             (ResourceKeys.TransactionType, transaction.HandpayType.GetDescription(typeof(HandpayType))),
                             (ResourceKeys.AmountHeader, combinedAmount > 0 && _multiplier > 0 ? $"{(combinedAmount / _multiplier).FormattedCurrencyString()}" : $"{combinedAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.ValidationNumber, VoucherExtensions.GetValidationString(transaction.Barcode)),
                             (ResourceKeys.State, transaction.State.GetDescription(typeof(HandpayState))),
                             (ResourceKeys.Printed, Localizer.For(CultureFor.Operator).GetString(transaction.Printed ? ResourceKeys.Yes : ResourceKeys.No)) }
                         let amount = (transaction.CashableAmount > 0 && _multiplier > 0
                                          ? transaction.CashableAmount / _multiplier
                                          : transaction.CashableAmount) +
                                      (transaction.PromoAmount > 0 && _multiplier > 0
                                          ? transaction.PromoAmount / _multiplier
                                          : transaction.PromoAmount) +
                                      (transaction.NonCashAmount > 0 && _multiplier > 0
                                          ? transaction.NonCashAmount / _multiplier
                                          : transaction.NonCashAmount)
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             transaction.Name,
                             amount.FormattedCurrencyString())
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             transaction.TransactionId,
                             transaction.TransactionDateTime,
                             additionalInfo)
                         { LogSequence = transaction.LogSequence };
            return events;
        }

        public IEnumerable<Ticket> GenerateLogTickets(IEnumerable<long> transactionIDs)
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var allBillTransactions = transactionHistory.RecallTransactions<HandpayTransaction>()
                .OrderByDescending(e => e.TransactionId)
                .ToList();

            var printBillTransactions =
                allBillTransactions.Where(e => transactionIDs.Any(id => id == e.TransactionId)).ToList();

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IHandpayTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            var time = ServiceManager.GetInstance().GetService<ITime>();
            var data = printBillTransactions.Select(
                    handpayTransaction => new HandpayData
                    {
                        TransactionId = $"{handpayTransaction.TransactionId}",
                        CashableAmount =
                            handpayTransaction.CashableAmount > 0
                                ? $"{(handpayTransaction.CashableAmount / _multiplier).FormattedCurrencyString()}"
                                : string.Empty,
                        PromoAmount =
                            handpayTransaction.PromoAmount > 0
                                ? $"{(handpayTransaction.PromoAmount / _multiplier).FormattedCurrencyString()}"
                                : string.Empty,
                        NonCashAmount =
                            handpayTransaction.NonCashAmount > 0
                                ? $"{(handpayTransaction.NonCashAmount / _multiplier).FormattedCurrencyString()}"
                                : string.Empty,
                        TimeStamp = time.GetFormattedLocationTime(handpayTransaction.TransactionDateTime),
                        ValidationId = handpayTransaction.Barcode,
                        SequenceNumber = handpayTransaction.LogSequence,
                        HandpayType = handpayTransaction.HandpayType.GetDescription(typeof(HandpayType)),
                        DeviceId = handpayTransaction.DeviceId,
                        Printed = handpayTransaction.Printed,
                        State = handpayTransaction.State.GetDescription(typeof(HandpayState)),
                        Transaction = handpayTransaction
                    }
                ).ToList();
            var tickets = new List<Ticket>();

            var printNumberOfPages = (int)Math.Ceiling((double)data.Count / ticketCreator.EventsPerPage);
            for (var page = 0; page < printNumberOfPages; page++)
            {
                var singlePageLogs = data.Skip(page * ticketCreator.EventsPerPage).Take(ticketCreator.EventsPerPage).ToList();
                var ticket = ticketCreator.Create(1, singlePageLogs);
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

            var selectedId = new Collection<long> { selectedRow.TransactionId };

            return GenerateLogTickets(selectedId).FirstOrDefault();
        }

        public bool IsReprintSupported()
        {
            return ServiceManager.GetInstance().GetService<ITransactionHistory>()
                .IsPrintable<HandpayTransaction>();
        }

        public long GetMaxLogSequence()
        {
            var transactions = ServiceManager.GetInstance().GetService<ITransactionHistory>()
                .RecallTransactions<HandpayTransaction>().OrderByDescending(log => log.LogSequence).ToList();
            return transactions.Any() ? transactions.First().LogSequence : -1;
        }

        public Ticket GenerateReprintTicket(long transactionId)
        {
            var transactions = ServiceManager.GetInstance().GetService<ITransactionHistory>()
                .RecallTransactions<HandpayTransaction>().OrderByDescending(log => log.LogSequence).ToList();

            var transaction = transactions.FirstOrDefault(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return null;
            }

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus?.Publish(new HandpayReceiptReprintEvent(transaction));

            Logger.Info(
                "Printing handpay receipt - Name: " + transaction.Name + "  Transaction Id: " +
                transaction.TransactionId +
                "  Date: " + transaction.TransactionDateTime);

            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                    return HandpayTicketsCreator.CreateGameWinReprintTicket(transaction);
                case HandpayType.CancelCredit:
                    return HandpayTicketsCreator.CreateCanceledCreditsReceiptReprintTicket(transaction);
                case HandpayType.BonusPay:
                    return HandpayTicketsCreator.CreateBonusPayReprintTicket(transaction);
                default:
                    return null;
            }
        }
    }
}
