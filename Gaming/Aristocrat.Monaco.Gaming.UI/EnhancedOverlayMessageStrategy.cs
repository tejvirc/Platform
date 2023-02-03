namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Utils;

    public class EnhancedOverlayMessageStrategy : IOverlayMessageStrategy
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string HandPayDisplayKey = "HandPayImage";
        private const string HandPayOverrideDisplayKey = "HandPayOverrideImage";

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;

        private ICommand ExitHandpayPendingCommand { get; }

        public EnhancedOverlayMessageStrategy(IPropertiesManager properties, IEventBus eventBus, ISystemDisableManager disableManager)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));

            ExitHandpayPendingCommand = new RelayCommand<object>(OnExitHandpayPendingPressed);
        }

        public long LastCashOutAmount { get; set; }

        public long HandpayAmount { get; set; }

        public long LargeWinWager { get; set; }

        public HandpayType? LastHandpayType { get; set; }

        public bool CashOutButtonPressed { get; set; } = false;

        public bool IsBasic => false;

        public IMessageOverlayData HandleMessageOverlayCashOut(
            IMessageOverlayData data,
            bool lastCashOutForcedByMaxBank,
            LobbyCashOutState cashOutState)
        {
            Logger.Debug($"HandleMessageOverlayCashOut entered. CashOutState={cashOutState}, lastCashOutForcedByMaxBank={lastCashOutForcedByMaxBank}");
            data.DisplayForEvents = true;

            if (cashOutState == LobbyCashOutState.Undefined)
            {
                return data;
            }

            if (lastCashOutForcedByMaxBank && cashOutState == LobbyCashOutState.Voucher)
            {
                data = HandleMessageOverlayForcedCashOut(data);
            }
            else
            {
                switch (cashOutState)
                {
                    case LobbyCashOutState.Voucher:
                        data = HandleMessageOverlayVoucher(data, lastCashOutForcedByMaxBank, cashOutState);
                        break;
                    case LobbyCashOutState.HandPay:
                        data = HandleMessageOverlayHandPayCashout(data);
                        break;
                    case LobbyCashOutState.Wat:
                        data = HandleMessageOverlayWat(data, lastCashOutForcedByMaxBank);
                        break;
                }
            }

            return data;
        }

        public IMessageOverlayData HandleMessageOverlayCashIn(
            IMessageOverlayData data,
            CashInType cashInType,
            bool stateContainsCashOut,
            LobbyCashOutState cashOutState)
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
            Logger.Debug("HandleMessageOverlayHandPay entered");

            data.DisplayForEvents = true;
            if (_properties.GetValue(GamingConstants.HandpayPresentationOverride, false))
            {
                Logger.Debug($"Overriding handpay presentation, key: {HandPayOverrideDisplayKey}");

                data.DisplayImageResourceKey = HandPayOverrideDisplayKey;
                return data;
            }
            data.DisplayImageResourceKey = HandPayDisplayKey;
            data.IsSubText2Visible = true;

            data.Text = CashOutButtonPressed || LastHandpayType == HandpayType.CancelCredit
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HandpayPresentationText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JackpotPresentationText);

            data.SubText = OverlayMessageUtils.ToCredits(HandpayAmount).FormattedCurrencyString();
            data.SubText2 = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CancelCreditPending);

            if (_properties.GetValue(ApplicationConstants.ShowWagerWithLargeWinInfo, false) &&
                LastHandpayType == HandpayType.GameWin && LargeWinWager > 0)
            {
                data.IsSubText3Visible = true;
                data.SubText3 = Localizer.For(CultureFor.Operator)
                    .FormatString(ResourceKeys.JackpotWager, OverlayMessageUtils.ToCredits(LargeWinWager).FormattedCurrencyString());
            }

            // Enable Cancel handpay for CancelCredit only
            var enableHandpayExit = (bool)_properties.GetProperty(
                AccountingConstants.HandpayPendingExitEnabled,
                false);
            if (enableHandpayExit && LastHandpayType == HandpayType.CancelCredit)
            {
                data.IsButtonVisible = true;
                data.ButtonCommand = ExitHandpayPendingCommand;
                data.ButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlayOnText);
                data.SubText2 = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CancelCreditPending);
            }

            if (enableHandpayExit && LastHandpayType == HandpayType.GameWin)
            {
                // Display a different message for Substantial Win
                data.SubText2 = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SubstantialWinHandpay);
            }

            return data;
        }

        private IMessageOverlayData HandleMessageOverlayForcedCashOut(IMessageOverlayData data)
        {
            Logger.Debug("HandleMessageOverlayForcedCashOut entered");
            data.IsSubText2Visible = true;
            data.DisplayForEvents = true;
            data.DisplayImageResourceKey = HandPayDisplayKey;

            data.Text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashWinPresentationVoucher);
            data.SubText = OverlayMessageUtils.ToCredits(LastCashOutAmount).FormattedCurrencyString();
            data.SubText2 = Localizer.For(CultureFor.Operator)
                             .GetString(ResourceKeys.MaximumValueReachedCashOutText1) + " " +
                             Localizer.For(CultureFor.Operator).FormatString(
                             ResourceKeys.PrintingPresentationVoucher,
                             OverlayMessageUtils.ToCredits(LastCashOutAmount).FormattedCurrencyString());

            return data;
        }

        private IMessageOverlayData HandleMessageOverlayVoucher(IMessageOverlayData data, bool lastCashOutForcedByMaxBank, LobbyCashOutState cashOutState)
        {
            Logger.Debug("HandleMessageOverlayVoucher entered");
            if (CashOutButtonPressed)
            {
                data.IsSubText2Visible = true;
                data.SubText2 = Environment.NewLine +
                                Localizer.For(CultureFor.Operator).FormatString(
                                ResourceKeys.PrintingPresentationVoucher,
                                OverlayMessageUtils.ToCredits(LastCashOutAmount)
                                .FormattedCurrencyString());
            }
            else
            {
                //Making enhancements false
                data.DisplayForEvents = false;
                data.DisplayImageResourceKey = string.Empty;

                var printHandpayReceipt = _properties.GetValue(AccountingConstants.EnableReceipts, false);

                data = OverlayMessageUtils.GetCashoutTextData(
                    data,
                    lastCashOutForcedByMaxBank,
                    cashOutState,
                    printHandpayReceipt,
                    LastCashOutAmount,
                    HandpayAmount);
            }

            return data;
        }

        private IMessageOverlayData HandleMessageOverlayHandPayCashout(IMessageOverlayData data)
        {
            // Do not set the message overlay to PAID yet if the handpay is still pending
            if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.HandpayPendingDisableKey))
            {
                Logger.Debug("HandleMessageOverlayHandPayCashout ignored due to handpay pending");
                return data;
            }

            Logger.Debug("HandleMessageOverlayHandPayCashout entered");

            data.SubText = OverlayMessageUtils.ToCredits(HandpayAmount).FormattedCurrencyString();
            data.Text = Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.HandPayPaidPresentationText);
            data.DisplayImageResourceKey = HandPayDisplayKey;

            var printHandpayReceipt = _properties.GetValue(AccountingConstants.EnableReceipts, false);
            if (printHandpayReceipt)
            {
                data.IsSubText2Visible = true;
                data.SubText2 = Localizer.For(CultureFor.Player).GetString(ResourceKeys.PrintHandPayText);
            }

            return data;
        }

        private IMessageOverlayData HandleMessageOverlayWat(IMessageOverlayData data, bool lastCashOutForcedByMaxBank)
        {
            Logger.Debug("HandleMessageOverlayWat entered");
            var printHandpayReceipt = _properties.GetValue(AccountingConstants.EnableReceipts, false);
            return OverlayMessageUtils.GetCashoutTextData(
                data,
                lastCashOutForcedByMaxBank,
                LobbyCashOutState.Wat,
                printHandpayReceipt,
                LastCashOutAmount,
                HandpayAmount);
        }

        private void OnExitHandpayPendingPressed(object obj)
        {
            _eventBus.Publish(new HandpayPendingCanceledEvent());
        }
    }
}
