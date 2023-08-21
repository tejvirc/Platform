namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Transfer In (WatOn) events/transactions.
    /// </summary>
    public class TransferInEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        private readonly double _multiplier;

        public string LogType => EventLogType.TransferIn.GetDescription(typeof(EventLogType));

        public TransferInEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var watOnTransactions = transactionHistory.RecallTransactions<WatOnTransaction>()
                .OrderByDescending(x => x.LogSequence);
            var events = from transaction in watOnTransactions
                         let additionalInfo = new[] {
                             GetDateAndTimeHeader(transaction.TransactionDateTime),
                             (ResourceKeys.WatNumber, transaction.LogSequence.ToString()),
                             (ResourceKeys.DeviceId, transaction.DeviceId.ToString()),
                             (ResourceKeys.HostTransactionId, transaction.RequestId),
                             (ResourceKeys.CashableHeader, transaction.AuthorizedCashableAmount > 0 && _multiplier > 0 ? $"{(transaction.AuthorizedCashableAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedCashableAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.PromoHeader, transaction.AuthorizedPromoAmount > 0 && _multiplier > 0 ? $"{(transaction.AuthorizedPromoAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedPromoAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.NonCashableHeader, transaction.AuthorizedNonCashAmount > 0 && _multiplier > 0 ? $"{(transaction.AuthorizedNonCashAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedNonCashAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.StatusHeader, GetEnumDescriptionString(transaction.Status))}
                         let amount = (transaction.AuthorizedCashableAmount > 0 && _multiplier > 0
                                          ? transaction.AuthorizedCashableAmount / _multiplier
                                          : transaction.AuthorizedCashableAmount) +
                                      (transaction.AuthorizedPromoAmount > 0 && _multiplier > 0
                                          ? transaction.AuthorizedPromoAmount / _multiplier
                                          : transaction.AuthorizedPromoAmount) +
                                      (transaction.AuthorizedNonCashAmount > 0 && _multiplier > 0
                                          ? transaction.AuthorizedNonCashAmount / _multiplier
                                          : transaction.AuthorizedNonCashAmount)
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferIn),
                             amount.FormattedCurrencyString())
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             transaction.TransactionId,
                             transaction.TransactionDateTime,
                             additionalInfo){ LogSequence = transaction.LogSequence };
            return events;
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var watOnTransactions = transactionHistory.RecallTransactions<WatOnTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return watOnTransactions.Any() ? watOnTransactions.First().LogSequence : -1;
        }

        protected string GetEnumDescriptionString(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
