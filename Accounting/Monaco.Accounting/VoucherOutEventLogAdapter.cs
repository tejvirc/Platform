namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Application.UI.Events;
    using Common;
    using Contracts;
    using Contracts.Models;
    using Contracts.Tickets;
    using Contracts.Vouchers;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Voucher Out events/transactions.
    /// </summary>
    public class VoucherOutEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter, ILogTicketPrintable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly double _multiplier;

        public string LogType => EventLogType.VoucherOut.GetDescription(typeof(EventLogType));

        public VoucherOutEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var voucherOutTransactions = transactionHistory.RecallTransactions<VoucherOutTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();

            var events = from transaction in voucherOutTransactions
                         let additionalInfo = new[]{
                            (ResourceKeys.TicketNumberHeader, transaction.VoucherSequence.ToString()),
                            GetDateAndTimeHeader(transaction.TransactionDateTime),
                            (ResourceKeys.AmountHeader,transaction.Amount > 0 && _multiplier > 0 ? $"{(transaction.Amount / _multiplier).FormattedCurrencyString()}" : $"{transaction.Amount.FormattedCurrencyString()}"),
                            (ResourceKeys.TypeOfAccountHeader, GetTypeOfAccount(transaction)),
                            (ResourceKeys.ValidationNumber, VoucherExtensions.GetValidationString(transaction.Barcode)),
                            (ResourceKeys.StatusHeader, transaction.HostAcknowledged ? Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Acknowledged) : Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Pending))}
                         let amount = transaction.Amount > 0 && _multiplier > 0
                             ? transaction.Amount / _multiplier
                             : transaction.Amount
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherOut),
                             $"#{transaction.VoucherSequence}",
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
            var allVoucherOutTransactions = transactionHistory.RecallTransactions<VoucherOutTransaction>()
                .OrderByDescending(log => log.LogSequence)
                .ToList();

            var voucherOutTransactionsForPrinting =
                allVoucherOutTransactions.Where(e => transactionIDs.Any(id => id == e.TransactionId)).ToList();
            var ticketCreator = ServiceManager.GetInstance().TryGetService<ICashSlipEventLogTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            var time = ServiceManager.GetInstance().GetService<ITime>();

            var data = new List<VoucherData>();
            try
            {
                data.AddRange(
                    voucherOutTransactionsForPrinting.Select(
                        voucherOutData => new VoucherData
                        {
                            TransactionId = $"{voucherOutData.TransactionId}",
                            Amount =
                                voucherOutData.Amount > 0
                                    ? $"{(voucherOutData.Amount / _multiplier).FormattedCurrencyString()}"
                                    : string.Empty,
                            TimeStamp = time.GetFormattedLocationTime(voucherOutData.TransactionDateTime),
                            SequenceNumber = voucherOutData.LogSequence,
                            VoucherSequence = voucherOutData.VoucherSequence,
                            TypeOfAccount = GetTypeOfAccount(voucherOutData),
                            ValidationId = VoucherExtensions.GetValidationString(voucherOutData.Barcode),
                            Status = voucherOutData.HostAcknowledged
                                ? Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Acknowledged)
                                : Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.Pending)
                        }));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            var printNumberOfPages = (int)Math.Ceiling((double)data.Count / ticketCreator.EventsPerPage);
            var tickets = new List<Ticket>();
            for (var page = 0; page < printNumberOfPages; page++)
            {
                var singlePageLogs = data.Skip(page * ticketCreator.EventsPerPage).Take(ticketCreator.EventsPerPage).ToList();
                var cashSlips = singlePageLogs.Select(
                    x => new CashSlipEventLogRecord
                    {
                        SequenceNumber = x.SequenceNumber,
                        Amount = x.Amount,
                        TimeStamp = x.TimeStamp,
                        Barcode = x.ValidationId
                    }).OrderByDescending(x => x.SequenceNumber).ToList();
                var ticket = ticketCreator.Create(1, cashSlips);
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
                .IsPrintable<VoucherOutTransaction>();
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var voucherOutTransactions = transactionHistory.RecallTransactions<VoucherOutTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return voucherOutTransactions.Any() ? voucherOutTransactions.First().LogSequence : -1;
        }

        public Ticket GenerateReprintTicket(long transactionId)
        {
            var transactions = ServiceManager.GetInstance().GetService<ITransactionHistory>()
                .RecallTransactions<VoucherOutTransaction>().OrderByDescending(log => log.LogSequence).ToList();
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            if (!transactions.Any())
            {
                eventBus.Publish(
                    new OperatorMenuPopupEvent(
                        true,
                        Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.LogsNoRecords)));
                return null;
            }

            var transaction =
                transactions.FirstOrDefault(
                    t => t.TransactionId.ToString() == transactionId.ToString());

            if (transaction == null)
            {
                return null;
            }

            Ticket ticket;
            if (transaction.TypeOfAccount == AccountType.Cashable || transaction.TypeOfAccount == AccountType.Promo)
            {
                ticket = VoucherTicketsCreator.CreateCashOutReprintTicket(transaction);
            }
            else
            {
                ticket = VoucherTicketsCreator.CreateCashOutRestrictedReprintTicket(transaction);
            }

            eventBus.Publish(new VoucherReprintRequestEvent(transaction)); // VLT-8628

            Logger.Info(
                "Reprinting voucher - Name: " + transaction.Name + "  Transaction Id: " + transaction.TransactionId +
                "  Date: " + transaction.TransactionDateTime);
            return ticket;
        }

        private static string GetTypeOfAccount(VoucherOutTransaction transaction)
        {
            if (!string.IsNullOrEmpty(transaction.LogDisplayType))
            {
                return transaction.LogDisplayType;
            }

            string typeOfAccount;
            switch (transaction.TypeOfAccount)
            {
                case AccountType.Cashable:
                    typeOfAccount = Localizer.For(CultureFor.Player).GetString(ResourceKeys.Cashable);
                    break;
                case AccountType.Promo:
                    typeOfAccount = Localizer.For(CultureFor.Player).GetString(ResourceKeys.CashablePromotion);
                    break;
                case AccountType.NonCash:
                    typeOfAccount = Localizer.For(CultureFor.Player).GetString(ResourceKeys.NonCashablePromotional);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transaction.TypeOfAccount), transaction.TypeOfAccount, null);
            }

            return typeOfAccount;
        }
    }
}
