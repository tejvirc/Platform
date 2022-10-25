namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    ///     NativeMethods contains the user32.dll imports used with the KeyConverter.
    /// </summary>
    [CLSCompliant(false)]
    public static class NativeMethods
    {
        /// <summary>
        ///     Defines the error codes for <see cref="SetErrorMode" />
        /// </summary>
        [Flags]
        public enum ErrorModes : uint
        {
            /// <summary>
            ///     System default
            /// </summary>
            SystemDefault = 0x0,

            /// <summary>
            /// Sem Fail Critical Errors
            /// </summary>
            SemFailCriticalErrors = 0x0001,

            /// <summary>
            /// Sem No Alignment Fault Except
            /// </summary>
            SemNoAlignmentFaultExcept = 0x0004,

            /// <summary>
            /// Sem No Gp Fault Error Box
            /// </summary>
            SemNoGpFaultErrorBox = 0x0002,
            /// <summary>
            /// Sem No Open File Error Box
            /// </summary>
            SemNoOpenFileErrorBox = 0x8000
        }

        /// <summary>
        ///     left
        /// </summary>
        public const int LeftDown = 0x02;

        /// <summary>
        ///     left up
        /// </summary>
        public const int LeftUp = 0x04;

        /// <summary>
        ///     down
        /// </summary>
        public const int RightDown = 0x08;

        /// <summary>
        ///     right up
        /// </summary>
        public const int RightUp = 0x10;

        /// <summary>
        ///     Move
        /// </summary>
        public const int Move = 0x01;

        /// <summary>
        ///     Absolute
        /// </summary>
        public const int Absolute = 0x8000;

        /// <summary>
        ///     Sets Window Z-Order to the top
        /// </summary>
        /// <param name="window"></param>
        public static void BringWindowToTop(Window window)
        {
            SetWindowZOrder(window, WindowPosition.Top);
        }

        /// <summary>
        ///     Sets Window Z-Order to the back.
        /// </summary>
        /// <param name="window"></param>
        public static void SendWindowToBack(Window window)
        {
            SetWindowZOrder(window, WindowPosition.Bottom);
        }

        /// <summary>
        ///     Sets the current error mode
        /// </summary>
        /// <param name="uMode"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern ErrorModes SetErrorMode(ErrorModes uMode);

        /// <summary>
        ///     mouse_event
        /// </summary>
        /// <param name="dwFlags">Flags</param>
        /// <param name="dx">dx</param>
        /// <param name="dy">dy</param>
        /// <param name="dwData">Data</param>
        /// <param name="dwExtraInfo">Extra info</param>
        [DllImport("user32.dll")]
        internal static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        /// <summary>
        ///     The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system
        ///     resources
        ///     associated with the object. After the object is deleted, the specified handle is no longer valid.
        /// </summary>
        /// <param name="ho">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("gdi32.dll")]
        internal static extern int DeleteObject(IntPtr ho);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(
            IntPtr hWnd,
            int hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int wFlags);

        private static void SetWindowZOrder(Window window, WindowPosition position)
        {
            var wih = new WindowInteropHelper(window);
            var hWnd = wih.Handle;

            SetWindowPos(
                hWnd,
                (int)position,
                0,
                0,
                0,
                0,
                (int)(SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE));
        }

        private enum WindowPosition
        {
            NotTopMost = -2,
            TopMost = -1,
            Top = 0,
            Bottom = 1
        }

        [Flags]
        private enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002
        }
    }
}