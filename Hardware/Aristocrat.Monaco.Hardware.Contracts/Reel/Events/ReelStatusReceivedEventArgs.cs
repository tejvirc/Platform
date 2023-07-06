namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The reel status received event arguments.
    /// </summary>
    public class ReelStatusReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructs a new instance of the ReelStatusReceivedEventArgs class.
        /// </summary>
        /// <param name="statuses">The collection of statuses.</param>
        public ReelStatusReceivedEventArgs(params ReelStatus[] statuses)
        {
            Statuses = statuses;
        }

        /// <summary>
        ///     Constructs a new instance of the ReelStatusReceivedEventArgs class.
        /// </summary>
        /// <param name="statuses">The collection of statuses.</param>
        public ReelStatusReceivedEventArgs(IEnumerable<ReelStatus> statuses)
        {
            Statuses = statuses.ToArray();
        }

        /// <summary>
        ///     Get the statuses.
        /// </summary>
        public ICollection<ReelStatus> Statuses { get; }
    }
}
