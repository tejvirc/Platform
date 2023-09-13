namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Currency;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Rtp;
    using Contracts.Tickets;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Contains logic for JurisdictionSettingsPageViewModel.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class JurisdictionSettingsPageViewModel : OperatorMenuPageViewModelBase
    {
        private const string ShowAllowedRtpSetting = "ShowAllowedRTP";

        private readonly decimal _defaultAnyGameMinimum;
        private readonly decimal _defaultAnyGameMaximum;
        private readonly HardMeterLogicalState _mechanicalMeter;
        private readonly bool _doorOpticSensor;
        private readonly bool _zeroCreditOnOos;

        private string _allowedSlotRtp;
        private string _allowedPokerRtp;
        private string _allowedKenoRtp;
        private string _allowedBlackjackRtp;
        private string _allowedRouletteRtp;
        private string _currencyDisplayText;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JurisdictionSettingsPageViewModel" /> class.
        /// </summary>
        public JurisdictionSettingsPageViewModel()
        {
            Jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

            var localization = ServiceManager.GetInstance().GetService<ILocalization>();
            var currencyProvider = localization.GetProvider(CultureFor.Currency) as CurrencyCultureProvider;
            Currency = currencyProvider?.ConfiguredCurrency;
            _currencyDisplayText = Currency?.DisplayName;

            _defaultAnyGameMinimum = PropertiesManager.GetValue(GamingConstants.AnyGameMinimumReturnToPlayer, decimal.MinValue);
            _defaultAnyGameMaximum = PropertiesManager.GetValue(GamingConstants.AnyGameMaximumReturnToPlayer, decimal.MaxValue);

            SetupRtpValuesAndVisibility();

            var hardMeter = ServiceManager.GetInstance().TryGetService<IHardMeter>();
            _mechanicalMeter = hardMeter?.LogicalState ?? HardMeterLogicalState.Disabled;

            MechanicalMeterVisibility = PropertiesManager.GetValue(
                ApplicationConstants.ConfigWizardHardMetersConfigVisible,
                true);

            _doorOpticSensor = PropertiesManager.GetValue(
                ApplicationConstants.ConfigWizardDoorOpticsEnabled,
                false);

            _zeroCreditOnOos = !PropertiesManager.GetValue(
                ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                false);

        }

        public Currency Currency { get; }

        public string Jurisdiction { get; }

        public string CurrencyDisplayText
        {
            get => _currencyDisplayText;
            private set => SetProperty(ref _currencyDisplayText, value);
        }

        public string AllowedSlotRtp
        {
            get => _allowedSlotRtp;
            private set => SetProperty(ref _allowedSlotRtp, value);
        }

        public string AllowedPokerRtp
        {
            get => _allowedPokerRtp;
            private set => SetProperty(ref _allowedPokerRtp, value);
        }

        public string AllowedKenoRtp
        {
            get => _allowedKenoRtp;
            private set => SetProperty(ref _allowedKenoRtp, value);
        }

        public string AllowedBlackjackRtp
        {
            get => _allowedBlackjackRtp;
            private set => SetProperty(ref _allowedBlackjackRtp, value);
        }

        public string AllowedRouletteRtp
        {
            get => _allowedRouletteRtp;
            private set => SetProperty(ref _allowedRouletteRtp, value);
        }

        public string MechanicalMeter => HardMeterLogicalStateToString(_mechanicalMeter);

        public string DoorOpticSensor => _doorOpticSensor
            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledLabel)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);

        public string ZeroCreditOnOos => _zeroCreditOnOos
            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText);

        public bool SlotRtpVisibility { get; private set; }

        public bool PokerRtpVisibility { get; private set; }

        public bool KenoRtpVisibility { get; private set; }

        public bool BlackjackRtpVisibility { get; private set; }

        public bool RouletteRtpVisibility { get; private set; }

        public bool MechanicalMeterVisibility { get; private set; }

        protected override void OnLoaded()
        {
            UpdateCurrencyDescription();
            SetupRtpValuesAndVisibility();
            OnPropertyChanged(nameof(MechanicalMeter), nameof(DoorOpticSensor), nameof(ZeroCreditOnOos), nameof(CurrencyDisplayText));
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            UpdateCurrencyDescription();
            SetupRtpValuesAndVisibility();
            OnPropertyChanged(nameof(MechanicalMeter), nameof(DoorOpticSensor), nameof(ZeroCreditOnOos), nameof(CurrencyDisplayText));
            base.OnOperatorCultureChanged(evt);
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    var ticketCreator = ServiceManager.GetInstance()
                        .TryGetService<IJurisdictionSetupInformationTicketCreator>();
                    tickets = ticketCreator.CreateJurisdictionSetupInformationTicket();
                    break;
            }

            return tickets;
        }

        private void SetupRtpValuesAndVisibility()
        {
            var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
            var showAllowedRtp = config.GetSetting(this, ShowAllowedRtpSetting, true);

            var rtpRange = string.Empty;
            SlotRtpVisibility = showAllowedRtp && GetAllowedRtpForGameType(
                GamingConstants.AllowSlotGames,
                GamingConstants.SlotMinimumReturnToPlayer,
                GamingConstants.SlotMaximumReturnToPlayer,
                ref rtpRange);

            AllowedSlotRtp = rtpRange;

            PokerRtpVisibility = showAllowedRtp &&
                GetAllowedRtpForGameType(
                GamingConstants.AllowPokerGames,
                GamingConstants.PokerMinimumReturnToPlayer,
                GamingConstants.PokerMaximumReturnToPlayer,
                ref rtpRange);

            AllowedPokerRtp = rtpRange;

            KenoRtpVisibility = showAllowedRtp &&
                GetAllowedRtpForGameType(
                GamingConstants.AllowKenoGames,
                GamingConstants.KenoMinimumReturnToPlayer,
                GamingConstants.KenoMaximumReturnToPlayer,
                ref rtpRange);

            AllowedKenoRtp = rtpRange;

            BlackjackRtpVisibility = showAllowedRtp &&
                GetAllowedRtpForGameType(
                GamingConstants.AllowBlackjackGames,
                GamingConstants.BlackjackMinimumReturnToPlayer,
                GamingConstants.BlackjackMaximumReturnToPlayer,
                ref rtpRange);

            AllowedBlackjackRtp = rtpRange;

            RouletteRtpVisibility = showAllowedRtp &&
                GetAllowedRtpForGameType(
                GamingConstants.AllowRouletteGames,
                GamingConstants.RouletteMinimumReturnToPlayer,
                GamingConstants.RouletteMaximumReturnToPlayer,
                ref rtpRange);

            AllowedRouletteRtp = rtpRange;
        }

        private bool GetAllowedRtpForGameType(string allowGameTypeKey, string minimumRtpKey, string maximumRtpKey, ref string allowedRtpRange)
        {
            if (PropertiesManager.GetValue(allowGameTypeKey, true))
            {
                var minimumRtp = PropertiesManager.GetValue(minimumRtpKey, _defaultAnyGameMinimum);
                var maximumRtp = PropertiesManager.GetValue(maximumRtpKey, _defaultAnyGameMaximum);

                var rtpRange = new RtpRange(minimumRtp, maximumRtp);

                allowedRtpRange = rtpRange.ToString();

                return true;
            }

            return false;
        }

        private string HardMeterLogicalStateToString(HardMeterLogicalState state)
        {
            switch (state)
            {
                case HardMeterLogicalState.Idle:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledLabel);
                case HardMeterLogicalState.Error:
                    return $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled)} - {Localizer.For(CultureFor.Operator).GetString(Resources.ErrorText)}";
                case HardMeterLogicalState.Disabled:
                case HardMeterLogicalState.Uninitialized:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
            }

            return string.Empty;
        }

        private void UpdateCurrencyDescription()
        {
            var culture = CurrencyExtensions.CurrencyCultureInfo;
            var currencyText = CurrencyExtensions.GetFormattedDescriptionForOperator(culture, Currency.IsoCode);
            CurrencyDisplayText = currencyText;
        }
    }
}
