namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming keyed credits transaction.
    /// </summary>
    public class KeyedCreditsEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(KeyedCreditsEventLogAdapter));

        public string LogType => EventLogType.KeyedCredit.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var billTransactions = transactionHistory.RecallTransactions<KeyedCreditsTransaction>()
                .OrderByDescending(x => x.TransactionDateTime);
            var events = (from transaction in billTransactions
                          let additionalInfo = new[]{
                              GetDateAndTimeHeader(transaction.TransactionDateTime),
                              (ResourceKeys.Amount, transaction.Amount.MillicentsToDollars().FormattedCurrencyString()),
                              (ResourceKeys.AccountTypeHeader, transaction.AccountType.GetDescription(typeof(AccountType))),
                              (ResourceKeys.KeyedCreditType, transaction.KeyedType)}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeyedCredit),
                              transaction.Name,
                              transaction.FormattedValue)
                          select new EventDescription(
                              name,
                              "info",
                              "KeyedCredit",
                              transaction.TransactionId,
                              transaction.TransactionDateTime,
                              additionalInfo)).ToList();
            return events;
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var transactions = transactionHistory.RecallTransactions<KeyedCreditsTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return transactions.Any() ? transactions.First().LogSequence : -1;
        }
    }
}
