namespace Executor
{
    using Common.Utils;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] CommandLineParams;

        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CommandLineParams = e.Args;
            Detector.Log = Log;
            Validator.Log = Log;

            Log.Debug("Executor Application Starting: " + DateTime.Now);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log.Debug("------------------------------------------ Executor Application Exit ------------------------------------------" + Environment.NewLine + Environment.NewLine);
        }
    }

}
