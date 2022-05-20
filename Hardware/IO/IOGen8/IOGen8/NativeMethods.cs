namespace Vgt.Client12.Hardware.IO
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    ///     A static helper class for native P/Invoke calls.
    /// </summary>
    internal class NativeMethods
    {
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

        /// <summary>
        ///     Enumerates all system firmware tables of the specified type.
        /// </summary>
        /// <param name="firmwareTableProviderSignature">
        ///     The identifier of the firmware table provider to which the query is to be
        ///     directed. This parameter can be one of the following values.
        /// </param>
        /// <param name="firmwareTableBuffer">
        ///     A pointer to a buffer that receives the list of firmware tables. If this parameter is
        ///     NULL, the return value is the required buffer size.
        /// </param>
        /// <param name="bufferSize">The size of the firmwareTableBuffer buffer, in bytes.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint EnumSystemFirmwareTables(
            uint firmwareTableProviderSignature,
            IntPtr firmwareTableBuffer,
            uint bufferSize);

        /// <summary>
        ///     Retrieves the specified firmware table from the firmware table provider.
        /// </summary>
        /// <param name="firmwareTableProviderSignature">
        ///     The identifier of the firmware table provider to which the query is to be
        ///     directed. This parameter can be one of the following values.
        /// </param>
        /// <param name="firmwareTableId">
        ///     The identifier of the firmware table. This identifier is little endian, you must reverse
        ///     the characters in the string.
        /// </param>
        /// <param name="firmwareTableBuffer">
        ///     A pointer to a buffer that receives the requested firmware table. If this parameter
        ///     is NULL, the return value is the required buffer size.
        /// </param>
        /// <param name="bufferSize">The size of the firmwareTableBuffer buffer, in bytes.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetSystemFirmwareTable(
            uint firmwareTableProviderSignature,
            uint firmwareTableId,
            IntPtr firmwareTableBuffer,
            uint bufferSize);
    }
}