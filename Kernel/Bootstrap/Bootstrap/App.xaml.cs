namespace Aristocrat.Monaco.Bootstrap
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    ///     The top-level Application object which contains the Main Window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        ///     Application entry point. This is the WPF equivalent a console application's
        ///     <code>
        ///         static int void Main(string[] args)
        ///     </code>
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // The Application is now running
            NativeMethods.SetErrorMode(
                NativeMethods.ErrorModes.SemFailCriticalErrors |
                NativeMethods.ErrorModes.SemNoGpFaultErrorBox);

            // Starts the boot-up sequence on a long running Task thread
            Task.Factory.StartNew(() =>
                Bootstrap.Run(e.Args),
                TaskCreationOptions.LongRunning);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Handle render thread exception UCEERR_RENDERTHREADFAILURE during display disconnect.
            if (e.Exception is COMException commException && (uint)commException.HResult == 0x88980406)
            {
                e.Handled = true;
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            e.ApplicationExitCode = Environment.ExitCode;
        }
    }
}