namespace TestExtension1
{
    using System.Reflection;
    using Aristocrat.Monaco.Kernel;
    using log4net;

    /// <summary>
    ///     A disposable IRunnable implementation with a run method that just returns immediately.
    /// </summary>
    public class TestRunnableNoLoop : BaseRunnable
    {
        /// <summary>
        ///     Create a logger for use in this class
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     The number of Initialize, Run and Stop method calls on this object.
        /// </summary>
        private int m_calls;

        /// <summary>
        ///     Gets a 1-based value indicating the order in which Initialize() was called.
        ///     A zero means it was not called.
        /// </summary>
        public int InitializeOrder { get; private set; }

        /// <summary>
        ///     Gets a 1-based value indicating the order in which Run() was called.
        ///     A zero means it was not called.
        /// </summary>
        public int RunOrder { get; private set; }

        /// <summary>
        ///     Gets a 1-based value indicating the order in which Stop() was called.
        ///     A zero means it was not called.
        /// </summary>
        public int StopOrder { get; private set; }

        /// <summary>
        ///     Gets a 1-based value indicating the order in which Dispose() was called.
        ///     A zero means it was not called.
        /// </summary>
        public int DisposeOrder { get; private set; }

        /// <summary>
        ///     Called when the runnable is initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            InitializeOrder = ++m_calls;
            Log.DebugFormat("OnInitialize() called, order = {0}", InitializeOrder);
        }

        /// <summary>
        ///     Called when the runnable is run.
        /// </summary>
        /// <remarks>This runnable does not implement a run loop.</remarks>
        protected override void OnRun()
        {
            RunOrder = ++m_calls;
            Log.DebugFormat("OnRun() called, order = {0}", RunOrder);
        }

        /// <summary>
        ///     Called when the runnable is stopped.
        /// </summary>
        protected override void OnStop()
        {
            StopOrder = ++m_calls;
            Log.DebugFormat("OnStop() called, order = {0}", StopOrder);
        }

        /// <summary>
        ///     Releases resources held by this object.
        /// </summary>
        /// <param name="disposing">true if called from user code; false if called from destructor.</param>
        protected override void Dispose(bool disposing)
        {
            DisposeOrder = ++m_calls;
            Log.DebugFormat("Dispose() called, order = {0}", DisposeOrder);

            base.Dispose(disposing);

            Log.Debug("Dispose() returning");
        }
    }
}