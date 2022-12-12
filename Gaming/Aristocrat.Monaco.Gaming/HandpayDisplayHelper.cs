namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Kernel;
    using Kernel.MessageDisplay;
    using Localization.Properties;
    using Quartz.Util;

    /// <summary>
    /// Utility class to display information related to handpays
    /// </summary>
    public static class HandpayDisplayHelper
    {
        private static readonly ITransactionHistory History =
            ServiceManager.GetInstance().GetService<ITransactionHistory>();

        private static readonly IMessageDisplay MessageDisplay =
            ServiceManager.GetInstance().GetService<IMessageDisplay>();

        private static readonly IGameHistory GameHistory = ServiceManager.GetInstance().GetService<IGameHistory>();


        /// <summary>
        /// Attempts to recover an alternative cancel credit ticker message if applicable.
        /// </summary>
        public static void RecoverAlternativeCancelCreditTickerMessage()
        {
            if (!AlternativeCancelCreditMessageIsUsed())
            {
                return;
            }

            var displayableMessage = GetAlternativeCancelCreditTickerMessage();
            if (displayableMessage != null)
            {
                MessageDisplay.RemoveMessage(displayableMessage);
                MessageDisplay.DisplayMessage(displayableMessage);
            }
        }

        /// <summary>
        /// Displays the appropriate ticker message in Platform for the transaction given.
        /// </summary>
        /// <param name="transaction">The handpay transaction</param>
        public static void HandleHandpayMessageDisplay(HandpayTransaction transaction)
        {
            var displayableMessage = GetTickerMessage(transaction);
            if (displayableMessage != null)
            {
                MessageDisplay.RemoveMessage(displayableMessage);
                MessageDisplay.DisplayMessage(displayableMessage);
            }
        }

        private static IDisplayableMessage GetTickerMessage(HandpayTransaction transaction)
        {
            if (transaction.TransactionAmount <= 0)
            {
                return null;
            }

            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                case HandpayType.BonusPay:
                    return GenerateDisplayableMessage(
                        Localizer.GetString(
                            transaction.IsCreditType()
                                ? ResourceKeys.JackpotToCreditsKeyedOff
                                : ResourceKeys.JackpotHandpayKeyedOff, CultureProviderType.Player),
                        transaction.TransactionAmount);
                case HandpayType.CancelCredit:
                    IDisplayableMessage displayableMessage = AlternativeCancelCreditMessageIsUsed()
                        ? GetAlternativeCancelCreditTickerMessage()
                        : GenerateDisplayableMessage(
                            Localizer.GetString(ResourceKeys.CashOutHandpayKeyedOff, CultureProviderType.Player),
                            transaction.TransactionAmount);
                    return displayableMessage;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Returns true if the alternative cancel credit ticker message should be used, false otherwise
        /// </summary>
        /// <returns></returns>
        private static bool AlternativeCancelCreditMessageIsUsed()
        {
            return !Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)
                .IsNullOrWhiteSpace();
        }

        private static IList<HandpayTransaction> GetAllCancelCreditTransactionsSinceLastPlayedTime()
        {
            var lastIdleState = GameHistory.GetGameHistory().Where(g => g.PlayState == PlayState.Idle).OrderByDescending(g => g.EndDateTime).FirstOrDefault();
            return History.RecallTransactions<HandpayTransaction>().Where(
                    t => t.HandpayType == HandpayType.CancelCredit && t.KeyOffType != KeyOffType.Cancelled &&
                         t.KeyOffType != KeyOffType.Unknown && t.State == HandpayState.Committed &&
                         t.TransactionDateTime >= (lastIdleState?.EndDateTime ?? DateTime.MinValue))
                .OrderByDescending(t => t.TransactionDateTime).ToList();
        }

        private static IDisplayableMessage GetAlternativeCancelCreditTickerMessage()
        {
            IDisplayableMessage displayableMessage;
            var messageContent = Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashOutHandpayKeyedOff);
            var orderedTransactions = GetAllCancelCreditTransactionsSinceLastPlayedTime();
            if (orderedTransactions.Sum(t => t.TransactionAmount) <= 0)
            {
                return null;
            }

            if (orderedTransactions.Count == 1)
            {
                displayableMessage = GenerateCancelCreditDisplayableMessageWithGuid(
                    messageContent,
                    orderedTransactions.First().TransactionAmount);
            }
            else
            {
                messageContent += Localizer.For(CultureFor.PlayerTicket)
                    .GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix);
                displayableMessage = GenerateCancelCreditDisplayableMessageWithGuid(
                    messageContent,
                    orderedTransactions.First().TransactionAmount,
                    orderedTransactions.Sum(t => t.TransactionAmount));
            }

            return displayableMessage;
        }

        private static IDisplayableMessage GenerateDisplayableMessage(string message, long transactionAmount)
        {
            return new DisplayableMessage(
                () => string.Format(
                    message,
                    transactionAmount.MillicentsToDollars().FormattedCurrencyString()),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(HandpayKeyedOffEvent));
        }

        private static IDisplayableMessage GenerateCancelCreditDisplayableMessageWithGuid(
            string message,
            long transactionAmount)
        {
            return new DisplayableMessage(
                () => string.Format(
                    message,
                    transactionAmount.MillicentsToDollars().FormattedCurrencyString()),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(HandpayKeyedOffEvent),
                AccountingConstants.AlternativeCancelCreditTickerMessageGuid);
        }

        private static IDisplayableMessage GenerateCancelCreditDisplayableMessageWithGuid(
            string message,
            long transactionAmount,
            long totalAmount)
        {
            return new DisplayableMessage(
                () => string.Format(
                    message,
                    transactionAmount.MillicentsToDollars().FormattedCurrencyString(),
                    totalAmount.MillicentsToDollars().FormattedCurrencyString()),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(HandpayKeyedOffEvent),
                AccountingConstants.AlternativeCancelCreditTickerMessageGuid);
        }
    }
}