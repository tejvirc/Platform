namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.HandCount;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Log adapter for handling/transforming Hard Meter Out events/transactions.
    /// </summary>
    public class HardMeterOutEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        private readonly double _multiplier;

        public string LogType => EventLogType.HardMeterOut.GetDescription(typeof(EventLogType));

        public HardMeterOutEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var hardMeterOutTransactions = transactionHistory.RecallTransactions<HardMeterOutTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();

            var events = from transaction in hardMeterOutTransactions
                         let amount = transaction.Amount > 0 && _multiplier > 0
                             ? transaction.Amount / _multiplier
                             : transaction.Amount
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterOut),
                             $"#{transaction.LogSequence}",
                             amount.FormattedCurrencyString())
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             transaction.TransactionId,
                             transaction.TransactionDateTime)
                         { LogSequence = transaction.LogSequence };
            return events;
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var voucherOutTransactions = transactionHistory.RecallTransactions<VoucherOutTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return voucherOutTransactions.Any() ? voucherOutTransactions.First().LogSequence : -1;
        }
    }
}