namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts.Bonus;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Bonus events/transactions.
    /// </summary>
    public class BonusEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        public string LogType => EventLogType.BonusAward.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            (string, string)[] BuildAdditionalInfo(BonusTransaction transaction)
            {
                var result = new List<(string, string)>
                {
                    GetDateAndTimeHeader(transaction.TransactionDateTime),
                    (ResourceKeys.TransactionId, transaction.TransactionId.ToString()),
                    (ResourceKeys.PaidAmount, transaction.PaidAmount.MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.TransactionTypeHeader, transaction.Mode.GetDescription(typeof(BonusMode))),
                    (ResourceKeys.PayMethod, transaction.PayMethod.GetDescription(typeof(PayMethod))),
                    (ResourceKeys.TotalSentByHost, (transaction.CashableAmount + transaction.NonCashAmount + transaction.PromoAmount).MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.CashableAmount, transaction.CashableAmount.MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.NonCashAmount, transaction.NonCashAmount.MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.PromoAmount, transaction.PromoAmount.MillicentsToDollars().FormattedCurrencyString()),
                    (ResourceKeys.BonusState, transaction.Exception == 0 ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Success) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failure)),
                    (ResourceKeys.FailedError, ((BonusException)transaction.Exception).GetDescription(typeof(BonusException))),
                };

                if (transaction.JackpotNumber != 0
                    || !string.IsNullOrEmpty(transaction.SourceID)
                    || !string.IsNullOrEmpty(transaction.Message))
                {
                    result.AddRange(new[] { (ResourceKeys.JackpotNumber, transaction.JackpotNumber.ToString()), (ResourceKeys.SourceID, transaction.SourceID), (ResourceKeys.BonusReason, transaction.Message) });
                }

                return result.ToArray();
            }

            var bonusTransactions = transactionHistory.RecallTransactions<BonusTransaction>()
                .OrderByDescending(x => x.LogSequence);
            var events = from transaction in bonusTransactions
                         let additionalInfo = BuildAdditionalInfo(transaction)
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BonusAward),
                             transaction.PaidAmount.MillicentsToDollars().FormattedCurrencyString())
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             transaction.TransactionId,
                             transaction.TransactionDateTime,
                             additionalInfo);
            return events;
        }

        public long GetMaxLogSequence()
        {
            var history = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var bonusTransactions = history.RecallTransactions<BonusTransaction>()
                .OrderByDescending(log => log.LogSequence).ToList();
            return bonusTransactions.Any() ? bonusTransactions.First().LogSequence : -1;
        }
    }
}
