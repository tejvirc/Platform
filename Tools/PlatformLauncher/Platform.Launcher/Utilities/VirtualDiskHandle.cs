namespace Platform.Launcher.Utilities
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class VirtualDiskHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public VirtualDiskHandle()
            : base(true)
        {
        }

        /// <inheritdoc />
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return SafeNativeMethods.CloseHandle(handle);
        }

        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            public static extern bool CloseHandle(IntPtr handle);
        }
    }
}