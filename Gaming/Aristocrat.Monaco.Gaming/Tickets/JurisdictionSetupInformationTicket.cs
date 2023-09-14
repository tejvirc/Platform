namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System.Linq;
    using Contracts;
    using Application.Tickets;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Extensions;
    using Kernel;
    using Kernel.Contracts;
    using Hardware.Contracts.HardMeter;
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
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.JurisdictionSetupInformation).ToUpper(TicketLocalizer.CurrentCulture));
            }

            _defaultAnyGameMinimum = PropertiesManager.GetValue(
                GamingConstants.AnyGameMinimumReturnToPlayer,
                int.MinValue);
            _defaultAnyGameMaximum = PropertiesManager.GetValue(
                GamingConstants.AnyGameMaximumReturnToPlayer,
                int.MaxValue);
        }

        public override void AddTicketHeader()
        {
            AddEmptyLines();
            AddLabeledLine(ResourceKeys.DateAndTime, Time.GetLocationTime().ToString(DateTimeFormat));

            var serialNumber =
                PropertiesManager.GetValue(ApplicationConstants.SerialNumber, TicketLocalizer.GetString(ResourceKeys.DataUnavailable));
            AddLabeledLine(ResourceKeys.SerialNumber, serialNumber);

            var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
            var assetNumber = string.Empty;
            if (machineId != 0)
            {
                assetNumber = machineId.ToString();
            }

            AddLabeledLine(ResourceKeys.AssetNumber, assetNumber);

            AddLabeledLine(
                ResourceKeys.MacAddressLabel,
                string.Join(":", Enumerable.Range(0, 6).Select(i => NetworkInterfaceInfo.DefaultPhysicalAddress.Substring(i * 2, 2))));

            var propertyName = (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
            if (propertyName.Length > MaxPrintPropertyNameLength)
            {
                AddLabeledLine(ResourceKeys.PropertyName, propertyName.Substring(0, MaxPrintPropertyNameLength));
                AddLabeledLine(string.Empty, propertyName.Substring(MaxPrintPropertyNameLength).PadRight(MaxPrintPropertyNameLength, ' '), false);
            }
            else
            {
                AddLabeledLine(ResourceKeys.PropertyName, propertyName);
            }

            AddDashesLine();
        }

        public override void AddTicketContent()
        {
            AddLabeledLine(
                ResourceKeys.JurisdictionLabel,
                PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty));

            var currencyDisplayString =
                CurrencyExtensions.CurrencyDisplayCulture.GetFormattedCurrencyDescription(CurrencyExtensions.Currency.IsoCode);
            AddLabeledLine(ResourceKeys.Currency, currencyDisplayString);

            var config = ServiceManager.GetService<IOperatorMenuConfiguration>();
            var showAllowedRtp = config.GetSetting(ShowAllowedRtpSetting, true);

            var rtpRange = string.Empty;
            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowSlotGames,
                    GamingConstants.SlotMinimumReturnToPlayer,
                    GamingConstants.SlotMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedSlotRtp, rtpRange, reformatLabelFirst: true);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowPokerGames,
                    GamingConstants.PokerMinimumReturnToPlayer,
                    GamingConstants.PokerMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedPokerRtp, rtpRange, reformatLabelFirst: true);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowKenoGames,
                    GamingConstants.KenoMinimumReturnToPlayer,
                    GamingConstants.KenoMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedKenoRtp, rtpRange, reformatLabelFirst: true);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                    GamingConstants.AllowBlackjackGames,
                    GamingConstants.BlackjackMinimumReturnToPlayer,
                    GamingConstants.BlackjackMaximumReturnToPlayer,
                    ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedBlackjackRtp, rtpRange, reformatLabelFirst: true);
            }

            if (showAllowedRtp && GetAllowedRtpForGameType(
                GamingConstants.AllowRouletteGames,
                GamingConstants.RouletteMinimumReturnToPlayer,
                GamingConstants.RouletteMaximumReturnToPlayer,
                ref rtpRange))
            {
                AddLabeledLine(ResourceKeys.AllowedRouletteRtp, rtpRange, reformatLabelFirst: true);
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
                    : TicketLocalizer.GetString(ResourceKeys.OffText),
                reformatLabelFirst: true);
        }

        private bool GetAllowedRtpForGameType(
            string allowGameTypeKey,
            string minimumRtpKey,
            string maximumRtpKey,
            ref string allowedRtpRange)
        {
            if (PropertiesManager.GetValue(allowGameTypeKey, true))
            {
                allowedRtpRange = GameConfigHelper.GetRtpRangeString(
                    PropertiesManager.GetValue(minimumRtpKey, _defaultAnyGameMinimum),
                    PropertiesManager.GetValue(maximumRtpKey, _defaultAnyGameMaximum));

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