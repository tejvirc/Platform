namespace Aristocrat.Monaco.Gaming.UI.Utils
{
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Models;
    using Localization.Properties;

    public static class OverlayMessageUtils
    {
        public static double ToCredits(long valueInMillicents)
        {
            return (double)valueInMillicents.MillicentsToDollars();
        }

        public static IMessageOverlayData GetCashoutTextData(
            IMessageOverlayData data,
            bool lastCashOutForcedByMaxBank,
            LobbyCashOutState cashOutState,
            bool printHandpayReceipt,
            long lastCashOutAmount,
            long handpayAmount)
        {
            var cashoutAmountText = ToCredits(lastCashOutAmount).FormattedCurrencyString();
            var cashoutTypeText = string.Empty;
            switch (cashOutState)
            {
                case LobbyCashOutState.Voucher:
                    var voucherKey = lastCashOutForcedByMaxBank
                        ? ResourceKeys.MaximumValueReachedCashOutText2
                        : ResourceKeys.PrintingTicket;
                    cashoutTypeText = Localizer.For(CultureFor.Player).GetString(voucherKey);
                    break;
                case LobbyCashOutState.Wat:
                    cashoutTypeText = Localizer.For(CultureFor.Player).GetString(ResourceKeys.WatOutText);
                    break;
                case LobbyCashOutState.HandPay:
                    cashoutAmountText = handpayAmount != 0 ? ToCredits(handpayAmount).FormattedCurrencyString() : cashoutAmountText;
                    cashoutTypeText = printHandpayReceipt
                        ? Localizer.For(CultureFor.Player).GetString(ResourceKeys.PrintHandPayText)
                        : Localizer.For(CultureFor.Player).GetString(ResourceKeys.Handpay);
                    break;
            }

            return GenerateCashoutTextData(data, lastCashOutForcedByMaxBank, cashoutTypeText, cashoutAmountText);
        }

        private static IMessageOverlayData GenerateCashoutTextData(
            IMessageOverlayData data,
            bool lastCashOutForcedByMaxBank,
            string cashoutTypeText,
            string cashoutAmountText)
        {
            if (lastCashOutForcedByMaxBank)
            {
                data.Text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MaximumValueReachedCashOutText1);
                data.IsSubText2Visible = true;
                data.SubText = cashoutTypeText;
                data.SubText2 = cashoutAmountText;
            }
            else
            {
                data.Text = cashoutTypeText;
                data.SubText = cashoutAmountText;
            }

            return data;
        }
    }
}
