namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     The state of a Runnable
    /// </summary>
    public enum RunnableState
    {
        /// <summary>The state the Runnable is in before its Initialize method is called</summary>
        Uninitialized,

        /// <summary>The state the Runnable is in after its Initialize method is called</summary>
        Initializing,

        /// <summary>The state the Runnable is in after its Initialize has finished executing</summary>
        Initialized,

        /// <summary>The state the Runnable is in after its Run method is called</summary>
        Running,

        /// <summary>The state the Runnable is in after its Stop method is called if not already in the Stopped state</summary>
        Stopping,

        /// <summary>The state the Runnable is in after its Run method has returned</summary>
        Stopped
    }
}