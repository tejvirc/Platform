namespace Vgt.Client12.Hardware.IO
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal class DeviceControl
    {
        /// <summary>Gen8 File name base.</summary>
        public const string Tpci940FileNameBase = "\\\\.\\MK7IBUS0";

        public const int Tpci940CardIdLength = 64;

        public const uint GenericRead = 0x80000000;

        public const uint GenericWrite = 0x40000000;

        public const uint FileDeviceUnknown = 0x00000022;

        public const uint MethodBuffered = 0;

        public const uint FileAnyAccess = 0;

        public const uint Tpci940ShareMode = 0;

        public const uint Tpci940FileCreationDistribution = 3;

        public const uint Tpci940FileAttributes = 0;

        public const uint Tpci940DesiredAccess = GenericRead | GenericWrite;

        public const ulong Tpci940AvailableInputsMask = 0xFFFFFFFFFFFFFFFF;

        public const uint Tpci940AvailableOutputMask = 0xFFFF;

        public const int Tpci940Gen8PollingFrequency = 75;

        public static readonly IntPtr Tpci940TemplateFile = IntPtr.Zero;

        public static readonly IntPtr Tpci940SecurityAttributes = IntPtr.Zero;

        /// <summary>Function that creates IOCTL code for the TPCI940.</summary>
        /// <param name="command">Function Creates IOCTL code for the TPCI940</param>
        /// <returns>I/O control code (IOCTL)</returns>
        public static uint TP940DIO_MAKE_IOCTL(Gen8IOCommands command)
        {
            return CTL_CODE(FileDeviceUnknown, 0x840 + (uint)command, MethodBuffered, FileAnyAccess);
        }

        public static bool Ioctl<T, TA>(SafeFileHandle deviceHandle, TA data, ref T ret, Gen8IOCommands cmd)
        {
            // Allocate memory for ulong.
            var dataHandle = default(GCHandle);

            var dataPointer = IntPtr.Zero;
            var outPointer = Marshal.AllocHGlobal(Marshal.SizeOf(ret));
            try
            {
                // Pin I/O control data
                if (data != null)
                {
                    dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    dataPointer = dataHandle.AddrOfPinnedObject();
                }

                // Perform I/O control
                var result = NativeMethods.DeviceIoControl(
                    deviceHandle,
                    TP940DIO_MAKE_IOCTL(cmd),
                    dataPointer,
                    Marshal.SizeOf(data),
                    outPointer,
                    Marshal.SizeOf(ret),
                    out _,
                    IntPtr.Zero);

                ret = (T)Marshal.PtrToStructure(outPointer, ret.GetType());

                return result;
            }
            finally
            {
                if (dataPointer != IntPtr.Zero)
                {
                    dataHandle.Free();
                }

                if (outPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(outPointer);
                }
            }
        }

        public static bool IoctlUint(SafeFileHandle deviceHandle, uint inData, ref uint retData, Gen8IOCommands cmd)
        {
            var inPointer = Marshal.AllocHGlobal(Marshal.SizeOf(inData));
            var outPointer = Marshal.AllocHGlobal(Marshal.SizeOf(retData));

            try
            {
                unsafe
                {
                    *(uint*)inPointer.ToPointer() = inData;
                    *(uint*)outPointer.ToPointer() = retData;
                }

                // Perform I/O control
                var result = NativeMethods.DeviceIoControl(
                    deviceHandle,
                    TP940DIO_MAKE_IOCTL(cmd),
                    inPointer,
                    Marshal.SizeOf(inData),
                    outPointer,
                    Marshal.SizeOf(retData),
                    out _,
                    IntPtr.Zero);

                unsafe
                {
                    retData = *(uint*)outPointer.ToPointer();
                }

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(inPointer);
                Marshal.FreeHGlobal(outPointer);
            }
        }

        public static bool IoctlByteArray<A>(SafeFileHandle deviceHandle, A data, ref byte[] ret, Gen8IOCommands cmd)
        {
            // Allocate memory for ulong.
            var dataHandle = default(GCHandle);

            var dataPointer = IntPtr.Zero;
            var outPointer = Marshal.AllocHGlobal(ret.Length);
            try
            {
                // Pin I/O control data
                if (data != null)
                {
                    dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    dataPointer = dataHandle.AddrOfPinnedObject();
                }

                // Perform I/O control
                var result = NativeMethods.DeviceIoControl(
                    deviceHandle,
                    TP940DIO_MAKE_IOCTL(cmd),
                    dataPointer,
                    Marshal.SizeOf(data),
                    outPointer,
                    ret.Length,
                    out _,
                    IntPtr.Zero);

                Marshal.Copy(outPointer, ret, 0, ret.Length);

                return result;
            }
            finally
            {
                if (dataPointer != IntPtr.Zero)
                {
                    dataHandle.Free();
                }

                if (outPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(outPointer);
                }
            }
        }

        public static bool Ioctl<A>(SafeFileHandle deviceHandle, A data, Gen8IOCommands cmd)
        {
            // Allocate memory for ulong.
            var dataHandle = default(GCHandle);

            var dataPointer = IntPtr.Zero;

            try
            {
                // Pin I/O control data
                if (data != null)
                {
                    dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    dataPointer = dataHandle.AddrOfPinnedObject();
                }

                // Perform I/O control
                var result = NativeMethods.DeviceIoControl(
                    deviceHandle,
                    TP940DIO_MAKE_IOCTL(cmd),
                    dataPointer,
                    Marshal.SizeOf(data),
                    IntPtr.Zero,
                    0,
                    out _,
                    IntPtr.Zero);
                return result;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (dataPointer != IntPtr.Zero)
                {
                    dataHandle.Free();
                }
            }
        }

        public static int ReadReg32(SafeFileHandle deviceHandle, Gen8PCI.Legacy offset)
        {
            var output = 0;
            var input = (int)offset;
            if (Ioctl(deviceHandle, input, ref output, Gen8IOCommands.ReadBar2Register32))
            {
                return output;
            }

            return 0;
        }

        public static bool WriteReg32(SafeFileHandle deviceHandle, Gen8PCI.Legacy offset, int value)
        {
            var input = ((long)value << 32) | (long)offset;
            if (Ioctl(deviceHandle, input, Gen8IOCommands.WriteBar2Register32))
            {
                return true;
            }

            return false;
        }

        public static bool Ioctl(SafeFileHandle deviceHandle, Gen8IOCommands cmd)
        {
            // Allocate memory for ulong.
            try
            {
                // Perform I/O control
                var result = NativeMethods.DeviceIoControl(
                    deviceHandle,
                    TP940DIO_MAKE_IOCTL(cmd),
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    out _,
                    IntPtr.Zero);
                return result;
            }
            catch
            {
                return false;
            }
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
    }
}