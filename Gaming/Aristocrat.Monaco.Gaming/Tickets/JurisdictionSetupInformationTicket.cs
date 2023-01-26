namespace Aristocrat.Monaco.Gaming.Tickets
{
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Application.Tickets;
    using Contracts;
    using Contracts.Rtp;
    using Hardware.Contracts.HardMeter;
    using Kernel;
    using Localization.Properties;

    public class JurisdictionSetupInformationTicket : AuditTicket
    {
        private const string ShowAllowedRtpSetting = "ShowAllowedRTP";
        private readonly int _defaultAnyGameMinimum;
        private readonly int _defaultAnyGameMaximum;

        public JurisdictionSetupInformationTicket(string titleOverride = null)
            : base(titleOverride)
        {
            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.JurisdictionSetupInformation));
            }

            _defaultAnyGameMinimum = PropertiesManager.GetValue(
                GamingConstants.AnyGameMinimumReturnToPlayer,
                int.MinValue);
            _defaultAnyGameMaximum = PropertiesManager.GetValue(
                GamingConstants.AnyGameMaximumReturnToPlayer,
                int.MaxValue);
        }

        public override void AddTicketContent()
        {
            AddLabeledLine(
                ResourceKeys.Currency,
                PropertiesManager.GetValue(ApplicationConstants.CurrencyId, string.Empty));

            var config = ServiceManager.GetService<IOperatorMenuConfiguration>();
            var showAllowedRtp = config.GetSetting(ShowAllowedRtpSetting, true);

            var rtpRange = string.Empty;
            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowSlotGames,
                    GamingConstants.SlotMinimumReturnToPlayer,
                    GamingConstants.SlotMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedSlotRtp, rtpRange);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowPokerGames,
                    GamingConstants.PokerMinimumReturnToPlayer,
                    GamingConstants.PokerMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedPokerRtp, rtpRange);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowKenoGames,
                    GamingConstants.KenoMinimumReturnToPlayer,
                    GamingConstants.KenoMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedKenoRtp, rtpRange);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowBlackjackGames,
                    GamingConstants.BlackjackMinimumReturnToPlayer,
                    GamingConstants.BlackjackMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedBlackjackRtp, rtpRange);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                GamingConstants.AllowRouletteGames,
                GamingConstants.RouletteMinimumReturnToPlayer,
                GamingConstants.RouletteMaximumReturnToPlayer,
                ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedRouletteRtp, rtpRange);
            }

            AddLabeledLine(
                ResourceKeys.MechanicalMeter,
                HardMeterLogicalStateToString(
                    ServiceManager.TryGetService<IHardMeter>()?.LogicalState ??
                    HardMeterLogicalState.Disabled));

            AddLabeledLine(
                ResourceKeys.DoorOpticSensor,
                (bool)PropertiesManager.GetProperty(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false)
                    ? TicketLocalizer.GetString(ResourceKeys.Enabled)
                    : TicketLocalizer.GetString(ResourceKeys.Disabled));

            AddLabeledLine(
                ResourceKeys.ZeroCredit,
                !PropertiesManager.GetValue(
                    ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                    false)
                    ? TicketLocalizer.GetString(ResourceKeys.OnText)
                    : TicketLocalizer.GetString(ResourceKeys.OffText));
        }

        private bool GetAllowedRtpForGameType(
            string allowGameTypeKey,
            string minimumRtpKey,
            string maximumRtpKey,
            ref string allowedRtpRange)
        {
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
                    return TicketLocalizer.GetString(ResourceKeys.EnabledLabel);
                case HardMeterLogicalState.Error:
                    return
                        $"{TicketLocalizer.GetString(ResourceKeys.Disabled)} - {TicketLocalizer.GetString(Resources.ErrorText)}";
                case HardMeterLogicalState.Disabled:
                case HardMeterLogicalState.Uninitialized:
                    return TicketLocalizer.GetString(ResourceKeys.Disabled);
                default:
                    return string.Empty;
            }
        }
    }
}