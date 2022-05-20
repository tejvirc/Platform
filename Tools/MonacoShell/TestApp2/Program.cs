using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestApp2
{
    class ShellProgressBar : IDisposable
    {
        private readonly IntPtr handle = IntPtr.Zero;
        private  NativeMethods.COPYDATASTRUCT copyDataStruct = new NativeMethods.COPYDATASTRUCT();
        private  IntPtr copyDataStructPtr = IntPtr.Zero;
        int _progress = 0;
        public ShellProgressBar(int Id = 0)
        {
            copyDataStruct.dwData = Id;
            handle = NativeMethods.FindWindow("ShellWindow", null);
            copyDataStructPtr = Marshal.AllocHGlobal(Marshal.SizeOf(copyDataStruct));
        }
        public bool Connected  {get { return handle != IntPtr.Zero;} }

        public void SendProgress(int progress)
        {
            if(handle == IntPtr.Zero)
                return;

            // Clamp 0->100
            if(progress < 0)
                _progress = 0;
            else if (progress > 100)
                _progress = 100;
            else
                _progress = progress;

            string progressString = _progress.ToString();

            IntPtr pinnedStrPtr = Marshal.StringToCoTaskMemAnsi(progressString);
            var pinned = GCHandle.Alloc(progressString, GCHandleType.Pinned);
            {
                copyDataStruct.cbData = progressString.Length + 1;
                copyDataStruct.lpData = pinnedStrPtr;
            }

            Marshal.StructureToPtr(copyDataStruct, copyDataStructPtr, false);
            NativeMethods.SendMessage(handle,NativeMethods.WM_COPYDATA,IntPtr.Zero, copyDataStructPtr);

            Marshal.FreeCoTaskMem(pinnedStrPtr);
            copyDataStruct.lpData = IntPtr.Zero;
            copyDataStruct.cbData = 0;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SendProgress(100);
                }

                Marshal.FreeHGlobal(copyDataStructPtr);
                copyDataStructPtr = IntPtr.Zero;
                // no need to close or zero HWND handle
                disposedValue = true;
            }
        }

         ~ShellProgressBar()
         {
            Dispose(false);
         }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        internal class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct COPYDATASTRUCT
            {
                public Int64 dwData;
                public int cbData;
                public IntPtr lpData;
            }

            public const int WM_COPYDATA = 0x004A;
            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);

            [DllImport("user32.dll",CharSet=CharSet.Ansi)]
            public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var progressBar0 = new ShellProgressBar(0);
            var progressBar1 = new ShellProgressBar(1);
            if (!progressBar0.Connected)
            {
                Console.WriteLine("FindWindow failed to find window of Class 'ShellWindow'");
                Console.Out.Flush();
            }

            for (int x = 0; x < 3; ++x)
            {
                Console.WriteLine("Working...");
                Console.Out.Flush();
                System.Threading.Thread.Sleep(200);
            }

            progressBar0.SendProgress(0);

            for (int x = 1; x < 5; ++x)
            {
                Console.WriteLine($"Loading module # {x} of 4...");
                Console.Out.Flush();
                progressBar1.SendProgress(0);
                for (int i = 1; i < 101; ++i)
                {
                    progressBar1.SendProgress(i);
                }
                progressBar0.SendProgress(x*25);
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine("Module loading complete.");
            Console.Out.Flush();
        }
    }
}
