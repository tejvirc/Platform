namespace Aristocrat.Monaco.Hardware.Services
{
    using Cabinet.Contracts;

    public static class TouchDeviceVersionReader
    {
        public static string FirmwareVersion(this ITouchDevice touchDevice)
        {
            string result = null;
            if (!string.IsNullOrEmpty(touchDevice.ReadableVersionNumber))
            {
                result = touchDevice.ReadableVersionNumber;
            }
            else
            {
                result = touchDevice.VersionNumber.FormatHidVersion();
            }
            return result;
        }

        private static string FormatHidVersion(this int version)
        {
            return $"{(version >> 8) & 0xFF:X}.{version & 0xFF:X2}";
        }
    }
}