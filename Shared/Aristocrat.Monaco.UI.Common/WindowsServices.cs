// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberHidesStaticFromOuterClass
namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 
    /// </summary>
    public static class WindowsServices
    {
        private const int WsExTransparent = 0x00000020;
        private const int GwlExstyle = -20;

        /// <summary></summary>
        public const int WM_TOUCH = 0x0240;

        // Touch event flags ((TOUCHINPUT.dwFlags) [winuser.h]
        /// <summary></summary>
        public const int TOUCHEVENTF_MOVE = 0x0001;

        /// <summary></summary>
        public const int TOUCHEVENTF_DOWN = 0x0002;

        /// <summary></summary>
        public const int TOUCHEVENTF_UP = 0x0004;

        /// <summary></summary>
        public const int TOUCHEVENTF_INRANGE = 0x0008;

        /// <summary></summary>
        public const int TOUCHEVENTF_PRIMARY = 0x0010;

        /// <summary></summary>
        public const int TOUCHEVENTF_NOCOALESCE = 0x0020;

        /// <summary></summary>
        public const int TOUCHEVENTF_PEN = 0x0040;

        /// <summary></summary>
        public const int TOUCHEVENTF_PALM = 0x0080;

        /// <summary></summary>
        public enum TouchWindowFlag : uint
        {
            /// <summary>FineTouch</summary>
            FineTouch = 0x1,
            /// <summary>WantPalm</summary>
            WantPalm = 0x2
        }

        /// <summary>
        /// TOUCHINPUT
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TOUCHINPUT
        {
            /// LONG->int
            public int x;

            /// LONG->int
            public int y;

            /// HANDLE->void*
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private IntPtr hSource;

            /// DWORD->unsigned int
            public uint dwID;

            /// DWORD->unsigned int
            public uint dwFlags;

            /// DWORD->unsigned int
            public uint dwMask;

            /// DWORD->unsigned int
            public uint dwTime;

            /// ULONG_PTR->unsigned int
            public uint dwExtraInfo;

            /// DWORD->unsigned int
            public uint cxContact;

            /// DWORD->unsigned int
            public uint cyContact;

            /// <summary> The hardware source pointer </summary>
            // ReSharper disable once ConvertToAutoProperty
            public IntPtr HSource => hSource;
        }

        /// <summary> Device information for HID Touch </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_HID
        {
            /// <summary> The vendor ID for this device </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwVendorId;
            /// <summary> The product ID for this device </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwProductId;
            /// <summary> The version number for this device </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwVersionNumber;
            /// <summary> The us usage page for this device </summary>
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsagePage;
            /// <summary> The us usage for this device </summary>
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsage;
        }

        /// <summary> Device information for a keyboard </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_KEYBOARD
        {
            /// <summary> The type of keyboard </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
            /// <summary> The sub type of keyboard </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwSubType;
            /// <summary> The keyboard mode </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwKeyboardMode;
            /// <summary> The number of function keys </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfFunctionKeys;
            /// <summary> The number of indicators </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfIndicators;
            /// <summary> The total number of keys </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfKeysTotal;
        }

        /// <summary> Device information for a mouse </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_MOUSE
        {
            /// <summary> The id for hte mouse </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwId;
            /// <summary> The number of buttons on the mouse </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfButtons;
            /// <summary> The sample rate for the mouse </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwSampleRate;
            /// <summary> Whether or not has a horizontal wheel </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int fHasHorizontalWheel;
        }

        /// <summary> Device information </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct RID_DEVICE_INFO
        {
            /// <summary> The size of the data </summary>
            [FieldOffset(0)]
            public int cbSize;
            /// <summary> The device type </summary>
            [FieldOffset(4)]
            public int dwType;
            /// <summary> The mouse data </summary>
            [FieldOffset(8)]
            public RID_DEVICE_INFO_MOUSE mouse;
            /// <summary> The keyboard data </summary>
            [FieldOffset(8)]
            public RID_DEVICE_INFO_KEYBOARD keyboard;
            /// <summary> The HID device data </summary>
            [FieldOffset(8)]
            public RID_DEVICE_INFO_HID hid;
        }

        /// <summary> The device information types </summary>
        public enum DeviceInfoTypes
        {
            /// <summary> Pre-parse data </summary>
            RIDI_PREPARSEDDATA = 0x20000005,
            /// <summary> Device name </summary>
            RIDI_DEVICENAME = 0x20000007,
            /// <summary> Device info </summary>
            RIDI_DEVICEINFO = 0x2000000B
        }

        /// <summary> The device type for the device information </summary>
        public enum DeviceType
        {
            /// <summary> A HID device type </summary>
            HID = 2
        }

        /// <summary></summary>
        public static void SetWindowExTransparent(IntPtr hwnd, bool transparent = true)
        {
            var extendedStyle = NativeMethods.GetWindowLong(hwnd, GwlExstyle);

            var style = transparent ? extendedStyle | WsExTransparent : extendedStyle & ~WsExTransparent;

            NativeMethods.SetWindowLong(hwnd, GwlExstyle, style);
        }

        /// <summary>
        ///     Gets the touch device information for the provided device pointer
        /// </summary>
        /// <param name="hDevice">The device to get the information for</param>
        /// <param name="pData">The data where the information will be saved</param>
        /// <param name="pcbSize">The size of the device information expected</param>
        public static void GetTouchDeviceInfo(IntPtr hDevice, ref RID_DEVICE_INFO pData, ref uint pcbSize)
        {
            NativeMethods.GetRawInputDeviceInfo(hDevice, (uint)DeviceInfoTypes.RIDI_DEVICEINFO, ref pData, ref pcbSize);
        }

        /// <summary>
        /// RegisterTouchWindow
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool RegisterTouchWindow(IntPtr hwnd, TouchWindowFlag flag)
        {
            return NativeMethods.RegisterTouchWindow(hwnd, flag);
        }

        /// <summary>
        /// UnregisterTouchWindow
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool UnregisterTouchWindow(IntPtr hwnd)
        {
            return NativeMethods.UnregisterTouchWindow(hwnd);
        }

        /// <summary>
        /// GetTouchInputInfo
        /// </summary>
        /// <param name="hTouchInput"></param>
        /// <param name="cInputs"></param>
        /// <param name="pInputs"></param>
        /// <returns></returns>
        public static bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, TOUCHINPUT[] pInputs)
        {
            return NativeMethods.GetTouchInputInfo(hTouchInput, cInputs, pInputs, Marshal.SizeOf(pInputs[0]));
        }

        /// <summary>
        /// CloseTouchInputHandle
        /// </summary>
        /// <param name="lParam"></param>
        public static void CloseTouchInputHandle(IntPtr lParam)
        {
            NativeMethods.CloseTouchInputHandle(lParam);
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern int GetWindowLong(IntPtr hwnd, int index);

            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

            [DllImport("user32")]
            public static extern bool RegisterTouchWindow(IntPtr hWnd, TouchWindowFlag flags);

            [DllImport("user32")]
            public static extern bool UnregisterTouchWindow(IntPtr hWnd);

            [DllImport("user32")]
            public static extern bool GetTouchInputInfo(
                IntPtr hTouchInput,
                int cInputs,
                [In, Out] TOUCHINPUT[] pInputs,
                int cbSize);

            [DllImport("user32")]
            public static extern void CloseTouchInputHandle(IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "GetRawInputDeviceInfoA")]
            public static extern uint GetRawInputDeviceInfo(
                IntPtr hDevice,
                uint uiCommand,
                ref RID_DEVICE_INFO pData,
                ref uint pcbSize);
        }
    }
}