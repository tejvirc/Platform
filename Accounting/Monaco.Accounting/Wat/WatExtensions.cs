namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Extension methods for an IWatTransferOnHandler and IWatOffProvider.
    /// </summary>
    public static class WatExtensions
    {
        /// <summary>Return a informative string with amount and type of amount.</summary>
        /// <param name="amount">The value to represent as amount.</param>
        /// <param name="label">The value to represent as label for amount.</param>
        /// <returns>informative string with amount and type of amount if amount if non zero otherwise Empty</returns>
        public static string GetAmountWithLabel(long amount, string label)
        {
            var localizeLabel = Localizer.For(CultureFor.PlayerTicket).GetString(label);

            return amount > 0
                ? $" {localizeLabel} {amount.MillicentsToDollars().FormattedCurrencyString()}"
                : string.Empty;
        }

        /// <summary>Return a informative string for Wat transfer in/out.</summary>
        /// <param name="cashableAmount">The value to represent as cashable amount.</param>
        /// <param name="promoAmount">The value to represent as promo amount.</param>
        /// <param name="nonCashableAmount">The value to represent as nonCashable amount.</param>
        /// <param name="transferTypeKey">The value to represent as label for amount.</param>
        /// <returns>informative string for Wat transfer in/out.</returns>
        public static List<string> GetWatTransferMessage(
            string transferTypeKey,
            long cashableAmount,
            long promoAmount,
            long nonCashableAmount)
        {
            var transferMessages = new List<string>();

            AddTransferMessage(cashableAmount, ResourceKeys.CashableHeader);
            AddTransferMessage(promoAmount, ResourceKeys.PromoHeader);
            AddTransferMessage(nonCashableAmount, ResourceKeys.NonCashableHeader);

            return transferMessages;

            void AddTransferMessage(long amount, string transferTypeLabel)
            {
                if (amount > 0)
                {
                    transferMessages.Add(Localizer.For(CultureFor.PlayerTicket).GetString(transferTypeKey) + GetAmountWithLabel(amount, transferTypeLabel));
                }
            }
        }
    }
}