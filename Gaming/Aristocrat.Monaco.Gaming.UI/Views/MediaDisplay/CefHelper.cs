namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using CefSharp;
    using CefSharp.Wpf;
    using Kernel;
    using log4net;

    public static class CefHelper
    {
        private const string CachePath = @"/BrowserCache";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public static void Initialize()
        {
            if (Cef.IsInitialized)
            {
                return;
            }

            Logger.Info("Initializing CEF");

            var directory = ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(CachePath);
            try
            {
                if (Directory.Exists(directory.FullName))
                {
                    Directory.Delete(directory.FullName, true);
                }
            }
            catch (IOException e)
            {
                Logger.Error("Failed to delete the cache folder", e);
            }

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
                CachePath = directory.FullName,
                WindowlessRenderingEnabled = true,
                IgnoreCertificateErrors = true
            };

            //disable - gpu and disable-gpu - compositing are added by SetOffScreenRenderingBestPerformanceArgs. disable - gpu - vsync is not
            settings.DisableGpuAcceleration();
            settings.SetOffScreenRenderingBestPerformanceArgs();

            if (!settings.CefCommandLineArgs.ContainsKey("disable-gpu-vsync"))
            {
                settings.CefCommandLineArgs.Add("disable-gpu-vsync"); //Disable Vsync
            }

            if (!settings.CefCommandLineArgs.ContainsKey("force-renderer-accessibility"))
            {
                settings.CefCommandLineArgs.Add("force-renderer-accessibility");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("disable-renderer-accessibility"))
            {
                settings.CefCommandLineArgs.Add("disable-renderer-accessibility");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("allow-running-insecure-content"))
            {
                settings.CefCommandLineArgs.Add("allow-running-insecure-content");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("renderer-startup-dialog"))
            {
                settings.CefCommandLineArgs.Add("renderer-startup-dialog");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("disable-site-isolation-trials"))
            {
                settings.CefCommandLineArgs.Add("disable-site-isolation-trials");
            }

            if (!settings.CefCommandLineArgs.ContainsKey("enable-media-stream"))
            {
                settings.CefCommandLineArgs.Add("enable-media-stream");
            }

            try
            {
                var initialized = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    initialized = Cef.Initialize(settings, true, (IBrowserProcessHandler)null);
                });

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