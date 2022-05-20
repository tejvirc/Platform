namespace Executor
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;

    public static class LogicDoorUtil
    {
        private static SafeFileHandle _deviceHandle;

        // Largely copied from main Platform solution.  File: Hardware\Device\IOGen8.cs
        public static bool DoorOpen()
        {
            try
            {
                // Create device handle.
                _deviceHandle = NativeMethods.CreateFile(
                    "\\\\.\\MK7IBUS0",
                    0x80000000 | 0x40000000,
                    0,
                    IntPtr.Zero,
                    3,
                    0,
                    IntPtr.Zero);

                ulong inputs = 0;

                // Allocate memory for ulong.
                var inputRegister = Marshal.AllocHGlobal(Marshal.SizeOf(inputs));
                 
                int numBytesReturned;
                bool success = NativeMethods.DeviceIoControl(
                    _deviceHandle,
                    TP940DIO_MAKE_IOCTL(),
                    IntPtr.Zero,
                    0,                          // nInBufferSize
                    inputRegister,              // lpOutBuffer
                    Marshal.SizeOf(inputs),     // nOutBufferSize
                    out numBytesReturned,       // lpBytesReturned
                    IntPtr.Zero);               // lpOverlapped

                if (!success)
                {
                    App.Log.Warn("Win32 call to check logic door was not successful. Assuming logic door is closed.");
                    return false;
                }

                if (_deviceHandle.IsInvalid)
                {
                    App.Log.Warn("Win32 call to check logic door yielded an invalid device handle. Assuming logic door is closed.");
                    return false;
                }

                return _isDoorOpen(inputRegister);
            }
            catch (Exception e)
            {
                App.Log.Warn("Exception caught while checking logic door, assuming door is closed: " + e.Message + " " + e.StackTrace);
            }

            return false;
        }

        private static bool _isDoorOpen(IntPtr ptr)
        {
            long log2DoorOpenStatus = 0x200000000000;
            long m = Marshal.ReadInt64(ptr);

            App.Log.Info("m = " + m);

            long n = m & log2DoorOpenStatus;
            App.Log.Info("m & doorOpenStatus = " + n);

            bool res = (m & log2DoorOpenStatus) == log2DoorOpenStatus;
            App.Log.Info("(m & log2DoorOpenStatus) == log2DoorOpenStatus = " + res);

            return res;
        }


        /// <summary>Function that creates IOCTL code for the TPCI940.</summary>
        /// <param name="command">Function Creates IOCTL code for the TPCI940</param>
        /// <returns>I/O control code (IOCTL)</returns>
        private static uint TP940DIO_MAKE_IOCTL()
        {
            // 57 represents Gen8IOCommands.InputData
            return CTL_CODE((uint)0x00000022, 0x840 + (uint)57, (uint)0, (uint)0);
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

    /// <summary>
    ///  Taken from the Platform ... Hardware\IO\IOGen8\IOGen8\NativeMethods
    /// </summary>
    public static class NativeMethods
    {
        /// <summary>DeviceIoControl function.</summary>
        /// <param name="deviceHandle">A handle to the device on which the operation is to be performed</param>
        /// <param name="dwioControlCode">The control code for the operation</param>
        /// <param name="inBuffer">A pointer to the input buffer that contains the data required to perform the operation</param>
        /// <param name="bufferSize">The size of the input buffer, in bytes</param>
        /// <param name="outBuffer">A pointer to the output buffer that is to receive the data returned by the operation</param>
        /// <param name="outBufferSize">The size of the output buffer, in bytes</param>
        /// <param name="bytesReturned">
        ///     A pointer to a variable that receives the size of the data stored in the output buffer, in
        ///     bytes
        /// </param>
        /// <param name="overlapped">A pointer to an OVERLAPPED structure</param>
        /// <returns>If the operation completes successfully, the return value is True.</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("Kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            [In] SafeFileHandle deviceHandle,
            [In] uint dwioControlCode, // EIOControlCode
            [In] IntPtr inBuffer,
            [In] int bufferSize,
            [Out] IntPtr outBuffer,
            [In] int outBufferSize,
            out int bytesReturned,
            [In] IntPtr overlapped);

        /// <summary>Creates File handle for access to device.</summary>
        /// <param name="fileName">The name of the file or device to be created or opened</param>
        /// <param name="desiredAccess">
        ///     The requested access to the file or device, which can be summarized as read, write, both or neither
        /// </param>
        /// <param name="shareMode">The requested sharing mode of the file or device</param>
        /// <param name="securityAttributes">A pointer to a SECURITY_ATTRIBUTES structure </param>
        /// <param name="creationDisposition">An action to take on a file or device that exists or does not exist</param>
        /// <param name="flagsAndAttributes">The file or device attributes and flags</param>
        /// <param name="templateFile">A valid handle to a template file with the GENERIC_READ access right</param>
        /// <returns>A file handle for the device.</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);
    }
}
