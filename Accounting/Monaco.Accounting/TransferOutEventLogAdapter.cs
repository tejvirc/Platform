namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.Wat;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Transfer Out (WatOff) events/transactions.
    /// </summary>
    public class TransferOutEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        private readonly double _multiplier;

        public string LogType => EventLogType.TransferOut.GetDescription(typeof(EventLogType));

        public TransferOutEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var watTransactions = transactionHistory.RecallTransactions<WatTransaction>()
                .OrderByDescending(x => x.LogSequence);
            var events = from transaction in watTransactions
                         let additionalInfo = new[]{
                             GetDateAndTimeHeader(transaction.TransactionDateTime),
                             (ResourceKeys.WatNumber, transaction.LogSequence.ToString()),
                             (ResourceKeys.DeviceId, transaction.DeviceId.ToString()),
                             (ResourceKeys.HostTransactionId, transaction.RequestId),
                             (ResourceKeys.CashableHeader,transaction.AuthorizedCashableAmount > 0 && _multiplier > 0 ?
                                   $"{(transaction.AuthorizedCashableAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedCashableAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.PromoHeader,transaction.AuthorizedPromoAmount > 0 && _multiplier > 0 ?
                                   $"{(transaction.AuthorizedPromoAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedPromoAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.NonCashableHeader,transaction.AuthorizedNonCashAmount > 0 && _multiplier > 0 ?
                                   $"{(transaction.AuthorizedNonCashAmount / _multiplier).FormattedCurrencyString()}" : $"{transaction.AuthorizedNonCashAmount.FormattedCurrencyString()}"),
                             (ResourceKeys.StatusHeader,    transaction.Status.GetDescription(typeof(WatStatus)))}
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
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferOut),
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

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var watTransactions = transactionHistory.RecallTransactions<WatTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return watTransactions.Any() ? watTransactions.First().LogSequence : -1;
        }
    }
}
