namespace Aristocrat.Monaco.Accounting.CoinAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.CoinAcceptor;
    using Hardware.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;
    
    /// <summary>
    ///     Log adapter for handling/transforming Coin events/transactions.
    /// </summary>
    public class CoinEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter

    {
        protected readonly ILog Logger = LogManager.GetLogger(typeof(CoinEventLogAdapter));

        public string LogType => EventLogType.CoinIn.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var dateFormat = PropertiesManagerExtensions.GetValue(propertiesManager, ApplicationConstants.LocalizationOperatorDateFormat, ApplicationConstants.DefaultDateTimeFormat);
            var dateTimeFormat = $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}";

            //will be set at configuration and fetched from there.
            long tokenValue = propertiesManager.GetValue(HardwareConstants.CoinValue, AccountingConstants.DefaultTokenValue);

            var timeService = ServiceManager.GetInstance().GetService<ITime>();

            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

            var coinTransactions = Enumerable.OrderByDescending(transactionHistory.RecallTransactions<CoinTransaction>(), x => x.TransactionId);
            var events = (from transaction in coinTransactions
                          let additionalInfo = new[]{
                    (ResourceKeys.DenominationHeader, tokenValue.MillicentsToDollars().FormattedCurrencyString()),
                    GetDateAndTimeHeader(ResourceKeys.InsertedTimeHeader, transaction.TransactionDateTime),
                    (ResourceKeys.AcceptedTimeHeader, timeService.GetFormattedLocationTime(TimeZoneInfo.ConvertTimeFromUtc(transaction.Accepted, timeService.TimeZoneInformation), dateTimeFormat)),
                    (ResourceKeys.AmountCreditedHeader,tokenValue.MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.DetailsHeader, CoinAccountingExtensions.GetDetailsMessage(transaction.Details))}
                          let name = string.Join(
                              EventLogUtilities.EventDescriptionNameDelimiter,
                              CoinAccountingExtensions.GetDetailsMessage(transaction.Details),
                              tokenValue.MillicentsToDollars().FormattedCurrencyString())
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
            var coinTransactions = transactionHistory.RecallTransactions<CoinTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return coinTransactions.Any() ? coinTransactions.First().LogSequence : -1;
        }
    }
}
