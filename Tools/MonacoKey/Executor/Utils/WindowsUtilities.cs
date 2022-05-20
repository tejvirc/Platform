﻿namespace Executor.Utils
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    // Shoutout, Donovan Meyer... Who'd have thought shutdown took all this.
    internal class WindowsUtilities
    {
        private const uint TokenQuery = 0x0008;
        private const uint TokenAdjustPrivileges = 0x0020;
        private const uint SePrivilegeEnabled = 0x00000002;
        private const string SeShutdownName = "SeShutdownPrivilege";

        public static void Reboot()
        {
            var tokenHandle = IntPtr.Zero;

            try
            {
                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TokenQuery | TokenAdjustPrivileges, out tokenHandle))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open process token handle");

                var tokenPrivs = new TokenPrivileges
                {
                    PrivilegeCount = 1,
                    Privileges = new LuidAndAttributes[1]
                };
                tokenPrivs.Privileges[0].Attributes = SePrivilegeEnabled;

                if (!LookupPrivilegeValue(null, SeShutdownName, out tokenPrivs.Privileges[0].Luid))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open lookup shutdown privilege");

                // Add the shutdown privilege to the process token
                if (!AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivs, 0, IntPtr.Zero, IntPtr.Zero))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to adjust process token privileges");

                // Reboot
                if (!ExitWindowsEx(ExitWindows.Reboot, ShutdownReason.MajorApplication | ShutdownReason.MajorHardware | ShutdownReason.FlagPlanned))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to reboot system");
            }
            finally
            {
                // close the process token
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out Luid lpLuid);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
            ref TokenPrivileges newState,
            uint zero,
            IntPtr null1,
            IntPtr null2);

        [Flags]
        private enum ExitWindows : uint
        {
            // ONE of the following five:
            LogOff = 0x00,
            ShutDown = 0x01,
            Reboot = 0x02,
            PowerOff = 0x08,
            RestartApps = 0x40,

            // plus AT MOST ONE of the following two:
            Force = 0x04,
            ForceIfHung = 0x10
        }

        [Flags]
        private enum ShutdownReason : uint
        {
            MajorApplication = 0x00040000,
            MajorHardware = 0x00010000,
            MajorLegacyApi = 0x00070000,
            MajorOperatingSystem = 0x00020000,
            MajorOther = 0x00000000,
            MajorPower = 0x00060000,
            MajorSoftware = 0x00030000,
            MajorSystem = 0x00050000,

            MinorBlueScreen = 0x0000000F,
            MinorCordUnplugged = 0x0000000b,
            MinorDisk = 0x00000007,
            MinorEnvironment = 0x0000000c,
            MinorHardwareDriver = 0x0000000d,
            MinorHotfix = 0x00000011,
            MinorHung = 0x00000005,
            MinorInstallation = 0x00000002,
            MinorMaintenance = 0x00000001,
            MinorMMC = 0x00000019,
            MinorNetworkConnectivity = 0x00000014,
            MinorNetworkCard = 0x00000009,
            MinorOther = 0x00000000,
            MinorOtherDriver = 0x0000000e,
            MinorPowerSupply = 0x0000000a,
            MinorProcessor = 0x00000008,
            MinorReconfig = 0x00000004,
            MinorSecurity = 0x00000013,
            MinorSecurityFix = 0x00000012,
            MinorSecurityFixUninstall = 0x00000018,
            MinorServicePack = 0x00000010,
            MinorServicePackUninstall = 0x00000016,
            MinorTermSrv = 0x00000020,
            MinorUnstable = 0x00000006,
            MinorUpgrade = 0x00000003,
            MinorWMI = 0x00000015,

            FlagUserDefined = 0x40000000,
            FlagPlanned = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Luid
        {
            public readonly uint LowPart;
            public readonly int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LuidAndAttributes
        {
            public Luid Luid;
            public uint Attributes;
        }

        private struct TokenPrivileges
        {
            public uint PrivilegeCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public LuidAndAttributes[] Privileges;
        }
    }
}