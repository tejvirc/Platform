// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Aristocrat.Monaco.Hardware
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const short FILE_SHARE_READ = 0x1;
        internal const short FILE_SHARE_WRITE = 0x2;
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const int ACCESS_NONE = 0;
        internal const int INVALID_HANDLE_VALUE = -1;
        internal const short OPEN_EXISTING = 3;
        internal const int WAIT_TIMEOUT = 0x102;
        internal const uint WAIT_OBJECT_0 = 0;
        internal const uint WAIT_FAILED = 0xffffffff;
        internal const uint ERROR_IO_PENDING = 997;
        internal const uint MAX_TOUCH_POINTS = 256;

        internal const int WAIT_INFINITE = -1;

        internal const int DBT_DEVICEARRIVAL = 0x8000;
        internal const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        internal const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        internal const int DBT_DEVTYP_HANDLE = 6;
        internal const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        internal const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        internal const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        internal const int WM_DEVICECHANGE = 0x219;
        internal const short DIGCF_PRESENT = 0x2;
        internal const short DIGCF_DEVICEINTERFACE = 0x10;
        internal const int DIGCF_ALLCLASSES = 0x4;

        [DllImport("user32.dll")]
        internal static extern bool UnregisterDeviceNotification(IntPtr handle);

        [DllImport("user32.dll")]
        internal static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            int Flags);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool CreateHardLink(string fileName, string existingFileName, IntPtr securityAttributes);

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public char dbcc_name;
            public static readonly int Size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE));
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal class DEV_BROADCAST_DEVICEINTERFACE_1
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HANDLE
        {
            internal int dbch_size;
            internal int dbch_devicetype;
            internal int dbch_reserved;
            internal int dbch_handle;
            internal int dbch_hdevnotify;
            internal Guid dbch_eventguid;
            internal long dbch_nameoffset;
            internal byte[] dbch_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HDR
        {
            internal int dbch_size;
            internal int dbch_devicetype;
            internal int dbch_reserved;
        }

        /// <summary>
        ///     CM_Locate_DevNodeA
        /// </summary>
        /// <param name="pdnDevInst"></param>
        /// <param name="pDeviceID"></param>
        /// <param name="ulFlags"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int CM_Locate_DevNodeA(out IntPtr pdnDevInst, string pDeviceID, int ulFlags);

        /// <summary>
        ///     CM_Reenumerate_DevNode
        /// </summary>
        /// <param name="dnDevInst"></param>
        /// <param name="ulFlags"></param>
        /// <param name="hMachine"></param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern int CM_Reenumerate_DevNode(int dnDevInst, int ulFlags, IntPtr hMachine);

        /// <summary>
        ///     CM_LOCATE_DEVNODE_NORMAL
        /// </summary>
        private const int CM_LOCATE_DEVNODE_NORMAL = 0x00000000;

        /// <summary>
        ///     CR_SUCCESS
        /// </summary>
        private const int CR_SUCCESS = 0x00000000;

        /// <summary>
        ///     Re-enumerate Devices
        /// </summary>
        /// <returns>Success</returns>
        public static bool ReenumerateDevices()
        {
            var apiResult = CM_Locate_DevNodeA(out var pdnDevInst, null, NativeMethods.CM_LOCATE_DEVNODE_NORMAL);
            if (apiResult != NativeMethods.CR_SUCCESS)
            {
                return false;
            }

            apiResult = CM_Reenumerate_DevNode(pdnDevInst.ToInt32(), 0, IntPtr.Zero);

            if (apiResult != NativeMethods.CR_SUCCESS)
            {
                return false;
            }

            return true;
        }

        #region TouchInjection

        /// <summary>
        ///     Initialize touch injection.
        /// </summary>
        /// <param name="maxCount">The maximum number of touch points to simulate.  Must be less than or equal to 256.</param>
        /// <param name="feedbackMode">Specifies the visual feedback mode of the generated touch points.</param>
        /// <returns>true if success.</returns>
        [DllImport("user32.dll")]
        public static extern bool InitializeTouchInjection(uint maxCount = MAX_TOUCH_POINTS, TouchFeedback feedbackMode = TouchFeedback.DEFAULT);

        /// <summary>
        ///     Inject touch input.
        /// </summary>
        /// <param name="count">The number of entries in the contacts array.</param>
        /// <param name="contacts">The POINTER_TOUCH_INFO to inject.</param>
        /// <returns>true if success.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InjectTouchInput(int count, [MarshalAs(UnmanagedType.LPArray), In] PointerTouchInfo[] contacts);

        /// <summary>
        ///     Touch feedback.
        /// </summary>
        public enum TouchFeedback
        {
            /// <summary>
            ///     Specifies default touch feedback.
            /// </summary>
            DEFAULT = 0x1,

            /// <summary>
            ///     Specifies indirect touch feedback.
            /// </summary>
            INDIRECT = 0x2,

            /// <summary>
            ///     Specifies no touch feedback.
            /// </summary>
            NONE = 0x3
        }

        /// <summary>
        ///     Contact area.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ContactArea
        {
            [FieldOffset(0)]
            public int left;
            [FieldOffset(4)]
            public int top;
            [FieldOffset(8)]
            public int right;
            [FieldOffset(12)]
            public int bottom;
        }

        /// <summary>
        ///     Touch flags.
        /// </summary>
        public enum TouchFlags
        {
            NONE = 0x00000000
        }

        /// <summary>
        ///     Touch mask.
        /// </summary>
        [Flags]
        public enum TouchMask
        {
            /// <summary>
            ///     None of the optional fields are valid (default).
            /// </summary>
            NONE = 0x00000000,

            /// <summary>
            ///     The contact area field is valid.
            /// </summary>
            CONTACTAREA = 0x00000001,

            /// <summary>
            ///     The orientation field is valid.
            /// </summary>
            ORIENTATION = 0x00000002,

            /// <summary>
            ///     The pressure field is valid.
            /// </summary>
            PRESSURE = 0x00000004
        }

        /// <summary>
        ///    Pointer flags
        /// </summary>
        [Flags]
        public enum PointerFlags
        {
            /// <summary>
            ///     None (default).
            /// </summary>
            NONE = 0x00000000,

            /// <summary>
            ///     Indicates the arrival of a new pointer.
            /// </summary>
            NEW = 0x00000001,

            /// <summary>
            ///     Indicates that this pointer continues to exist.
            /// </summary>
            /// <remarks>When this flag is not set, it indicates the pointer has left detection range.
            /// This flag is typically not set when a hovering pointer leaves detection range (PointerFlag.UPDATE is set)
            /// or when a pointer in contact with a window surface leaves detection range (PointerFlag.UP is set). </remarks>
            INRANGE = 0x00000002,

            /// <summary>
            ///     Indicates that this pointer is in contact with the digitizer surface.
            /// </summary>
            /// <remarks>When this flag is not set, it indicates a hovering pointer.</remarks>
            INCONTACT = 0x00000004,

            /// <summary>
            ///     Indicates a primary action, analogous to a mouse left button down.
            /// </summary>
            /// <remarks>A touch pointer has this flag set when it is in contact with the digitizer surface.
            /// A pen pointer has this flag set when it is in contact with the digitizer surface with no buttons pressed.
            /// A mouse pointer has this flag set when the mouse left button is down.</remarks>
            FIRSTBUTTON = 0x00000010,

            /// <summary>
            ///     Indicates a secondary action, analogous to a mouse right button down.
            /// </summary>
            /// <remarks>A touch pointer does not use this flag.
            /// A pen pointer has this flag set when it is in contact with the digitizer surface with the pen barrel button pressed.
            /// A mouse pointer has this flag set when the mouse right button is down.</remarks>
            SECONDBUTTON = 0x00000020,

            /// <summary>
            ///     Indicates a secondary action, analogous to a mouse middle button down.
            /// </summary>
            /// <remarks>A touch pointer does not use this flag.
            /// A pen pointer does not use this flag.
            /// A mouse pointer has this flag set when the mouse middle button is down.</remarks>
            THIRDBUTTON = 0x00000040,

            /// <summary>
            ///     Indicates actions of one or more buttons beyond those listed above, dependent on the pointer type.
            /// </summary>
            /// <remarks>Applications that wish to respond to these actions must retrieve information specific to the
            /// pointer type to determine which buttons are pressed.  For example, an application can determine the
            /// buttons states of a pen by calling GetPointerPenInfo and examining the flags that specify button states.</remarks>
            OTHERBUTTON = 0x00000080,

            /// <summary>
            ///     Indicates that this pointer has been designated as primary.
            /// </summary>
            /// <remarks>A primary pointer may perform actions beyond those available to non-primary pointers.
            /// For example, when a primary pointer makes contact with a window’s surface, it may provide the
            /// window an opportunity to activate by sending it a WM_POINTERACTIVATE message.</remarks>
            PRIMARY = 0x00000100,

            /// <summary>
            ///     Confidence is a suggestion from the source device about whether the pointer represents an intended or accidental
            ///     interaction.
            /// </summary>
            /// <remarks>Relevant for PT_TOUCH pointers where an accidental interaction (such as with the palm of the hand) can trigger input.
            /// The presence of this flag indicates that the source device has high confidence that this input is part of an intended interaction.</remarks>
            CONFIDENCE = 0x00000200,

            /// <summary>
            ///     Indicates that the pointer is departing in an abnormal manner, such as when the system receives invalid input for
            ///     the pointer or when a device with active pointers departs abruptly.
            /// </summary>
            /// <remarks>If the application receiving the input is in a position to do so, it should treat the interaction as not
            /// completed and reverse any effects of the concerned pointer.</remarks>
            CANCELLED = 0x00000400,

            /// <summary>
            ///     Indicates that this pointer just transitioned to a “down” state (touchdown).
            /// </summary>
            DOWN = 0x00010000,

            /// <summary>
            ///     Indicates that this information provides a simple update that does not include pointer state changes.
            /// </summary>
            UPDATE = 0x00020000,

            /// <summary>
            ///     Indicates that this pointer just transitioned to an “up” state (lift-off).
            /// </summary>
            UP = 0x00040000,

            /// <summary>
            ///     Indicates input associated with a pointer wheel.
            /// </summary>
            /// <remarks>For mouse pointers, this is equivalent to the action of the mouse scroll wheel (WM_MOUSEWHEEL).</remarks>
            WHEEL = 0x00080000,

            /// <summary>
            ///     Indicates input associated with a pointer h-wheel.
            /// </summary>
            /// <remarks>For mouse pointers, this is equivalent to the action of the mouse horizontal scroll wheel (WM_MOUSEHWHEEL).</remarks>
            HWHEEL = 0x00100000
        }

        /// <summary>
        ///     Touch point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TouchPoint
        {
            /// <summary>
            ///     The x-coordinate of the point.
            /// </summary>
            public int X;

            /// <summary>
            ///     The y-coordinate of the point.
            /// </summary>
            public int Y;
        }

        /// <summary>
        ///     Pointer input type.
        /// </summary>
        public enum PointerInputType
        {
            /// <summary>
            ///     Generic pointer type. This type never appears in pointer messages or pointer data.
            /// </summary>
            /// <remarks>Some data query functions allow the caller to restrict the query to specific pointer type.
            /// The PT_POINTER type can be used in these functions to specify that the query is to include pointers
            /// of all types.</remarks>
            POINTER = 0x00000001,

            /// <summary>
            ///     Touch pointer type.
            /// </summary>
            TOUCH = 0x00000002,

            /// <summary>
            ///     Pen pointer type.
            /// </summary>
            PEN = 0x00000003,

            /// <summary>
            ///     Mouse pointer type.
            /// </summary>
            MOUSE = 0x00000004
        }

        /// <summary>
        ///    Pointer info type
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PointerInfo
        {
            /// <summary>
            ///     A value from the PointerInputType enumeration that specifies the pointer type.
            /// </summary>
            public PointerInputType pointerType;

            /// <summary>
            ///     An identifier that uniquely identifies a pointer during its lifetime.
            /// </summary>
            /// <remarks>A pointer comes into existence when it is first detected and ends its existence
            /// when it goes out of detection range.  Note that if a physical entity (finger or pen) goes
            /// out of detection range and then returns to be detected again, it is treated as a new pointer
            /// and may be assigned a new pointer identifier.</remarks>
            public uint PointerId;

            /// <summary>
            ///     An identifier common to multiple pointers for which the source device reported an update in a single input frame.
            /// </summary>
            /// <remarks>For example, a parallel-mode multi-touch digitizer may report the positions of multiple touch contacts in a
            /// single update to the system.  Note that the frame identifier assigned as input is reported to the system for all pointers
            /// across all devices, therefore, this field may not contain strictly sequential values in a single series of messages that
            /// a window receives.  However, this field will contain the same numerical value for all input updates that were reported in
            /// the same input frame by a single device.</remarks>
            public uint FrameId;

            /// <summary>
            ///     May be any reasonable combination of flags from the Pointer Flags constants.
            /// </summary>
            public PointerFlags PointerFlags;

            /// <summary>
            ///     Handle to the source device that can be used in calls to the raw input device API and the digitizer device API.
            /// </summary>
            public IntPtr SourceDevice;

            /// <summary>
            ///     Window to which this message was targeted. If the pointer is captured, either implicitly by virtue of having
            ///     made contact over this window or explicitly using the pointer capture API, this is the capture window.
            /// </summary>
            /// <remarks>If the pointer is uncaptured, this is the window over which the pointer was when this message was generated.</remarks>
            public IntPtr WindowTarget;

            /// <summary>
            ///     Location in screen coordinates.
            /// </summary>
            public TouchPoint PtPixelLocation;

            /// <summary>
            ///     Location in device coordinates.
            /// </summary>
            public TouchPoint PtPixelLocationRaw;

            /// <summary>
            ///     Location in HIMETRIC units.
            /// </summary>
            public TouchPoint PtHimetricLocation;

            /// <summary>
            ///     Location in device coordinates in HIMETRIC units.
            /// </summary>
            public TouchPoint PtHimetricLocationRaw;

            /// <summary>
            ///     A message time stamp assigned by the system when this input was received.
            /// </summary>
            public uint Time;

            /// <summary>
            ///     Count of inputs that were coalesced into this message.
            /// </summary>
            /// <remarks>This count matches the total count of entries that can be returned by a call
            /// to GetPointerInfoHistory.  If no coalescing occurred, this count is 1 for the single
            /// input represented by the message.</remarks>
            public uint HistoryCount;

            /// <summary>
            ///     A value whose meaning depends on the nature of input.
            /// </summary>
            /// <remarks>When flags indicate PointerFlag.WHEEL, this value indicates the distance the wheel is rotated,
            /// expressed in multiples or factors of WHEEL_DELTA.  A positive value indicates that the wheel was rotated
            /// forward and a negative value indicates that the wheel was rotated backward.  When flags indicate PointerFlag.HWHEEL,
            /// this value indicates the distance the wheel is rotated, expressed in multiples or factors of WHEEL_DELTA. A positive
            /// value indicates that the wheel was rotated to the right and a negative value indicates that the wheel was rotated to
            /// the left.</remarks>
            public uint InputData;

            /// <summary>
            ///     Indicates which keyboard modifier keys were pressed at the time the input was generated.
            /// </summary>
            /// <remarks>May be zero or a combination of the following values.
            /// POINTER_MOD_SHIFT – A SHIFT key was pressed.
            /// POINTER_MOD_CTRL – A CTRL key was pressed.</remarks>
            public uint KeyStates;

            /// <summary>
            ///     Performance count.
            /// </summary>
            public ulong PerformanceCount;

            /// <summary>
            ///     Button change type.
            /// </summary>
            public PointerButtonChangeType ButtonChangeType;
        }

        /// <summary>
        ///     Pointer button change type.
        /// </summary>
        public enum PointerButtonChangeType
        {
            NONE,
            FIRSTBUTTON_DOWN,
            FIRSTBUTTON_UP,
            SECONDBUTTON_DOWN,
            SECONDBUTTON_UP,
            THIRDBUTTON_DOWN,
            THIRDBUTTON_UP,
            FOURTHBUTTON_DOWN,
            FOURTHBUTTON_UP,
            FIFTHBUTTON_DOWN,
            FIFTHBUTTON_UP
        }

        /// <summary>
        ///     Pointer touch info.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PointerTouchInfo
        {
            /// <summary>
            ///     Contains basic pointer information common to all pointer types.
            /// </summary>
            public PointerInfo PointerInfo;

            /// <summary>
            ///     Lists the touch flags.
            /// </summary>
            public TouchFlags TouchFlags;

            /// <summary>
            ///     Indicates which of the optional fields contain valid values.
            /// </summary>
            /// <remarks>The member can be zero or any combination of the values from the Touch Mask constants.</remarks>
            public TouchMask TouchMasks;

            /// <summary>
            ///     Pointer contact area in pixel screen coordinates.
            /// </summary>
            public ContactArea ContactArea;

            /// <summary>
            ///     A raw pointer contact area.
            /// </summary>
            /// <remarks>By default, if the device does not report a contact area, this field defaults to a
            /// 0-by-0 rectangle centered around the pointer location.</remarks>
            public ContactArea ContactAreaRaw;

            /// <summary>
            ///     A pointer orientation, with a value between 0 and 359, where 0 indicates a touch pointer
            /// aligned with the x-axis and pointing from left to right; increasing values indicate degrees
            /// of rotation in the clockwise direction.
            /// </summary>
            /// <remarks>This field defaults to 0 if the device does not report orientation.</remarks>
            public uint Orientation;

            /// <summary>
            ///     Pointer pressure normalized in a range of 0 to 1024.
            /// </summary>
            public uint Pressure;

            /// <summary>
            ///     Move the touch point and ContactArea.
            /// </summary>
            /// <param name="deltaX">the change in the x-value</param>
            /// <param name="deltaY">the change in the y-value</param>
            public void Move(int deltaX, int deltaY)
            {
                PointerInfo.PtPixelLocation.X += deltaX;
                PointerInfo.PtPixelLocation.Y += deltaY;
                ContactArea.left += deltaX;
                ContactArea.right += deltaX;
                ContactArea.top += deltaY;
                ContactArea.bottom += deltaY;
            }
        }

        /// <summary>
        ///     System error codes
        ///     See: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
        /// </summary>
        public enum SystemErrors
        {
            ERROR_NOT_READY = 21,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_TIMEOUT = 1460
        }

        #endregion
    }
}