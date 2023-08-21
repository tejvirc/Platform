namespace Aristocrat.Monaco.G2S.Descriptors
{
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.SharedDevice;

    public abstract class DeviceDescriptorBase
    {
        private const int MaxLength = 32;
        private const string GdsSeparator = ",";

        protected Descriptor GetDescriptor(IDeviceAdapter device)
        {
            return new Descriptor(
                device.VendorId.ToString("X"),
                device.ProductId.ToString("X"),
                Truncate(
                    $"{device.DeviceConfiguration.FirmwareId}{GdsSeparator}{device.DeviceConfiguration.FirmwareRevision}"),
                Truncate(device.DeviceConfiguration.Manufacturer),
                Truncate(device.DeviceConfiguration.Model),
                Truncate(device.DeviceConfiguration.SerialNumber));
        }

        private static string Truncate(string source)
        {
            return source != null && source.Length > MaxLength ? source.Substring(0, MaxLength) : source;
        }
    }
}