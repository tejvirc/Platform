﻿namespace Vgt.Client12.Hardware.HidLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class HidDevices
    {
        private static Guid _hidClassGuid = Guid.Empty;

        private static Guid HidClassGuid
        {
            get
            {
                if (_hidClassGuid.Equals(Guid.Empty))
                {
                    NativeMethods.HidD_GetHidGuid(ref _hidClassGuid);
                }
                return _hidClassGuid;
            }
        }

        public static bool IsConnected(string devicePath)
        {
            return EnumerateDevices().Any(x => x.Path == devicePath);
        }

        public static HidDevice GetDevice(string devicePath)
        {
            return Enumerate(devicePath).FirstOrDefault();
        }

        public static IEnumerable<HidDevice> Enumerate()
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(string devicePath)
        {
            return EnumerateDevices().Where(x => x.Path == devicePath)
                .Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId, params int[] productIds)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(
                x => x.Attributes.VendorId == vendorId &&
                     productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId, int productId, ushort usagePage)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(
                x => x.Attributes.VendorId == vendorId &&
                     productId == (ushort)x.Attributes.ProductId && (ushort)x.Capabilities.UsagePage == usagePage);
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description))
                .Where(x => x.Attributes.VendorId == vendorId);
        }

        internal static IEnumerable<DeviceInfo> EnumerateDevices()
        {
            var devices = new List<DeviceInfo>();
            var hidClass = HidClassGuid;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(
                ref hidClass,
                null,
                0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                var deviceInfoData = CreateDeviceInfoData();
                var deviceIndex = 0;

                while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref deviceInfoData))
                {
                    deviceIndex += 1;

                    var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                    var deviceInterfaceIndex = 0;

                    while (NativeMethods.SetupDiEnumDeviceInterfaces(
                        deviceInfoSet,
                        ref deviceInfoData,
                        ref hidClass,
                        deviceInterfaceIndex,
                        ref deviceInterfaceData))
                    {
                        deviceInterfaceIndex++;
                        var devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData);
                        var description = GetBusReportedDeviceDescription(deviceInfoSet, ref deviceInfoData) ??
                                          GetDeviceDescription(deviceInfoSet, ref deviceInfoData);
                        devices.Add(new DeviceInfo { Path = devicePath, Description = description });
                    }
                }

                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return devices;
        }

        public static int GetVersion(int vendorId, params int[] productIds)
        {
            var enumeratedDevice = EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(
                x => x.Attributes.VendorId == vendorId &&
                     productIds.Contains(x.Attributes.ProductId));
            return enumeratedDevice.Select(x => x.Attributes.Version).FirstOrDefault();
        }

        private static NativeMethods.SP_DEVINFO_DATA CreateDeviceInfoData()
        {
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();

            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            deviceInfoData.DevInst = 0;
            deviceInfoData.ClassGuid = Guid.Empty;
            deviceInfoData.Reserved = IntPtr.Zero;

            return deviceInfoData;
        }

        private static string GetDevicePath(
            IntPtr deviceInfoSet,
            NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var bufferSize = 0;
            var interfaceDetail =
                new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA
                {
                    Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8
                };

            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(
                deviceInfoSet,
                ref deviceInterfaceData,
                IntPtr.Zero,
                0,
                ref bufferSize,
                IntPtr.Zero);

            return NativeMethods.SetupDiGetDeviceInterfaceDetail(
                deviceInfoSet,
                ref deviceInterfaceData,
                ref interfaceDetail,
                bufferSize,
                ref bufferSize,
                IntPtr.Zero)
                ? interfaceDetail.DevicePath
                : null;
        }

        private static string GetDeviceDescription(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devInfoData)
        {
            var descriptionBuffer = new byte[1024];

            var requiredSize = 0;
            var type = 0;

            NativeMethods.SetupDiGetDeviceRegistryProperty(
                deviceInfoSet,
                ref devInfoData,
                NativeMethods.SPDRP_DEVICEDESC,
                ref type,
                descriptionBuffer,
                descriptionBuffer.Length,
                ref requiredSize);

            return descriptionBuffer.ToUTF8String();
        }

        private static string GetBusReportedDeviceDescription(
            IntPtr deviceInfoSet,
            ref NativeMethods.SP_DEVINFO_DATA devInfoData)
        {
            var descriptionBuffer = new byte[1024];

            if (Environment.OSVersion.Version.Major > 5)
            {
                ulong propertyType = 0;
                var requiredSize = 0;

                var @continue = NativeMethods.SetupDiGetDeviceProperty(
                    deviceInfoSet,
                    ref devInfoData,
                    ref NativeMethods.DEVPKEY_Device_BusReportedDeviceDesc,
                    ref propertyType,
                    descriptionBuffer,
                    descriptionBuffer.Length,
                    ref requiredSize,
                    0);

                if (@continue)
                {
                    return descriptionBuffer.ToUTF16String();
                }
            }

            return null;
        }

        internal class DeviceInfo
        {
            public string Path { get; set; }
            public string Description { get; set; }
        }
    }
}