namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Kernel;
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

        private static DisplayableMessage GetTickerMessage(HandpayTransaction transaction)
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
                        Localizer.For(CultureFor.PlayerTicket).GetString(
                            transaction.IsCreditType()
                                ? ResourceKeys.JackpotToCreditsKeyedOff
                                : ResourceKeys.JackpotHandpayKeyedOff),
                        transaction.TransactionAmount);
                case HandpayType.CancelCredit:
                    DisplayableMessage displayableMessage = AlternativeCancelCreditMessageIsUsed()
                        ? GetAlternativeCancelCreditTickerMessage()
                        : GenerateDisplayableMessage(
                            Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CashOutHandpayKeyedOff),
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

        private static DisplayableMessage GetAlternativeCancelCreditTickerMessage()
        {
            DisplayableMessage displayableMessage;
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

        private static DisplayableMessage GenerateDisplayableMessage(string message, long transactionAmount)
        {
            return new(
                () => string.Format(
                    message,
                    transactionAmount.MillicentsToDollars().FormattedCurrencyString()),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(HandpayKeyedOffEvent));
        }

        private static DisplayableMessage GenerateCancelCreditDisplayableMessageWithGuid(
            string message,
            long transactionAmount)
        {
            return new(
                () => string.Format(
                    message,
                    transactionAmount.MillicentsToDollars().FormattedCurrencyString()),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(HandpayKeyedOffEvent),
                AccountingConstants.AlternativeCancelCreditTickerMessageGuid);
        }

        private static DisplayableMessage GenerateCancelCreditDisplayableMessageWithGuid(
            string message,
            long transactionAmount,
            long totalAmount)
        {
            return new(
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