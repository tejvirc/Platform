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
    using Contracts.Vouchers;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Voucher In events/transactions.
    /// </summary>
    public class VoucherInEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        private readonly double _multiplier;

        public string LogType => EventLogType.VoucherIn.GetDescription(typeof(EventLogType));

        public VoucherInEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var voucherInTransactions = transactionHistory.RecallTransactions<VoucherInTransaction>()
                .OrderByDescending(x => x.TransactionDateTime);
            var events = from transaction in voucherInTransactions
                         let additionalInfo = new[]{
                             (ResourceKeys.TicketNumber, transaction.VoucherSequence.ToString()),
                             GetDateAndTimeHeader(transaction.TransactionDateTime),
                             (ResourceKeys.AmountHeader, transaction.Amount > 0 && _multiplier > 0 ?
                                $"{(transaction.Amount / _multiplier).FormattedCurrencyString()}" : $"{transaction.Amount.FormattedCurrencyString()}"),
                             (ResourceKeys.TypeOfAccountHeader, GetTypeOfAccount(transaction)),
                             (ResourceKeys.ValidationNumber, VoucherExtensions.GetValidationString(transaction.Barcode)),
                             (ResourceKeys.StatusHeader, VoucherExtensions.GetStatusText(transaction)),
                             (ResourceKeys.DetailsHeader, VoucherExtensions.GetDetailsMessage(transaction.Exception))}
                         let amount = transaction.Amount > 0 && _multiplier > 0
                             ? transaction.Amount / _multiplier
                             : transaction.Amount
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn),
                             amount.FormattedCurrencyString(),
                             VoucherExtensions.GetStatusText(transaction))
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
            var voucherInTransactions = transactionHistory.RecallTransactions<VoucherInTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return voucherInTransactions.Any() ? voucherInTransactions.First().LogSequence : -1;
        }

        protected static string GetTypeOfAccount(VoucherInTransaction transaction)
        {
            if (!string.IsNullOrEmpty(transaction.LogDisplayType))
            {
                return transaction.LogDisplayType;
            }

            var typeOfAccount = string.Empty;
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
            }

            return typeOfAccount;
        }
    }
}
