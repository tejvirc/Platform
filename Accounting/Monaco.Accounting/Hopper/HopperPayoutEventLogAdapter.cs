namespace Aristocrat.Monaco.Accounting.Hopper
{
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts.Extensions;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Hardware.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Hopper Payout events/transactions.
    /// </summary>
    public class HopperPayoutEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter

    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(HopperPayoutEventLogAdapter));

        public string LogType => EventLogType.CoinOut.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            
            //will be set at configuration and fetched from there.
            long tokenValue = propertiesManager.GetValue(HardwareConstants.CoinValue, 100000);

            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var coinOutTransactions = Enumerable.OrderByDescending(transactionHistory.RecallTransactions<CoinOutTransaction>(), x => x.TransactionId);
            var events = (from transaction in coinOutTransactions
                          let additionalInfo = new[]{
                    (ResourceKeys.DenominationHeader, tokenValue.MillicentsToDollars().FormattedCurrencyString()),
                    GetDateAndTimeHeader(ResourceKeys.TimeLabel, transaction.TransactionDateTime),
                    (ResourceKeys.PaidAmount,transaction.TransactionAmount.MillicentsToDollars().FormattedCurrencyString())}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              //TBC Resources.HopperPayOut,
                              "Hopper Pay Out {0}",
                              transaction.TransactionAmount.MillicentsToDollars().FormattedCurrencyString())
                          select new EventDescription(
                              name,
                              "info",
                              LogType,
                              transaction.TransactionId,
                              transaction.TransactionDateTime,
                              additionalInfo)).ToList();
            return events;
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var coinOutTransactions = transactionHistory.RecallTransactions<CoinOutTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return coinOutTransactions.Any() ? coinOutTransactions.First().LogSequence : -1;
        }
    }
}
