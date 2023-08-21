namespace TestExtension1
{
    using System;
    using System.Reflection;
    using Aristocrat.Monaco.Kernel;
    using log4net;

    /// <summary>
    ///     An IRunnable implementation that is not disposable and has a run method that
    ///     just returns immediately.
    /// </summary>
    public class TestRunnableNoDispose : IRunnable
    {
        /// <summary>
        ///     Create a logger for use in this class
        /// </summary>
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     The number of Initialize, Run and Stop method calls on this object.
        /// </summary>
        private int m_calls;

        /// <summary>
        ///     Initializes a new instance of the TestRunnableNoDispose class.
        /// </summary>
        public TestRunnableNoDispose()
        {
            RunState = RunnableState.Uninitialized;
            m_log.Debug("Constructed");
        }

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
        ///     Gets the current run state of the object.
        /// </summary>
        public RunnableState RunState { get; private set; }

        public TimeSpan Timeout => TimeSpan.FromSeconds(10);

        /// <summary>
        ///     Initializes the runnable.
        /// </summary>
        public void Initialize()
        {
            RunState = RunnableState.Initialized;
            InitializeOrder = ++m_calls;
            m_log.DebugFormat("Initialize() called, order = {0}", InitializeOrder);
        }

        /// <summary>
        ///     Starts the runnable.
        /// </summary>
        public void Run()
        {
            RunState = RunnableState.Running;
            RunOrder = ++m_calls;
            m_log.DebugFormat("Run() called, order = {0}", RunOrder);

            RunState = RunnableState.Stopped;
            m_log.Debug("Run() returning");
        }

        /// <summary>
        ///     Stops the runnable.
        /// </summary>
        public void Stop()
        {
            StopOrder = ++m_calls;
            m_log.DebugFormat("Stop() called, order = {0}", StopOrder);
        }
    }
}