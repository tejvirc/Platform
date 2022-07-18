namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Kernel;
    using Sas.Contracts.SASProperties;

    /// <summary>
    /// Extension methods for WatTransaction and WatOnTransaction
    /// </summary>
    public static class WatTransactionExtensions
    {
        /// <summary>
        /// Update authorized transaction amount for WatTransaction
        /// </summary>
        public static void UpdateAuthorizedTransactionAmount(this WatTransaction transaction, IBank bank, IPropertiesManager propertiesManager)
        {
            if (transaction.AllowReducedAmounts)
            {
                var transferLimit = propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                transaction.AuthorizedCashableAmount = Math.Min(
                    transaction.CashableAmount,
                    Math.Min(bank.QueryBalance(AccountType.Cashable), transferLimit));
                transferLimit -= transaction.AuthorizedCashableAmount;
                transaction.AuthorizedPromoAmount = Math.Min(
                    transaction.PromoAmount,
                    Math.Min(bank.QueryBalance(AccountType.Promo), transferLimit));
                transferLimit -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedNonCashAmount = Math.Min(
                    transaction.NonCashAmount,
                    Math.Min(bank.QueryBalance(AccountType.NonCash), transferLimit));
            }
            else
            {
                transaction.AuthorizedCashableAmount = transaction.CashableAmount;
                transaction.AuthorizedPromoAmount = transaction.PromoAmount;
                transaction.AuthorizedNonCashAmount = transaction.NonCashAmount;
            }
        }

        /// <summary>
        /// Update authorized transaction amount for WatOnTransaction
        /// </summary>
        public static void UpdateAuthorizedTransactionAmount(this WatOnTransaction transaction, IBank bank, IPropertiesManager propertiesManager)
        {
            if (transaction.AllowReducedAmounts)
            {
                var transferLimit = propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                var currentBalance = bank.QueryBalance();
                var availableBalance = Math.Min(
                    (currentBalance > bank.Limit ? 0 : bank.Limit - currentBalance),
                    transferLimit);
                transaction.AuthorizedNonCashAmount = Math.Min(transaction.NonCashAmount, availableBalance);
                availableBalance -= transaction.AuthorizedNonCashAmount;
                transaction.AuthorizedPromoAmount = Math.Min(transaction.PromoAmount, availableBalance);
                availableBalance -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedCashableAmount = Math.Min(transaction.CashableAmount, availableBalance);
            }
            else
            {
                transaction.AuthorizedCashableAmount = transaction.CashableAmount;
                transaction.AuthorizedPromoAmount = transaction.PromoAmount;
                transaction.AuthorizedNonCashAmount = transaction.NonCashAmount;
            }
        }


        /// <summary>
        /// Update authorized transaction amount for Egm initiated Cashout
        /// </summary>
        public static void UpdateAuthorizedHostCashoutAmount(this WatTransaction transaction, IPropertiesManager propertiesManager, bool partialTransferAllowed, long cashableAmount, long nonRestrictedAmount, long restrictedAmount)
        {
            if (partialTransferAllowed)
            {
                var transferLimit = propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                transaction.AuthorizedCashableAmount = Math.Min(
                    Math.Min(transaction.CashableAmount, transferLimit),
                    cashableAmount.CentsToMillicents());
                transferLimit -= transaction.AuthorizedCashableAmount;
                transaction.AuthorizedPromoAmount = Math.Min(
                    Math.Min(transaction.PromoAmount, transferLimit),
                    nonRestrictedAmount.CentsToMillicents());
                transferLimit -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedNonCashAmount = Math.Min(
                    Math.Min(transaction.NonCashAmount, transferLimit),
                    restrictedAmount.CentsToMillicents());
            }
            else
            {
                transaction.AuthorizedCashableAmount = cashableAmount.CentsToMillicents();
                transaction.AuthorizedPromoAmount = nonRestrictedAmount.CentsToMillicents();
                transaction.AuthorizedNonCashAmount = restrictedAmount.CentsToMillicents();
            }
        }
    }
}