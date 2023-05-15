namespace Stubs
{
    using System;
    using System.Reflection;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using log4net;

    public class SimpleRunnable : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnInitialize()
        {
            Logger.Info("Initialized");
        }

        protected override void OnRun()
        {
            Logger.Info("Run started");

            Console.WriteLine("Press <q> to quit");
            while (true)
            {
                // get input
                if (Console.ReadKey().KeyChar == 'q')
                {
                    break;
                }
            }

            Console.WriteLine();
            Logger.Info("Stopped");
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping");
        }
    }
}
