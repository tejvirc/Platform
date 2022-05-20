namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// TouchWindowHelper class holds all implementation for altering a window to send WM_Touch messages after the WPF Stylus support is removed.
    /// </summary>
    public class TouchWindowHelper
    {
        private readonly Dictionary<uint, TouchDeviceEmulator> _devices = new Dictionary<uint, TouchDeviceEmulator>();
        private Window _window;
        private bool _disposed;

        /// <summary>
        /// TouchWindowHelper consolidates the functionality to restore Touch gestures after disabling WPF Stylus support.
        /// This will be used by both TouchWindow and TouchNavigationWindow
        /// </summary>
        /// <param name="window"></param>
        public TouchWindowHelper(Window window)
        {
            _window = window;
            _window.SourceInitialized += WindowOnSourceInitialized;
        }

        private void WindowOnSourceInitialized(object sender, EventArgs e)
        {
            if (PresentationSource.FromVisual(_window) is HwndSource source)
            {
                source.AddHook(WndProc);

                var presentationSource = (HwndSource)PresentationSource.FromDependencyObject(_window);
                if (presentationSource == null)
                {
                    throw new Exception("Unable to find the parent element host.");
                }

                WindowsServices.RegisterTouchWindow(presentationSource.Handle, WindowsServices.TouchWindowFlag.WantPalm);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            if (msg == WindowsServices.WM_TOUCH) //WM_TOUCH
            {
                handled = HandleTouch(wParam, lParam);
                return new IntPtr(1);
            }

            return IntPtr.Zero;
        }

        private bool HandleTouch(IntPtr wParam, IntPtr lParam)
        {
            var inputCount = wParam.ToInt32() & 0xffff;
            var inputs = new WindowsServices.TOUCHINPUT[inputCount];
            if (!WindowsServices.GetTouchInputInfo(lParam, inputCount, inputs))
            {
                return false;
            }

            foreach (var input in inputs)
            {
                var position = _window.PointFromScreen(new Point(input.x * 0.01, input.y * 0.01));
                if (!_devices.TryGetValue(input.dwID, out var device))
                {
                    device = new TouchDeviceEmulator((int)input.dwID, input.HSource);
                    _devices.Add(input.dwID, device);
                }

                device.Position = position;
                if ((input.dwFlags & WindowsServices.TOUCHEVENTF_DOWN) > 0)
                {
                    device.SetActiveSource(PresentationSource.FromVisual(_window));
                    device.Activate();
                    device.ReportDown();
                }
                else if (device.IsActive && (input.dwFlags & WindowsServices.TOUCHEVENTF_UP) > 0)
                {
                    device.ReportUp();
                    device.Deactivate();
                    _devices.Remove(input.dwID);
                }
                else if (device.IsActive && (input.dwFlags & WindowsServices.TOUCHEVENTF_MOVE) > 0)
                {
                    device.ReportMove();
                }
            }

            WindowsServices.CloseTouchInputHandle(lParam);
            return true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing)
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (PresentationSource.FromDependencyObject(_window) is HwndSource source)
                {
                    WindowsServices.UnregisterTouchWindow(source.Handle);
                }

                _window.SourceInitialized -= WindowOnSourceInitialized;
                _window = null;
            }

            _disposed = true;
        }
    }
}
