namespace Aristocrat.Monaco.Accounting.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Aristocrat.Monaco.Accounting.Contracts.Hopper;
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

        public string LogType => EventLogType.HopperRefill.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var dateFormat = PropertiesManagerExtensions.GetValue(propertiesManager, ApplicationConstants.LocalizationOperatorDateFormat, ApplicationConstants.DefaultDateTimeFormat);
            var dateTimeFormat = $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}";

            //will be set at configuration and fetched from there.
            long tokenValue = propertiesManager.GetValue(HardwareConstants.CoinValue, 100000);

            var timeService = ServiceManager.GetInstance().GetService<ITime>();

            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var hopperRefillTransactions = Enumerable.OrderByDescending(transactionHistory.RecallTransactions<HopperRefillTransaction>(), x => x.TransactionId);
            var events = (from transaction in hopperRefillTransactions
                          let additionalInfo = new[]{
                    (ResourceKeys.DenominationHeader, tokenValue.MillicentsToDollars().FormattedCurrencyString()),
                    GetDateAndTimeHeader(ResourceKeys.InsertedTimeHeader, transaction.TransactionDateTime),
                    (ResourceKeys.AmountCreditedHeader,transaction.LastRefillValue.MillicentsToDollars().FormattedCurrencyString())}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              //TBC Resources.HopperRefill,
                              "Hopper Refill",
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

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var hopperRefillTransactions = transactionHistory.RecallTransactions<HopperRefillTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return hopperRefillTransactions.Any() ? hopperRefillTransactions.First().LogSequence : -1;
        }
    }
}
