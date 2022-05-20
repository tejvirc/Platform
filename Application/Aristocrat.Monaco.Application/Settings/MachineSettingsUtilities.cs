namespace Aristocrat.Monaco.Application.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cabinet.Contracts;
    using Common;
    using Contracts.Localization;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>Definition of the ButtonDeckUtilities class</summary>
    [CLSCompliant(false)]
    public static class MachineSettingsUtilities
    {
        private const string MultipleItemDelimiter = " | ";

        public static string GetButtonDeckIdentification(ILocalizer localizer)
        {
            var buttonDeckInfo = string.Empty;
            var cabinetDetectionService = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();
            var displayDeviceVbd = cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD);
            var touchScreen = cabinetDetectionService.ExpectedTouchDevices.FirstOrDefault(
                x => x.ProductId == displayDeviceVbd?.TouchProductId && x.VendorId == displayDeviceVbd.TouchVendorId);
            if (displayDeviceVbd != null)
            {
                if (!string.IsNullOrEmpty(displayDeviceVbd.Name) &&
                    displayDeviceVbd.Name.Equals(VbdType.Bartop.GetDescription(typeof(VbdType))))
                {
                    buttonDeckInfo = localizer.GetString(ResourceKeys.Bartop) + " ";
                }
            }

            var buttonDeckType = ButtonDeckUtilities.GetButtonDeckType();
            var buttonDeckFirmwareVersion = string.Empty;
            if (buttonDeckType == ButtonDeckUtilities.ButtonDeckType.LCD)
            {
                buttonDeckFirmwareVersion = " " + ButtonDeckUtilities.GetButtonDeckFirmwareVersion();
            }

            var buttonDeckVbd = string.Empty;

            if (touchScreen != null)
            {
                buttonDeckVbd = " " + touchScreen.ProductString + " " +
                                cabinetDetectionService.GetFirmwareVersion(touchScreen);
            }

            switch (buttonDeckType)
            {
                case ButtonDeckUtilities.ButtonDeckType.NoButtonDeck:
                    buttonDeckInfo += localizer.GetString(ResourceKeys.None);
                    break;
                case ButtonDeckUtilities.ButtonDeckType.PhysicalButtonDeck:
                    buttonDeckInfo += localizer.GetString(ResourceKeys.MechanicalButtons);
                    break;
                case ButtonDeckUtilities.ButtonDeckType.LCD:
                case ButtonDeckUtilities.ButtonDeckType.SimulatedLCD:
                    buttonDeckInfo += localizer.GetString(ResourceKeys.LCDButtons) + buttonDeckFirmwareVersion;
                    break;
                case ButtonDeckUtilities.ButtonDeckType.Virtual:
                case ButtonDeckUtilities.ButtonDeckType.SimulatedVirtual:
                    buttonDeckInfo += localizer.GetString(ResourceKeys.VirtualButtons) + buttonDeckVbd;
                    break;
                default:
                    buttonDeckInfo += localizer.GetString(ResourceKeys.NotAvailable);
                    break;
            }

            return buttonDeckInfo;
        }

        public static string GetTouchScreenIdentification(ILocalizer localizer)
        {
            var cabinetDetectionService = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();

            var touchScreenIdentification = string.Empty;
            if (cabinetDetectionService.ExpectedSerialTouchDevices != null)
            {
                var serialTouchScreensInfo = cabinetDetectionService.ExpectedSerialTouchDevices
                    .Select(x => $"{x.Name} {x.VersionNumber}")
                    .DefaultIfEmpty(localizer.GetString(ResourceKeys.None));
                touchScreenIdentification = string.Join(MultipleItemDelimiter, serialTouchScreensInfo);
            }

            var touchScreensInfo = cabinetDetectionService.ExpectedTouchDevices
                .Select(x => $"{x.ProductString} {cabinetDetectionService.GetFirmwareVersion(x)}")
                .DefaultIfEmpty(localizer.GetString(ResourceKeys.None));

            return touchScreenIdentification + string.Join(MultipleItemDelimiter, touchScreensInfo);
        }

        public static IEnumerable<string> GetTouchScreenIdentificationsWithoutVbd(ILocalizer localizer)
        {
            var cabinetDetectionService = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();
            var displayDeviceVbd = cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD);

            IEnumerable<string> touchScreenIdentificationsWithoutVbd = null;
            if (cabinetDetectionService.ExpectedSerialTouchDevices != null)
            {
                touchScreenIdentificationsWithoutVbd = cabinetDetectionService.ExpectedSerialTouchDevices
                    .Where(x => x.ProductId != displayDeviceVbd?.TouchProductId || x.VendorId != displayDeviceVbd.TouchVendorId)
                    .Select(x => $"{x.Name} {x.VersionNumber}")
                    .DefaultIfEmpty(localizer.GetString(ResourceKeys.None));
            }

            if (touchScreenIdentificationsWithoutVbd == null && cabinetDetectionService.ExpectedTouchDevices != null)
            {
                touchScreenIdentificationsWithoutVbd = cabinetDetectionService.ExpectedTouchDevices
                    .Where(x => x.ProductId != displayDeviceVbd?.TouchProductId || x.VendorId != displayDeviceVbd.TouchVendorId)
                    .Select(x => $"{x.ProductString} {cabinetDetectionService.GetFirmwareVersion(x)}")
                    .DefaultIfEmpty(localizer.GetString(ResourceKeys.None));
            }

            return touchScreenIdentificationsWithoutVbd;
        }

        public static string GetTouchScreenIdentificationWithoutVbd(ILocalizer localizer)
        {
            var touchscreensInfo = GetTouchScreenIdentificationsWithoutVbd(localizer);
            return string.Join(MultipleItemDelimiter, touchscreensInfo);
        }

        public static string GetLightingIdentification(ILocalizer localizer)
        {
            var lightingDevices =
                ServiceManager.GetInstance().TryGetService<IEdgeLightingController>()?.Devices
                    .Select(x => $"{x.DeviceType} {(x.DeviceType == ElDeviceType.None ? "" : x.Version.FormatHidVersion())}").ToList() ?? new List<string>();
            return lightingDevices.Any()
                ? string.Join(MultipleItemDelimiter, lightingDevices)
                : localizer.GetString(ResourceKeys.None);
        }

        private static string FormatHidVersion(this int version)
        {
            return $"{(version >> 8) & 0xFF:X}.{version & 0xFF:X2}";
        }
    }
}