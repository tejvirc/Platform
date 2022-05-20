namespace Aristocrat.Monaco.Application.Helpers
{
    using System;
    using System.Text;
    using Aristocrat.Monaco.Hardware.Contracts.SerialPorts;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public static class DeviceHelper
    {
        private static readonly ISerialPortsService SerialPortsService = ServiceManager.GetInstance().TryGetService<ISerialPortsService>();

        public static string GetDeviceStatus(this IDevice device, bool showPort = true)
        {
            return device != null
                ? GetDeviceStatus(
                    device.Manufacturer,
                    device.Model,
                    device.Protocol,
                    showPort ? device.PortName : null,
                    device.FirmwareId,
                    device.FirmwareRevision,
                    device.VariantName,
                    device.VariantVersion)
                : string.Empty;
        }

        public static string GetDeviceStatus(
            string manufacturer,
            string model,
            string protocol,
            string port,
            string firmwareVersion,
            string firmwareRevision,
            string variantName,
            string variantVersion)
        {
            return string.IsNullOrEmpty(manufacturer)
                ? string.Empty
                : manufacturer.Contains(ApplicationConstants.Fake)
                    ? port == null
                        ? manufacturer
                        : $"{manufacturer} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedText)}"
                    : BuildDeviceStatusString(
                        manufacturer,
                        model,
                        protocol,
                        port,
                        firmwareVersion,
                        firmwareRevision,
                        variantName,
                        variantVersion);
        }

        private static string BuildDeviceStatusString(
            string manufacturer,
            string model,
            string protocol,
            string port,
            string firmwareVersion,
            string firmwareRevision,
            string variantName,
            string variantVersion)
        {
            var sb = new StringBuilder()
                .AppendWithSpace(manufacturer)
                .AppendWithSpace(model)
                .AppendWithSpace(protocol)
                .AppendWithSpace(firmwareVersion)
                .AppendWithSpace(firmwareRevision)
                .AppendWithSpace(variantName)
                .AppendWithSpace(variantVersion);

            if (sb.Length > 0 && !string.IsNullOrWhiteSpace(port))
            {
                sb.Append($" {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedToText)} {SerialPortsService.PhysicalToLogicalName(port)}");
            }

            return sb.ToString();
        }
    }
}