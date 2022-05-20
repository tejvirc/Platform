namespace Aristocrat.Monaco.UI.Common
{
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Threading;
    using log4net;

    /// <summary>
    ///     Disables application crash messages
    /// </summary>
    public class MonacoApplication : Application
    {
        /// <summary>
        ///     Logger
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="MonacoApplication" /> class.
        /// </summary>
        public MonacoApplication()
        {
            NativeMethods.SetErrorMode(
                NativeMethods.ErrorModes.SemFailCriticalErrors | NativeMethods.ErrorModes.SemNoGpFaultErrorBox);
            Startup += MonacoApplication_Startup;
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception);
            // Handle render thread exception UCEERR_RENDERTHREADFAILURE during display disconnect.
            if (e.Exception is COMException commException && (uint)commException.HResult == 0x88980406)
            {
                e.Handled = true;
            }
        }

        private void MonacoApplication_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
    }
}