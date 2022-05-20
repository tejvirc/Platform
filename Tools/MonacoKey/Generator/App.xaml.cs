namespace Generator
{
    using Common.Utils;
    using log4net;
    using log4net.Config;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Validator.Log = Log;
            Detector.Log = Log;
            Log.Debug("Application starting...");

            if (Environment.Is64BitOperatingSystem)
                Log.Debug("Operating System is 64 bit. :)");
            else
                Log.Debug("Operating System is 32 bit. :(");

            if (Environment.Is64BitProcess)
                Log.Debug("Platform Key Generator is 64 bit. :)");
            else
                Log.Debug("Platform Key Generator is 32 bit. :(");

            ScriptRunner sr = new ScriptRunner(Log);
            sr.StopFileExplorerPopup();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ScriptRunner sr = new ScriptRunner(Log);
            sr.StartFileExplorerPopup();
        }
    }

}
