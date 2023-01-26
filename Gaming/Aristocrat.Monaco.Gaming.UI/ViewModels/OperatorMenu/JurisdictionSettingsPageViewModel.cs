namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
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
        /// <summary>
        ///     Initializes a new instance of the <see cref="JurisdictionSettingsPageViewModel" /> class.
        /// </summary>
        public JurisdictionSettingsPageViewModel()
        {
            Jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

            Currency = CurrencyExtensions.DescriptionWithMinorSymbol;

            _defaultAnyGameMinimum = PropertiesManager.GetValue(GamingConstants.AnyGameMinimumReturnToPlayer, decimal.MinValue);
            _defaultAnyGameMaximum = PropertiesManager.GetValue(GamingConstants.AnyGameMaximumReturnToPlayer, decimal.MaxValue);

            SetupRtpValuesAndVisibility();

            var hardMeter = ServiceManager.GetInstance().TryGetService<IHardMeter>();

            MechanicalMeter = HardMeterLogicalStateToString(hardMeter?.LogicalState ?? HardMeterLogicalState.Disabled);

            MechanicalMeterVisibility = PropertiesManager.GetValue(ApplicationConstants.ConfigWizardHardMetersConfigVisible, true);

            DoorOpticSensor = (bool)PropertiesManager.GetProperty(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false) ? "Enabled" : "Disabled";

            ZeroCreditOnOos = !PropertiesManager.GetValue(
                ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                false)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText);
        }

        public string Jurisdiction { get; }

        public string Currency { get; }

        public string AllowedSlotRtp { get; private set; }

        public string AllowedPokerRtp { get; private set; }

        public string AllowedKenoRtp { get; private set; }

        public string AllowedBlackjackRtp { get; private set; }

        public string AllowedRouletteRtp { get; private set; }

        public string MechanicalMeter { get; }

        public string DoorOpticSensor { get; }

        public string ZeroCreditOnOos { get; }

        public bool SlotRtpVisibility { get; private set; }

        public bool PokerRtpVisibility { get; private set; }

        public bool KenoRtpVisibility { get; private set; }

        public bool BlackjackRtpVisibility { get; private set; }

        public bool RouletteRtpVisibility { get; private set; }

        public bool MechanicalMeterVisibility { get; private set; }

        private readonly decimal _defaultAnyGameMinimum;

        private readonly decimal _defaultAnyGameMaximum;

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    var ticketCreator = ServiceManager.GetInstance()
                        .TryGetService<IJurisdictionSetupInformationTicketCreator>();
                    tickets =  ticketCreator.CreateJurisdictionSetupInformationTicket();
                    break;
            }

            return tickets;
        }

        private void SetupRtpValuesAndVisibility()
        {
            // TODO: Put check in to include progressive RTP if progressive increment RTP is allowed.

            var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
            var showAllowedRtp = config.GetSetting(this, ShowAllowedRtpSetting, true);

            string rtpRange = string.Empty;
            SlotRtpVisibility = showAllowedRtp &&
                GetAllowedRtpForGameType(
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
            // TODO : Construct keys from game type, eg GamingConstants.GetAllowedGamesKey(GameType forGameType); then, loop through each game type for the sent-in values.

            if (PropertiesManager.GetValue(allowGameTypeKey, true))
            {
                allowedRtpRange = new RtpRange(
                    PropertiesManager.GetValue(minimumRtpKey, _defaultAnyGameMinimum),
                    PropertiesManager.GetValue(maximumRtpKey, _defaultAnyGameMaximum))
                    .ToString();
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
    }
}
