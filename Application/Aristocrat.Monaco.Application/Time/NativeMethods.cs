// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace Aristocrat.Monaco.Application.Time
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     NativeMethods contains methods and structs used to set the time on the system using
    ///     Win32 API calls.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///     Sets the current system time and date. The system time is expressed in
        ///     Coordinated Universal Time (UTC)
        /// </summary>
        /// <param name="lpSystemTime">
        ///     A pointer to a SYSTEMTIME structure that contains the new system date and time.
        ///     The wDayOfWeek member of the SYSTEMTIME structure is ignored.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetSystemTime(ref SystemTime lpSystemTime);

        /// <summary>
        ///     Get The TimeZone Information.
        /// </summary>
        /// <param name="lpTimeZoneInformation">
        ///     A pointer to a TimeZoneInformation structure.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetTimeZoneInformation(out TimeZoneInformation lpTimeZoneInformation);

        /// <summary>
        ///     Set The System Time Zone Information.
        /// </summary>
        /// <param name="lpTimeZoneInformation">
        ///     A pointer to a TimeZoneInformation structure.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetTimeZoneInformation([In] ref TimeZoneInformation lpTimeZoneInformation);

        /// <summary>
        ///     GetDynamicTimeZoneInformation.
        /// </summary>
        /// <param name="lpDynamicTimeZoneInformation">
        ///     A pointer to a TimeZoneInformation structure.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetDynamicTimeZoneInformation(
            out DynamicTimeZoneInformation lpDynamicTimeZoneInformation);

        /// <summary>
        ///     SetDynamicTimeZoneInformation.
        /// </summary>
        /// <param name="lpDynamicTimeZoneInformation">
        ///     A pointer to a TimeZoneInformation structure.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDynamicTimeZoneInformation(
            [In] ref DynamicTimeZoneInformation lpDynamicTimeZoneInformation);

        /// <summary>
        ///     AdjustTokenPrivileges.
        /// </summary>
        /// <param name="htok">
        ///     htok.
        /// </param>
        /// <param name="disall">
        ///     disall.
        /// </param>
        /// <param name="newst">
        ///     newst.
        /// </param>
        /// <param name="len">
        ///     len.
        /// </param>
        /// <param name="prev">
        ///     prev.
        /// </param>
        /// <param name="relen">
        ///     relen.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr htok,
            [MarshalAs(UnmanagedType.Bool)] bool disall,
            ref TokPriv1Luid newst,
            int len,
            IntPtr prev,
            IntPtr relen);

        /// <summary>
        ///     Get The TimeZone Information.
        /// </summary>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>
        ///     Get The TimeZone Information.
        /// </summary>
        /// <param name="h">
        ///     h.
        /// </param>
        /// <param name="acc">
        ///     acc.
        /// </param>
        /// <param name="phtok">
        ///     phtok.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        /// <summary>
        ///     Get The TimeZone Information.
        /// </summary>
        /// <param name="host">
        ///     host.
        /// </param>
        /// <param name="name">
        ///     name.
        /// </param>
        /// <param name="pluid">
        ///     pluid.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        /// <summary>
        ///     Get The TimeZone Information.
        /// </summary>
        /// <param name="hwnd">
        ///     host.
        /// </param>
        /// <param name="msg">
        ///     name.
        /// </param>
        /// <param name="wParam">
        ///     wParam.
        /// </param>
        /// <param name="lParam">
        ///     lParam.
        /// </param>
        /// <param name="fuFlags">
        ///     pluid.
        /// </param>
        /// <param name="uTimeout">
        ///     fuFlags.
        /// </param>
        /// <param name="lpdwResult">
        ///     lpdwResult.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails,
        ///     the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport(
            "user32",
            EntryPoint = "SendMessageTimeoutA",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int SendMessageTimeout(
            int hwnd,
            int msg,
            int wParam,
            string lParam,
            int fuFlags,
            int uTimeout,
            ref int lpdwResult);

        /// <summary>
        ///     Retrieves the calling thread's last-error code value.
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        /// <summary>
        ///     Specifies a date and time, using individual members for the month, day, year,
        ///     weekday, hour, minute, second, and millisecond. The time is either in coordinated
        ///     universal time (UTC) or local time, depending on the function that is being called.
        /// </summary>
        public struct SystemTime
        {
            /// <summary>
            ///     The year. The valid values for this member are 1601 through 30827.
            /// </summary>
            public short wYear;

            /// <summary>
            ///     The month. The valid values for this member are 1 through 12.
            /// </summary>
            public short wMonth;

            /// <summary>
            ///     The day of the week. The valid values for this member are 0 through 6.
            /// </summary>
            public short wDayOfWeek;

            /// <summary>
            ///     The day of the month. The valid values for this member are 1 through 31.
            /// </summary>
            public short wDay;

            /// <summary>
            ///     The hour. The valid values for this member are 0 through 23.
            /// </summary>
            public short wHour;

            /// <summary>
            ///     The minute. The valid values for this member are 0 through 59.
            /// </summary>
            public short wMinute;

            /// <summary>
            ///     The second. The valid values for this member are 0 through 59.
            /// </summary>
            public short wSecond;

            /// <summary>
            ///     The millisecond. The valid values for this member are 0 through 999.
            /// </summary>
            public short wMilliseconds;
        }

        /// <summary>
        ///     The layout of the Tzi value in the registry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Tzi
        {
            /// <summary>
            ///     The bias.
            /// </summary>
            public int bias;

            /// <summary>
            ///     The standardBias.
            /// </summary>
            public int standardBias;

            /// <summary>
            ///     The daylightBias.
            /// </summary>
            public int daylightBias;

            /// <summary>
            ///     The standardDate.
            /// </summary>
            public SystemTime standardDate;

            /// <summary>
            ///     The daylightDate.
            /// </summary>
            public SystemTime daylightDate;
        }

        /// <summary>
        ///     TimeZoneInformation
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TimeZoneInformation
        {
            /// <summary>
            ///     The bias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int bias;

            /// <summary>
            ///     The standardName.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string standardName;

            /// <summary>
            ///     The standardDate.
            /// </summary>
            public SystemTime standardDate;

            /// <summary>
            ///     The standardBias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int standardBias;

            /// <summary>
            ///     The daylightName.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string daylightName;

            /// <summary>
            ///     The daylightDate.
            /// </summary>
            public SystemTime daylightDate;

            /// <summary>
            ///     The daylightBias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int daylightBias;
        }

        /// <summary>
        ///     DynamicTimeZoneInformation
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DynamicTimeZoneInformation
        {
            /// <summary>
            ///     The bias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int bias;

            /// <summary>
            ///     The standardName.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string standardName;

            /// <summary>
            ///     The standardDate.
            /// </summary>
            public SystemTime standardDate;

            /// <summary>
            ///     The standardBias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int standardBias;

            /// <summary>
            ///     The daylightName.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string daylightName;

            /// <summary>
            ///     The daylightDate.
            /// </summary>
            public SystemTime daylightDate;

            /// <summary>
            ///     The daylightBias.
            /// </summary>
            [MarshalAs(UnmanagedType.I4)] public int daylightBias;

            /// <summary>
            ///     The timeZoneKeyName.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string timeZoneKeyName;

            /// <summary>
            ///     The dynamicDaylightTimeDisabled.
            /// </summary>
            public bool dynamicDaylightTimeDisabled;
        }

        /// <summary>
        ///     TokPriv1Luid
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            /// <summary>
            ///     The Count.
            /// </summary>
            public int Count;

            /// <summary>
            ///     The Luid.
            /// </summary>
            public long Luid;

            /// <summary>
            ///     The Attr.
            /// </summary>
            public int Attr;
        }
    }
}