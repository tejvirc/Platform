namespace Aristocrat.Monaco.Hardware.Contracts.VHD
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    ///     Represents a VHD SafeHandle
    /// </summary>
    public class VirtualDiskHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualDiskHandle" /> class.
        /// </summary>
        public VirtualDiskHandle()
            : base(true)
        {
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            return SafeNativeMethods.CloseHandle(handle);
        }

        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr handle);
        }
    }
}