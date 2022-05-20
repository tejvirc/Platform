namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Threading;

    /// <summary>
    ///     Provides a event wait handle for the installers.
    /// </summary>
    public class InstallerEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="waitHandle">Event wait handle</param>
        public InstallerEventArgs(EventWaitHandle waitHandle)
        {
            WaitHandle = waitHandle;
        }

        /// <summary>
        ///     Gets the event wait handle.
        /// </summary>
        public EventWaitHandle WaitHandle { get; }
    }
}