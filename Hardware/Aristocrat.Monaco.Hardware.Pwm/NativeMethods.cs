namespace Aristocrat.Monaco.Hardware.Pwm
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    ///     Groups the native Win32 APIs used for the serial communications
    /// </summary>
    internal class NativeMethods
    {

        /// <summary> Opening, Testing and Closing the Port Handle. </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

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
        internal static extern bool DeviceIoControl(
            [In] SafeFileHandle deviceHandle,
            [In] uint dwioControlCode, // EIOControlCode
            [In] IntPtr inBuffer,
            [In] int bufferSize,
            [Out] IntPtr outBuffer,
            [In] uint outBufferSize,
            out uint bytesReturned,
            [In] ref NativeOverlapped lpOverlapped);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("Kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            [In] SafeFileHandle deviceHandle,
            [In] uint dwioControlCode, // EIOControlCode
            [In] IntPtr inBuffer,
            [In] uint bufferSize,
            [Out] IntPtr outBuffer,
            [In] uint outBufferSize,
            out uint bytesReturned,
            [In] IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool ReadFile(
            [In] SafeFileHandle hFile,
            [In] IntPtr inBuffer,
            [In] uint nNumberOfBytesToRead,
            out uint nNumberOfBytesRead,
            [In] IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool ReadFile(
            SafeFileHandle hFile,
            [In] IntPtr inBuffer,
            uint nNumberOfBytesToRead,
            out uint nNumberOfBytesRead,
            [In] ref NativeOverlapped lpOverlapped);

        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CM_Get_Device_Interface_List(
            ref Guid interfaceClassGuid,
            string deviceID, char[] buffer,
            uint bufferLength,
            uint flags);

        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CM_Get_Device_Interface_List_Size(
            out uint size,
            ref Guid interfaceClassGuid,
            string deviceID,
            uint flags);

        [DllImport("kernel32.dll")]
        internal static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]


        internal static extern bool GetOverlappedResult(
            SafeFileHandle hFile,
            [In] ref NativeOverlapped lpOverlapped,
            out uint lpNumberOfBytesTransferred,
            bool bWait);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateEvent(
            IntPtr securityAttributes,
            int bManualReset,
            int bInitialState,
            string lpName);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool CancelIoEx(SafeFileHandle hFile, IntPtr lpOverlapped);
    }
}
