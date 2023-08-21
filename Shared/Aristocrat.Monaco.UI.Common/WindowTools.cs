namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using System.Windows.Forms;

    /// <summary>
    ///     Window Helper methods
    ///     TODO: Tech Debt - This class is outdated and should be merged into WindowToScreenMapper.cs
    /// </summary>
    public static class WindowTools
    {
        /// <summary>
        ///     Moves the Window to the Primary Screen
        /// </summary>
        /// <param name="window">The Window to move</param>
        /// <param name="fullscreen">If true, Window will be set to fullscreen</param>
        public static void AssignWindowToPrimaryScreen(Window window, bool fullscreen)
        {
            if (window != null && Screen.PrimaryScreen != null)
            {
                window.Top = Screen.PrimaryScreen.WorkingArea.Top;
                window.Left = Screen.PrimaryScreen.WorkingArea.Left;

                // This is pseudo fullscreen. Use 'Window.WindowState = WindowState.Maximized' for normal OS full screen
                if (fullscreen)
                {
                    window.Width = Screen.PrimaryScreen.WorkingArea.Width;
                    window.Height = Screen.PrimaryScreen.WorkingArea.Height;
                }
            }
        }

        /// <summary>
        ///     Use this method to remove Windows touch cross hair. (Windows Class must provide NULL Cursor, or in case of WPF)
        ///     Window.Cursor = System.Windows.Input.Cursors.None = null;
        ///     Warning: very hacky
        /// </summary>
        public static void RemoveTouchCursor()
        {
            NativeMethods.mouse_event(NativeMethods.Move, 1, 1, 0, UIntPtr.Zero);
        }
    }
}