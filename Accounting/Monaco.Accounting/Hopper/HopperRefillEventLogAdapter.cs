namespace Aristocrat.Monaco.Accounting.Hopper
{
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts.Extensions;
    using Application.Contracts.TiltLogger;
    using Accounting.Contracts.Hopper;
    using Common;
    using Contracts;
    using Hardware.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Hopper Refill events/transactions.
    /// </summary>
    public class HopperRefillEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter

    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(HopperRefillEventLogAdapter));

        /// <inheritdoc/>
        public string LogType => EventLogType.HopperRefill.GetDescription(typeof(EventLogType));

        /// <inheritdoc/>
        public IEnumerable<EventDescription> GetEventLogs()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            //will be set at configuration and fetched from there.
            var tokenValue = propertiesManager.GetValue(HardwareConstants.CoinValue, AccountingConstants.DefaultTokenValue);

            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var hopperRefillTransactions = Enumerable.OrderByDescending(transactionHistory.RecallTransactions<HopperRefillTransaction>(), x => x.TransactionId);
            var events = (from transaction in hopperRefillTransactions
                          let additionalInfo = new[]{
                    (ResourceKeys.DenominationHeader, tokenValue.MillicentsToDollars().FormattedCurrencyString()),
                    GetDateAndTimeHeader(ResourceKeys.InsertedTimeHeader, transaction.TransactionDateTime),
                    (ResourceKeys.AmountCreditedHeader,transaction.LastRefillValue.MillicentsToDollars().FormattedCurrencyString())}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              Resources.HopperRefill,
                              transaction.LastRefillValue.MillicentsToDollars().FormattedCurrencyString())
                          select new EventDescription(
                              name,
                              "info",
                              LogType,
                              transaction.TransactionId,
                              transaction.TransactionDateTime,
                              additionalInfo)).ToList();
            return events;
        }

        /// <inheritdoc/>
        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var hopperRefillTransactions = transactionHistory.RecallTransactions<HopperRefillTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return hopperRefillTransactions.Any() ? hopperRefillTransactions.First().LogSequence : -1;
        }
    }
}
