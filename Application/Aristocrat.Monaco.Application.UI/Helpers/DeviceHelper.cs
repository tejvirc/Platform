namespace Aristocrat.Monaco.Application.UI.Helpers
{
    using System;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Contracts;
    using Hardware.Contracts.SharedDevice;
    using Localization;
    using ViewModels;

    [CLSCompliant(false)]
    public static class DeviceHelper
    {
        // returns the numeric portion of a com port ex: 2 from COM2
        public static int ToPortNumber(this string portName)
        {
            if (!string.IsNullOrEmpty(portName) && portName.StartsWith(ApplicationConstants.SerialPortPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(portName.Substring(3), out var portNumber))
                {
                    return portNumber;
                }
            }

            return 0; // If USB or something wrong with COM port string.
        }

        public static string ToPortString(this int portNumber, DeviceType device)
        {
            return portNumber == 0 ? ApplicationConstants.USB : ApplicationConstants.SerialPortPrefix + portNumber;
        }

        public static string GetEnabledPropertyName(this DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.Printer:
                    return ApplicationConstants.PrinterEnabled;
                case DeviceType.NoteAcceptor:
                    return ApplicationConstants.NoteAcceptorEnabled;
                case DeviceType.IdReader:
                    return ApplicationConstants.IdReaderEnabled;
                case DeviceType.ReelController:
                    return ApplicationConstants.ReelControllerEnabled;
                case DeviceType.CoinAcceptor:
                    return HardwareConstants.CoinAcceptorEnabledKey;
                default:
                    return string.Empty;
            }
        }

        public static string GetDeviceStatus(this DeviceConfigViewModel config)
        {
            return config != null
                ? Application.Helpers.DeviceHelper.GetDeviceStatus(
                    config.Manufacturer,
                    string.Empty,
                    config.Protocol,
                    config.Port,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                : string.Empty;
        }
        public static DeviceState GetDeviceStatusType(this DeviceConfigViewModel config)
        {
            return config != null
                ? Application.Helpers.DeviceHelper.GetDeviceStatusType(
                    config.Manufacturer,
                    string.Empty,
                    config.Protocol,
                    config.Port,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                : DeviceState.None;
        }

        public static DeviceState GetDeviceEnum(this DeviceConfigViewModel config)
        {
            return DeviceState.None;
        }
    }
}