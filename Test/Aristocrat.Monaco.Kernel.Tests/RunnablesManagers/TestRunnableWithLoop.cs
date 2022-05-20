namespace TestExtension1
{
    using System.Reflection;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using log4net;

    /// <summary>
    ///     A disposable IRunnable implementation with a run method that blocks until stop is called.
    /// </summary>
    public class TestRunnableWithLoop : TestRunnableNoLoop
    {
        /// <summary>
        ///     Create a logger for use in this class
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Starts the runnable.
        /// </summary>
        protected override void OnRun()
        {
            base.OnRun();

            while (RunState == RunnableState.Running)
            {
                Thread.Sleep(0);
            }

            Log.Debug("OnRun() returning");
        }
    }
}