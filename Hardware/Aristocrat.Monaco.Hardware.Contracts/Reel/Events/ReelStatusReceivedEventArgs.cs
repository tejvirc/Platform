namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The reel status received event arguments.
    /// </summary>
    public class ReelStatusReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructs a new instance of the ReelStatusReceivedEventArgs class.
        /// </summary>
        /// <param name="statuses">The collection of statuses.</param>
        public ReelStatusReceivedEventArgs(IEnumerable<ReelStatus> statuses)
        {
            Statuses = statuses;
        }

        /// <summary>
        ///     Get the statuses.
        /// </summary>
        public IEnumerable<ReelStatus> Statuses { get; }
    }
}
