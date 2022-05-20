namespace Platform.Launcher.Utilities
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ShellProgressBar : IDisposable, IProgress<double>
    {
        private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);

        private readonly IntPtr _handle = IntPtr.Zero;
        private readonly Timer _timer;

        private NativeMethods.COPYDATASTRUCT _copyDataStruct;
        private IntPtr _copyDataStructPtr = IntPtr.Zero;
        private double _currentProgress;
        private bool _disposed;

        public ShellProgressBar(int id = 0)
        {
            _copyDataStruct.dwData = id;

            _handle = NativeMethods.FindWindow("ShellWindow", null);
            _copyDataStructPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_copyDataStruct));

            _timer = new Timer(TimerHandler);
            ResetTimer();
            SendProgress(0);
        }

        public double CurrentProgress => _currentProgress;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref _currentProgress, value);
        }

        ~ShellProgressBar()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                using (var handle = new ManualResetEvent(false))
                {
                    if (_timer.Dispose(handle))
                    {
                        handle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                }

                SendProgress(100);
            }

            Marshal.FreeHGlobal(_copyDataStructPtr);
            _copyDataStructPtr = IntPtr.Zero;

            // no need to close or zero HWND handle
            _disposed = true;
        }

        private void SendProgress(int progress)
        {
            if (_handle == IntPtr.Zero)
            {
                return;
            }

            var progressString = progress.ToString();

            var pinnedStrPtr = Marshal.StringToCoTaskMemAnsi(progressString);
            var pinned = GCHandle.Alloc(progressString, GCHandleType.Pinned);
            {
                _copyDataStruct.cbData = progressString.Length + 1;
                _copyDataStruct.lpData = pinnedStrPtr;
            }

            Marshal.StructureToPtr(_copyDataStruct, _copyDataStructPtr, false);
            NativeMethods.SendMessage(_handle, NativeMethods.WM_COPYDATA, IntPtr.Zero, _copyDataStructPtr);

            Marshal.FreeCoTaskMem(pinnedStrPtr);

            _copyDataStruct.lpData = IntPtr.Zero;
            _copyDataStruct.cbData = 0;
        }


        private void TimerHandler(object state)
        {
            if (_disposed)
            {
                return;
            }

            var percent = (int) (_currentProgress * 100);
            SendProgress(percent);
            ResetTimer();
        }

        private void ResetTimer()
        {
            try
            {
                _timer.Change(_animationInterval, Timeout.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal class NativeMethods
        {
            public const int WM_COPYDATA = 0x004A;

            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);

            [DllImport("user32.dll", CharSet = CharSet.Ansi)]
            public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct COPYDATASTRUCT
            {
                public long dwData;
                public int cbData;
                public IntPtr lpData;
            }
        }
    }
}