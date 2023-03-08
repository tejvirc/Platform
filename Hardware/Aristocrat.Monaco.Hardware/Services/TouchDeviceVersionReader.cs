namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Cabinet.Contracts;
    using log4net;
    using Vgt.Client12.Hardware.HidLibrary;

    public static class TouchDeviceVersionReader
    {
        private const string Kortek = "kortek";
        private const string Tovis = "nanots touch controller";
        private const int TovisVendorId = 0x126C;
        private const string VendorInterfaceName = "&mi_01#";

        public static string FirmwareVersion(this ITouchDevice touchDevice)
        {
            string versionStr = null;
            try
            {
                if (touchDevice.ProductString.ToLower().Contains(Kortek) ||
                    (touchDevice.ProductString.ToLower().Contains(Tovis) && touchDevice.VendorId == TovisVendorId))
                {
                    versionStr = ReadKortekVersion(touchDevice);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return versionStr ?? touchDevice.VersionNumber.FormatHidVersion();
        }

        private static string ReadKortekVersion(ITouchDevice touchDevice)
        {
            return HidDevices.Enumerate(touchDevice.VendorId, touchDevice.ProductId)
                .Where(x => x.DevicePath.ToLower().Contains(VendorInterfaceName)).Select(GetVersion)
                .FirstOrDefault(x => x != null);

            string GetVersion(HidDevice device)
            {
                const byte reportVendor = 0x76;
                const byte vendorMagic = 0x77;
                const byte vendorCmdGetFirmwareVersion = 0x78;
                const int touchVersionAddressMaster = 0x023C;
                string versionStr = null;
                device.OpenDevice(
                    DeviceMode.Overlapped,
                    DeviceMode.Overlapped,
                    ShareMode.ShareRead | ShareMode.ShareWrite);
                var data = new byte[64];
                data[0] = reportVendor;
                data[1] = vendorMagic;
                data[2] = vendorCmdGetFirmwareVersion;
                data[3] = touchVersionAddressMaster & 0xFF;
                data[4] = (touchVersionAddressMaster >> 8) & 0xFF;
                data[5] = 4;
                if (device.Write(data, 1000))
                {
                    var version = device.Read(1000);
                    if (version.Status == HidDeviceData.ReadStatus.Success)
                    {
                        data = version.Data;
                        versionStr = $"{data[8] | (data[9] << 8):D}.{data[10] | (data[11] << 8):D2}";
                    }
                }

                device.CloseDevice();
                device.Dispose();
                return versionStr;
            }
        }

        private static string FormatHidVersion(this int version)
        {
            return $"{(version >> 8) & 0xFF:X}.{version & 0xFF:X2}";
        }
    }
}