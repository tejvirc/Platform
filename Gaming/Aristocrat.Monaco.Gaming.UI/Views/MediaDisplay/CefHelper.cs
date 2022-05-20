namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using System;
    using System.Reflection;
    using System.Windows;
    using CefSharp;
    using CefSharp.Wpf;
    using Kernel;
    using log4net;

    public static class CefHelper
    {
        private const string CachePath = @"/BrowserCache";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Initialize()
        {
            if (Cef.IsInitialized)
            {
                return;
            }

            Logger.Info("Initializing CEF");

            var directory = ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(CachePath);

            Cef.EnableHighDPISupport();

            var settings = new CefSettings
            {
#if !(RETAIL)
                LogSeverity = LogSeverity.Error,
                LogFile = "..\\logs\\cef_debug.log",
#else // The CEF log is not whitelisted and therefore must be disabled for retail builds
//  We can whitelist it, but it grows without bounds (which is bad)
                LogSeverity = LogSeverity.Disable,
#endif
                //Locale = ViewModel.ActiveLocaleCode, // Is this needed?
                CachePath = directory.FullName,
                IgnoreCertificateErrors = true
            };

            // This should result in better off screen rendering performance and the expense of disabling WebGL (which is currently not required)
            settings.SetOffScreenRenderingBestPerformanceArgs();

            // disable-gpu and disable-gpu-compositing are added by SetOffScreenRenderingBestPerformanceArgs. disable-gpu-vsync is not
            if (!settings.CefCommandLineArgs.ContainsKey("disable-gpu"))
            {
                settings.CefCommandLineArgs.Add("disable-gpu", "1");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("disable-gpu-vsync"))
            {
                settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("disable-gpu-compositing"))
            {
                settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            }

            try
            {
                var initialized = false;
                Application.Current.Dispatcher.Invoke(() => initialized = Cef.Initialize(settings, true, (IBrowserProcessHandler)null));

                Logger.Info($"CEF Initialized={initialized}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing CEF", ex);
            }
        }

        public static void Shutdown()
        {
            if (!Cef.IsInitialized)
            {
                return;
            }

            Logger.Debug("Shutting down CEF");

            Cef.Shutdown();
        }
    }
}