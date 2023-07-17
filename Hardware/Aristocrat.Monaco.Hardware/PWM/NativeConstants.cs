namespace Aristocrat.Monaco.Hardware.PWM
{
    using System;
    using System.Globalization;
    internal class NativeConstants
    {
        /// <summary> Constants for errors </summary>
        internal const uint ErrorFileNotFound = 2;
        internal const uint ErrorInvalidName = 123;
        internal const uint ErrorAccessDenied = 5;
        internal const uint ErrorIoPending = 997;

        /// <summary> Constants for return value </summary>
        internal const int InvalidHandleValue = -1;

        /// <summary> Constants for dwFlagsAndAttributes </summary>
        internal const uint FileFlagOverlapped = 0x40000000;
        internal const uint FileFlagNoBuffering = 0x20000000;

        /// <summary> Constants for dwCreationDisposition </summary>
        internal const uint OpenExisting = 3;

        /// <summary> Constants for dwDesiredAccess </summary>
        internal const uint GenericRead = 0x80000000;
        internal const uint GenericWrite = 0x40000000;
        internal const uint FileShareRead = 0x00000001;
        internal const uint FileShareWrite = 0x00000002;

        /// <summary> Constants for WaitForSingleObject's return value </summary>
        internal const uint StatusWait0 = 0x0;
        internal const uint WaitObject0 = StatusWait0 + 0;
        internal const uint WaitTimeout = 258;

        /// <summary> Constants for IOCT call to identify devices </summary>
        internal const uint CoinAcceptorDeviceType = 0x8100;
        internal const uint NVCoinAcceptorDeviceType = 0x8200;
        internal const uint HopperDeviceType = 0x8300;



        internal const uint MethodBuffered = 0;
        internal const uint FileAnyAccess = 0;

        /// <summary> Configuration Manager CONFIGRET return status codes </summary>
        internal const int CrSuccess = 0x0;

        /// <summary>Flags for CM_Get_Device_Interface_List, CM_Get_Device_Interface_List_Size </summary>
        internal const uint CmGetDeviceInterfaceListPresent = 0x00000000; // only currently 'live' device interfaces
        internal const uint CmGetDeviceInterfaceListAllDevices = 0x00000001; // all registered device interfaces, live or not
        internal uint CmGetDeviceInterfaceListBits = 0x00000001;

        /// <summary>Function that creates IOCTL code for the TPCI940.</summary>
        /// <param name="command">Function Creates IOCTL code for the TPCI940</param>
        /// <returns>I/O control code (IOCTL)</returns>
        public static uint COINACCEPTOR_MAKE_IOCTL(uint DeviceType, CoinAcceptorCommands command)
        {
            return CTL_CODE(DeviceType, 0x800 + (uint)command, MethodBuffered, FileAnyAccess);
        }

        public static uint PWMDEVICE_MAKE_IOCTL<C>(uint DeviceType, C command)
        {
            var value = (uint)((IConvertible)command).ToInt32(CultureInfo.InvariantCulture.NumberFormat);
            return CTL_CODE(DeviceType, 0x800 + value, MethodBuffered, FileAnyAccess);
        }


        /// <summary>Function used to create a unique system I/O control code (IOCTL).</summary>
        /// <param name="deviceType">Defines the type of device for the given IOCTL</param>
        /// <param name="function">Defines an action within the device category</param>
        /// <param name="method">Defines the method codes for how buffers are passed for I/O and file system controls</param>
        /// <param name="access">Defines the access check value for any access</param>
        /// <returns>I/O control code (IOCTL)</returns>
        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return (deviceType << 16) | (access << 14) | (function << 2) | method;
        }

        /// <summary>Function that creates IOCTL code for the TPCI940.</summary>
        /// <param name="command">Function Creates IOCTL code for the TPCI940</param>
        /// <returns>I/O control code (IOCTL)</returns>


    }
}
