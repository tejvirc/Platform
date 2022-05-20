namespace Aristocrat.Monaco.Kernel
{
    using System.Threading;

    /// <summary>
    ///     Definition of the RunnableData class, which is used by BootExtender to store
    ///     information about the IRunnable objects it has started and therefore must
    ///     stop during shutdown.
    /// </summary>
    public class RunnableData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RunnableData" /> class.
        /// </summary>
        /// <param name="runnable">A reference to an IRunnable object</param>
        /// <param name="thread">A reference to the thread in which the runnable object is executing.</param>
        public RunnableData(IRunnable runnable, Thread thread)
        {
            Runnable = runnable;
            Thread = thread;
        }

        /// <summary>
        ///     Gets or sets a reference to an IRunnable object.
        /// </summary>
        public IRunnable Runnable { get; set; }

        /// <summary>
        ///     Gets or sets a reference to the thread in which the runnable object is executing.
        /// </summary>
        public Thread Thread { get; set; }
    }
}