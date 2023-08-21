namespace Aristocrat.Monaco.Application.UI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     A struct to store a window's rect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        /// <summary>
        ///     The left postion.
        /// </summary>
        public int Left;

        /// <summary>
        ///     The top postion.
        /// </summary>
        public int Top;

        /// <summary>
        ///     The right postion.
        /// </summary>
        public int Right;

        /// <summary>
        ///     The bottom postion.
        /// </summary>
        public int Bottom;
    }

    /// <summary>
    ///     A static helper class for native P/Invoke calls.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///     Identification code of Yes (OK) button.
        /// </summary>
        public const int YesId = 0x06;

        /// <summary>
        ///     The command code used to send a notification.
        /// </summary>
        public const uint CommandCode = 0x0111;

        /// <summary>
        ///     Gets the window's rect.
        /// </summary>
        /// <param name="hwnd">The hanlde of window.</param>
        /// <param name="rc">The resultant rect.</param>
        /// <returns>The status of operation.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out Rect rc);

        /// <summary>
        ///     Sends a message to an external window.
        /// </summary>
        /// <param name="windowHandle">The handle of target window.</param>
        /// <param name="messageId">The well-known message code.</param>
        /// <param name="messageParam1">The first message parameter.</param>
        /// <param name="messageParam2">The second message parameter.</param>
        /// <returns>The status code.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            IntPtr windowHandle,
            uint messageId,
            IntPtr messageParam1,
            IntPtr messageParam2);
    }
}