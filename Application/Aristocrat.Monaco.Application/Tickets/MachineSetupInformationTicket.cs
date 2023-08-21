namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using Contracts;
    using Contracts.ConfigWizard;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Quartz.Util;
    using Settings;

    public class MachineSetupInformationTicket : AuditTicket
    {
        private const string HardBootTimeKey = "System.HardBoot.Time";
        private const string SoftBootTimeKey = "System.SoftBoot.Time";

        public MachineSetupInformationTicket(string titleOverride = null)
            : base(titleOverride)
        {
            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.MachineSetupInformation));
            }
        }

        public override void AddTicketContent()
        {
            // Identification->Machine
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageAreaOverride))
            {
                AddLabeledLine(ResourceKeys.AreaLabel, PropertiesManager.GetValue(ApplicationConstants.Area, (string)null));
            }
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
            {
                AddLabeledLine(ResourceKeys.ZoneText, PropertiesManager.GetValue(ApplicationConstants.Zone, (string)null));
            }
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageBankOverride))
            {
                AddLabeledLine(ResourceKeys.BankText, PropertiesManager.GetValue(ApplicationConstants.Bank, (string)null));
            }
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPagePositionOverride))
            {
                AddLabeledLine(ResourceKeys.PositionText, PropertiesManager.GetValue(ApplicationConstants.Position, (string)null));
            }
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageLocationOverride))
            {
                AddLabeledLine(ResourceKeys.Location, PropertiesManager.GetValue(ApplicationConstants.Location, (string)null));
            }

            var zone = PropertiesManager.GetValue<TimeZoneInfo>(ApplicationConstants.TimeZoneKey, null);
            var hardBootTime = PropertiesManager.GetValue(HardBootTimeKey, DateTime.UtcNow);
            AddLabeledLine(
                ResourceKeys.HardBootTimeLabel,
                Time.GetLocationTime(hardBootTime, zone).ToString(DateTimeFormat));

            var softBootTime = PropertiesManager.GetValue(SoftBootTimeKey, DateTime.UtcNow);
            AddLabeledLine(
                ResourceKeys.SoftBootTimeLabel,
                Time.GetLocationTime(softBootTime, zone).ToString(DateTimeFormat));

            AddLabeledLine(ResourceKeys.IPAddressesLabel, NetworkInterfaceInfo.DefaultIpAddress?.ToString());

            var ioService = ServiceManager.GetService<IIO>();
            AddLabeledLine(ResourceKeys.Electronics, ioService.GetElectronics());

            AddLabeledLine(
                ResourceKeys.GraphicsCard,
                ServiceManager.GetService<IDisplayService>().GraphicsCard);

            AddLabeledLine(ResourceKeys.ButtonDeck, MachineSettingsUtilities.GetButtonDeckIdentification(TicketLocalizer));

            AddLabeledLine(ResourceKeys.TouchScreen, MachineSettingsUtilities.GetTouchScreenIdentification(TicketLocalizer));

            AddLabeledLine(ResourceKeys.LightingLabel, MachineSettingsUtilities.GetLightingIdentification(TicketLocalizer));

            AddLabeledLine(
                ResourceKeys.NoteAcceptorLabel,
                ServiceManager.TryGetService<INoteAcceptor>()?.DeviceConfiguration.GetDeviceStatus(false)
                    .TrimEmptyToNull() ?? TicketLocalizer.GetString(ResourceKeys.NotAvailable));

            AddLabeledLine(
                ResourceKeys.PrinterLabel,
                ServiceManager.TryGetService<IPrinter>()?.DeviceConfiguration.GetDeviceStatus(false)
                    .TrimEmptyToNull() ?? TicketLocalizer.GetString(ResourceKeys.NotAvailable));

            AddLabeledLine(ResourceKeys.ManufacturerLabel, ioService.DeviceConfiguration.Manufacturer);
            AddLabeledLine(ResourceKeys.ModelLabel, ioService.DeviceConfiguration.Model);

            AddLabeledLine(
                ResourceKeys.BiosVersion,
                ioService.GetFirmwareVersion(FirmwareData.Bios).TrimEmptyToNull() ?? TicketLocalizer.GetString(ResourceKeys.NotAvailable));
            AddLabeledLine(
                ResourceKeys.FpgaVersion,
                ioService.GetFirmwareVersion(FirmwareData.Fpga).TrimEmptyToNull() ?? TicketLocalizer.GetString(ResourceKeys.NotAvailable));

            var osService = ServiceManager.TryGetService<IOSService>();
            AddLabeledLine(ResourceKeys.WindowsVersionLabel, Environment.OSVersion.Version.ToString());
            AddLabeledLine(ResourceKeys.OSImageVersionLabel, osService?.OsImageVersion.ToString());
            AddLabeledLine(
                ResourceKeys.PlatformVersionLabel,
                PropertiesManager.GetValue(KernelConstants.SystemVersion, string.Empty));
        }
    }
}