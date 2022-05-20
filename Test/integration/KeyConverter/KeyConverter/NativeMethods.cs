namespace Aristocrat.Monaco.Test.KeyConverter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    ///     NativeMethods contains the user32.dll imports used with the KeyConverter.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///     Defines delegate for hook processing in this class.
        /// </summary>
        /// <param name="code">The message being received.</param>
        /// <param name="messageId">Additional information about the message.</param>
        /// <param name="hookStruct">Additional information about the message again.</param>
        /// <returns>Zero if the procedure processes the message otherwise nonzero.</returns>
        public delegate IntPtr HookProc(int code, IntPtr messageId, IntPtr hookStruct);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        /// <summary>
        ///     This is the Import for the SetWindowsHookEx function.
        ///     Use this function to install a thread-specific hook.
        /// </summary>
        /// <param name="idHook">Specifies the type of hook procedure to be installed.</param>
        /// <param name="hookPtr">Pointer to the hook procedure.</param>
        /// <param name="handle">Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
        /// <param name="threadId">Specifies the identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>The handle to the hook procedure.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SetWindowsHookEx(int idHook, IntPtr hookPtr, IntPtr handle, int threadId);

        /// <summary>
        ///     This is the Import for the UnhookWindowsHookEx function.
        ///     Call this function to uninstall the hook.
        /// </summary>
        /// <param name="idHook">Handle to the hook to be removed.</param>
        /// <returns>True if the unhook succeeds, false otherwise.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr idHook);

        /// <summary>
        ///     This is the Import for the GetKeystate function.
        ///     Call this function to get the global key state.
        /// </summary>
        /// <param name="nVirtKey">the v key.</param>
        /// <returns>returns key state.</returns>
        [DllImport("USER32.dll")]
        public static extern short GetKeyState(Keys nVirtKey);

        /// <summary>
        ///     This is the Import for the CallNextHookEx function.
        ///     Use this function to pass the hook information to the next hook procedure in chain.
        /// </summary>
        /// <param name="idHook">Handle to the current hook.</param>
        /// <param name="code">Specifies the hook code passed to the current hook procedure.</param>
        /// <param name="messageId">Specifies the messageId value passed to the current hook procedure.</param>
        /// <param name="hookStruct">Specifies the hookStruct value passed to the current hook procedure.</param>
        /// <returns>If the call succeeds, the value is nonzero, otherwise zero</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CallNextHookEx(IntPtr idHook, int code, IntPtr messageId, IntPtr hookStruct);

        /// <summary>
        ///     Gets a handle to the window in focus.
        /// </summary>
        /// <returns>Handle to the foreground window.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        ///     Gets the ProcessId of the passed Window's thread.
        /// </summary>
        /// <param name="windowHandle">Handle to the window.</param>
        /// <param name="processId">Pointer to a variable that receives the process identifier.</param>
        /// <returns>Identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);

        /// <summary>
        ///     Gets the ProcessId of the passed Window's thread.
        /// </summary>
        /// <returns>Identifier of the thread that created the window.</returns>
        [DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern int GetCurrentWin32ThreadId();

        /// <summary>
        ///     Gets the ProcessId of the passed Window's thread.
        /// </summary>
        /// <returns>Identifier of the thread that created the window.</returns>
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        /// <summary>
        ///     Gets the ProcessId of the passed Window's thread.
        /// </summary>
        /// <returns>Identifier of the thread that created the window.</returns>
        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                EnumThreadWindows(
                    thread.Id,
                    (hWnd, lParam) =>
                    {
                        handles.Add(hWnd);
                        return true;
                    },
                    IntPtr.Zero);
            }

            return handles;
        }

        /// <summary>
        ///     Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a
        ///     control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another
        ///     application.
        ///     <para>
        ///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633520%28v=vs.85%29.aspx  for more
        ///         information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">
        ///     C++ ( hWnd [in]. Type: HWND )<br />A <see cref="IntPtr" /> handle to the window or control containing the text.
        /// </param>
        /// <param name="lpString">
        ///     C++ ( lpString [out]. Type: LPTSTR )<br />The <see cref="System.Text.StringBuilder" /> buffer that will receive the
        ///     text. If
        ///     the string is as long or longer than the buffer, the string is truncated and terminated with a null character.
        /// </param>
        /// <param name="nMaxCount">
        ///     C++ ( nMaxCount [in]. Type: int )<br /> Should be equivalent to
        ///     <see cref="System.Text.StringBuilder.Length" /> after call returns. The <see cref="int" /> maximum number of
        ///     characters to copy
        ///     to the buffer, including the null character. If the text exceeds this limit, it is truncated.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the length, in characters, of the copied string, not including
        ///     the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window
        ///     or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.<br />
        ///     This function cannot retrieve the text of an edit control in another application.
        /// </returns>
        /// <remarks>
        ///     If the target window is owned by the current process, GetWindowText causes a WM_GETTEXT message to be sent to the
        ///     specified window or control. If the target window is owned by another process and has a caption, GetWindowText
        ///     retrieves the window caption text. If the window does not have a caption, the return value is a null string. This
        ///     behavior is by design. It allows applications to call GetWindowText without becoming unresponsive if the process
        ///     that owns the target window is not responding. However, if the target window is not responding and it belongs to
        ///     the calling application, GetWindowText will cause the calling application to become unresponsive. To retrieve the
        ///     text of a control in another process, send a WM_GETTEXT message directly instead of calling GetWindowText.<br />For
        ///     an example go to
        ///     <see cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644928%28v=vs.85%29.aspx#sending">
        ///         Sending a
        ///         Message.
        ///     </see>
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        ///     Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar). If
        ///     the specified window is a control, the function retrieves the length of the text within the control. However,
        ///     GetWindowTextLength cannot retrieve the length of the text of an edit control in another application.
        ///     <para>
        ///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633521%28v=vs.85%29.aspx for more
        ///         information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A <see cref="IntPtr" /> handle to the window or control.</param>
        /// <returns>
        ///     If the function succeeds, the return value is the length, in characters, of the text. Under certain
        ///     conditions, this value may actually be greater than the length of the text.<br />For more information, see the
        ///     following Remarks section. If the window has no text, the return value is zero.To get extended error information,
        ///     call GetLastError.
        /// </returns>
        /// <remarks>
        ///     If the target window is owned by the current process, <see cref="GetWindowTextLength" /> causes a
        ///     WM_GETTEXTLENGTH message to be sent to the specified window or control.<br />Under certain conditions, the
        ///     <see cref="GetWindowTextLength" /> function may return a value that is larger than the actual length of the
        ///     text.This occurs with certain mixtures of ANSI and Unicode, and is due to the system allowing for the possible
        ///     existence of double-byte character set (DBCS) characters within the text. The return value, however, will always be
        ///     at least as large as the actual length of the text; you can thus always use it to guide buffer allocation. This
        ///     behavior can occur when an application uses both ANSI functions and common dialogs, which use Unicode.It can also
        ///     occur when an application uses the ANSI version of <see cref="GetWindowTextLength" /> with a window whose window
        ///     procedure is Unicode, or the Unicode version of <see cref="GetWindowTextLength" /> with a window whose window
        ///     procedure is ANSI.<br />For more information on ANSI and ANSI functions, see Conventions for Function Prototypes.
        ///     <br />To obtain the exact length of the text, use the WM_GETTEXT, LB_GETTEXT, or CB_GETLBTEXT messages, or the
        ///     GetWindowText function.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            var length = GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        ///     Determines the visibility state of the specified window.
        ///     <para>
        ///         Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633530%28v=vs.85%29.aspx for more
        ///         information. For WS_VISIBLE information go to
        ///         https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600%28v=vs.85%29.aspx
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window to be tested.</param>
        /// <returns>
        ///     <c>true</c> or the return value is nonzero if the specified window, its parent window, its parent's parent
        ///     window, and so forth, have the WS_VISIBLE style; otherwise, <c>false</c> or the return value is zero.
        /// </returns>
        /// <remarks>
        ///     The visibility state of a window is indicated by the WS_VISIBLE[0x10000000L] style bit. When
        ///     WS_VISIBLE[0x10000000L] is set, the window is displayed and subsequent drawing into it is displayed as long as the
        ///     window has the WS_VISIBLE[0x10000000L] style. Any drawing to a window with the WS_VISIBLE[0x10000000L] style will
        ///     not be displayed if the window is obscured by other windows or is clipped by its parent window.
        /// </remarks>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
    }
}