namespace Aristocrat.Monaco.Application.Time
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    /// <summary>
    ///     TimeZoneInteropWrapper.
    /// </summary>
    internal static class TimeZoneInteropWrapper
    {
        /// <summary>
        ///     description...
        /// </summary>
        public const string SeTimeZoneName = "SeTimeZonePrivilege";

        /// <summary>
        ///     description...
        /// </summary>
        public const string SeShutdownName = "SeShutdownPrivilege";

        /// <summary>
        ///     description...
        /// </summary>
        public const string SeRestoreName = "SeRestorePrivilege";

        /// <summary>
        ///     description...
        /// </summary>
        public const string SeBackupName = "SeBackupPrivilege";

        private const int SePrivilegeEnabled = 0x00000002;
        private const int TokenQuery = 0x00000008;
        private const int TokenAdjustPrivileges = 0x00000020;

        private const int WmSettingChange = 0x001a;
        private const int HwndBroadcast = 0xffff;
        private const int SmtoAbortIfHung = 0x0002;
        private const int SmtoBlock = 0x0001;
        private const int SmtoNormal = 0x0000;
        private const int SmtoNoTimeOutIfNotHung = 0x0008;
        private const int SmtoErrorOnExit = 0x0020;

        /// <summary>
        ///     <method>name="BroadcastSettingsChange"</method>.
        /// </summary>
        /// <returns>
        ///     returns...
        /// </returns>
        public static int BroadcastSettingsChange()
        {
            var rtnValue = 0;
            return NativeMethods.SendMessageTimeout(
                HwndBroadcast,
                WmSettingChange,
                0,
                "intl",
                SmtoAbortIfHung,
                1000,
                ref rtnValue);
        }

        /// <summary>
        ///     <method>name="CheckWin32Error"</method>.
        /// </summary>
        public static void CheckWin32Error()
        {
            var rc = Marshal.GetLastWin32Error();
            if (rc != 0)
            {
                throw new Win32Exception(rc);
            }
        }

        /// <summary>
        ///     <method>name="ToSystemTime"</method>.
        /// </summary>
        /// <param name="dt">
        ///     parameter...
        /// </param>
        /// <returns>
        ///     returns...
        /// </returns>
        public static NativeMethods.SystemTime ToSystemTime(this DateTime dt)
        {
            var st = default(NativeMethods.SystemTime);
            st.wYear = 0; // Most timezone changes are relative each year
            st.wMonth = (short)dt.Month;

            var weekdayOfMonth = 1; // e.g. third Monday = 3...
            for (var dd = dt.Day; dd > 7; dd -= 7)
            {
                weekdayOfMonth++;
            }

            st.wDay = (short)weekdayOfMonth;
            st.wDayOfWeek = (short)dt.DayOfWeek;
            st.wHour = (short)dt.Hour;
            st.wMinute = (short)dt.Minute;
            st.wSecond = (short)dt.Second;
            st.wMilliseconds = (short)dt.Millisecond;

            return st;
        }

        /// <summary>
        ///     <method>name="ToSystemTime"</method>.
        /// </summary>
        /// <param name="tt">
        ///     parameter...
        /// </param>
        /// <returns>
        ///     returns...
        /// </returns>
        public static NativeMethods.SystemTime ToSystemTime(this TimeZoneInfo.TransitionTime tt)
        {
            var st = default(NativeMethods.SystemTime);

            st.wYear = 0;
            st.wMonth = (short)tt.Month; // 1 = January

            var weekdayOfMonth = 1; // e.g. third Monday = 3...
            for (var dd = tt.Day; dd > 7; dd -= 7)
            {
                weekdayOfMonth++;
            }

            st.wDay = (short)weekdayOfMonth;
            st.wDayOfWeek = (short)tt.DayOfWeek; // 0 = Sunday...

            st.wHour = (short)tt.TimeOfDay.Hour;
            st.wMinute = (short)tt.TimeOfDay.Minute;
            st.wSecond = (short)tt.TimeOfDay.Second;
            st.wMilliseconds = (short)tt.TimeOfDay.Millisecond;

            return st;
        }

        /// <summary>
        ///     <method>SetTokenPrivilege</method>.
        /// </summary>
        /// <param name="privilege">
        ///     A pointer to a SystemTime structure that contains the new system date and time.
        ///     The wDayOfWeek member of the SYSTEMTIME structure is ignored.
        /// </param>
        /// <returns>
        ///     returns...
        /// </returns>
        public static bool SetTokenPrivilege(string privilege)
        {
            NativeMethods.TokPriv1Luid tp;
            var hproc = NativeMethods.GetCurrentProcess();
            var htok = IntPtr.Zero;

            NativeMethods.OpenProcessToken(hproc, TokenAdjustPrivileges | TokenQuery, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SePrivilegeEnabled;
            NativeMethods.LookupPrivilegeValue(null, privilege, ref tp.Luid);

            return NativeMethods.AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        ///     <method>UnsetTokenPrivilege</method>.
        /// </summary>
        /// <param name="privilege">
        ///     A pointer to a SystemTime structure that contains the new system date and time.
        ///     The wDayOfWeek member of the SYSTEMTIME structure is ignored.
        /// </param>
        /// <returns>
        ///     returns true if privilege set successfully
        /// </returns>
        public static bool UnsetTokenPrivilege(string privilege)
        {
            try
            {
                var tp = default(NativeMethods.TokPriv1Luid);
                var hproc = NativeMethods.GetCurrentProcess();
                var htok = IntPtr.Zero;

                NativeMethods.OpenProcessToken(hproc, TokenAdjustPrivileges | TokenQuery, ref htok);
                tp.Count = 1;
                tp.Luid = 0;
                NativeMethods.LookupPrivilegeValue(null, privilege, ref tp.Luid);
                NativeMethods.AdjustTokenPrivileges(htok, false, ref tp, 1024, IntPtr.Zero, IntPtr.Zero);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     <method>SetDynamicTimeZone</method>.
        /// </summary>
        /// <param name="dtzi">
        ///     A pointer to a SystemTime structure that contains the new system date and time.
        ///     The wDayOfWeek member of the SYSTEMTIME structure is ignored.
        /// </param>
        public static void SetDynamicTimeZone(NativeMethods.DynamicTimeZoneInformation dtzi)
        {
            // Setting timezone is filtered from token by default
            SetTokenPrivilege(SeTimeZoneName);
            CheckWin32Error();

            try
            {
                // Set local system timezone
                if (!NativeMethods.SetDynamicTimeZoneInformation(ref dtzi))
                {
                    CheckWin32Error();
                }
                else
                {
                    CultureInfo.CurrentCulture.ClearCachedData();
                    TimeZoneInfo.ClearCachedData();

                    // Broadcast to other components in Windows that TimeZone has changed.
                    if (BroadcastSettingsChange() == 0)
                    {
                        CheckWin32Error();
                    }
                }
            }
            finally
            {
                // Setting timezone is filtered from token by default
                UnsetTokenPrivilege(SeTimeZoneName);
                CheckWin32Error();
            }
        }

        /// <summary>
        ///     Initialise the Tzi struct.
        /// </summary>
        /// <param name="info">The Tzi data from the registry.</param>
        /// <returns>
        ///     returns NativeMethods.Tzi struct
        /// </returns>
        public static NativeMethods.Tzi InitTzi(byte[] info)
        {
            var tzi = default(NativeMethods.Tzi);

            if (info.Length != Marshal.SizeOf(tzi))
            {
                return tzi;
            }

            var h = GCHandle.Alloc(info, GCHandleType.Pinned);

            try
            {
                var tziBox = (NativeMethods.Tzi?)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(NativeMethods.Tzi));
                if (tziBox == null)
                    throw new NullReferenceException("h.AddrOfPinnedObject()");
                tzi = tziBox.Value;
                return tzi;
            }
            finally
            {
                h.Free();
            }
        }

        /// <summary>
        ///     <method>SetDynamicTimeZone</method>.
        /// </summary>
        /// <param name="timeZoneInfo">
        ///     TimeZoneInfo to change to.
        ///     The wDayOfWeek member of the SYSTEMTIME structure is ignored.
        /// </param>
        public static void SetDynamicTimeZone(TimeZoneInfo timeZoneInfo)
        {
            // open key where all time zones are located in the registry
            var registryKeyTimeZoneSelected = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows NT\CurrentVersion\Time Zones\" + timeZoneInfo.Id);

            if (registryKeyTimeZoneSelected == null)
            {
                SetDynamicTimeZone(timeZoneInfo.ToDynamicTimeZoneInformation());
                return;
            }

            var tzi = InitTzi((byte[])registryKeyTimeZoneSelected.GetValue("Tzi"));
            var timeZoneInformation =
                default(NativeMethods.DynamicTimeZoneInformation);

            timeZoneInformation.bias = tzi.bias;
            timeZoneInformation.daylightBias = tzi.daylightBias;
            timeZoneInformation.standardBias = tzi.standardBias;
            timeZoneInformation.daylightDate = tzi.daylightDate;
            timeZoneInformation.standardDate = tzi.standardDate;

            var strValue = (string)registryKeyTimeZoneSelected.GetValue("MUI_Dlt");
            timeZoneInformation.daylightName = string.IsNullOrEmpty(strValue)
                ? (string)registryKeyTimeZoneSelected.GetValue("Dlt")
                : strValue;

            strValue = (string)registryKeyTimeZoneSelected.GetValue("MUI_Std");
            timeZoneInformation.standardName = string.IsNullOrEmpty(strValue)
                ? (string)registryKeyTimeZoneSelected.GetValue("Std")
                : strValue;

            timeZoneInformation.timeZoneKeyName = timeZoneInfo.StandardName;
            timeZoneInformation.dynamicDaylightTimeDisabled = false;

            SetDynamicTimeZone(timeZoneInformation);
        }

        /// <summary>
        ///     <method>GetDynamicTimeZone</method>.
        /// </summary>
        /// <returns>
        ///     returns...
        /// </returns>
        public static NativeMethods.DynamicTimeZoneInformation GetDynamicTimeZone()
        {
            GetDynamicTimeZoneInformation(out var dtzi);

            CheckWin32Error();

            return dtzi;
        }

        /// <summary>
        ///     <method>GetDynamicTimeZoneInformation</method>.
        /// </summary>
        /// <param name="dtzi">
        ///     paramater...
        /// </param>
        /// <returns>
        ///     returns...
        /// </returns>
        public static int GetDynamicTimeZoneInformation(out NativeMethods.DynamicTimeZoneInformation dtzi)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <method>ToDynamicTimeZoneInformation</method>.
        /// </summary>
        /// <param name="tzi">
        ///     paramater...
        /// </param>
        /// <returns>
        ///     returns DynamicTimeZoneInformation
        /// </returns>
        public static NativeMethods.DynamicTimeZoneInformation ToDynamicTimeZoneInformation(this TimeZoneInfo tzi)
        {
            var cpy = default(NativeMethods.DynamicTimeZoneInformation);
            cpy.bias = (int)tzi.BaseUtcOffset.TotalMinutes;

            var rules = tzi.GetAdjustmentRules();

            if (tzi.SupportsDaylightSavingTime && rules.Length > 0)
            {
                cpy.daylightName = tzi.DaylightName;
                cpy.daylightBias = (int)rules[0].DaylightDelta.TotalMinutes;
                cpy.dynamicDaylightTimeDisabled = false;

                foreach (var rule in rules)
                {
                    if (DateTime.Now >= rule.DateStart && DateTime.Now <= rule.DateEnd)
                    {
                        cpy.daylightDate = rule.DaylightTransitionStart.ToSystemTime();
                        cpy.standardDate = rule.DaylightTransitionEnd.ToSystemTime();
                        break;
                    }
                }

                cpy.daylightDate = rules[0].DateStart.ToSystemTime();
            }
            else
            {
                cpy.daylightName = string.Empty;
                cpy.dynamicDaylightTimeDisabled = true;
            }

            cpy.standardName = tzi.StandardName;
            cpy.timeZoneKeyName = tzi.StandardName;

            return cpy;
        }
    }
}