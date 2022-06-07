namespace Aristocrat.Monaco.Gaming.UI
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using System;
    using System.Reflection;
    using Utils;

    public class BasicOverlayMessageStrategy : IOverlayMessageStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;

        public BasicOverlayMessageStrategy(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public long LastCashOutAmount { get; set; }

        public long HandpayAmount { get; set; }

        public long LargeWinWager { get; set; }

        public HandpayType? LastHandpayType { get; set; } = HandpayType.CancelCredit;

        public bool CashOutButtonPressed { get; set; } = false;

        public bool IsBasic => true;

        public IMessageOverlayData HandleMessageOverlayCashOut(IMessageOverlayData data, bool lastCashOutForcedByMaxBank, LobbyCashOutState cashOutState)
        {
            Logger.Debug("BasicOverlayMessageStrategy HandleMessageOverlayCashout entered");
            var printHandpayReceipt = _properties.GetValue(AccountingConstants.EnableReceipts, false);
            return OverlayMessageUtils.GetCashoutTextData(
                data,
                lastCashOutForcedByMaxBank,
                cashOutState,
                printHandpayReceipt,
                LastCashOutAmount,
                HandpayAmount);
        }

        public IMessageOverlayData HandleMessageOverlayCashIn(IMessageOverlayData data, CashInType cashInType, bool stateContainsCashOut, LobbyCashOutState cashOutState)
        {
            Logger.Debug("HandleMessageOverlayCashIn entered");
            switch (cashInType)
            {
                case CashInType.Currency:
                    data.SubText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InsertingBillText);
                    break;
                case CashInType.Voucher:
                    data.SubText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InsertingVoucherText);
                    break;
                case CashInType.Wat:
                    if (stateContainsCashOut &&
                        (cashOutState == LobbyCashOutState.Voucher || cashOutState == LobbyCashOutState.HandPay))
                    {
                        data = HandleMessageOverlayCashOut(data, false, cashOutState);
                    }
                    else
                    {
                        data.Text = Resources.WatOnText;
                    }
                    break;
            }

            return data;
        }

        public IMessageOverlayData HandleMessageOverlayHandPay(IMessageOverlayData data, string subText2)
        {
            // Nothing to be done here. Basic does not use MessageOverlayState.Handpay

            return data;
        }
    }
}
