namespace Lobby
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        protected override void OnStartup(StartupEventArgs e)
        {
            Debug.WriteLine(string.Join(" ", e.Args));

            var hostMainHandle = new IntPtr(Convert.ToInt64(e.Args[1], 16));

            var host = new PlatformHost(hostMainHandle);

            var mainWindow = new Main();

            var mainHandle = new WindowInteropHelper(mainWindow).EnsureHandle();

            SetParent(mainHandle, host.Handle);

            mainWindow.Show();
        }
    }
}
