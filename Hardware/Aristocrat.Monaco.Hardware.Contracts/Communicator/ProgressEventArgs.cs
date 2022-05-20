namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;

    /// <summary>Additional information for progress events.</summary>
    /// <seealso cref="T:System.EventArgs" />
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Contracts.Communicator.ProgressEventArgs class.
        /// </summary>
        /// <param name="progress">The progress.</param>
        public ProgressEventArgs(int progress)
        {
            Progress = progress;
        }

        /// <summary>Gets the progress.</summary>
        /// <value>The progress.</value>
        public int Progress { get; }
    }
}