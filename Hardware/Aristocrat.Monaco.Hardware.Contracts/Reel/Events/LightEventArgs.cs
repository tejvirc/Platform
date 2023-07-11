namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Event args for all light events
    /// </summary>
    public class LightEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructs a new instance of the LightEventArgs class.
        /// </summary>
        /// <param name="statuses">The collection of statuses.</param>
        public LightEventArgs(params LightStatus[] statuses)
        {
            Statuses = statuses;
        }

        /// <summary>
        ///     Constructs a new instance of the LightEventArgs class.
        /// </summary>
        /// <param name="statuses">The collection of statuses.</param>
        public LightEventArgs(IEnumerable<LightStatus> statuses)
        {
            Statuses = statuses.ToArray();
        }

        /// <summary>
        ///     The statuses.
        /// </summary>
        public ICollection<LightStatus> Statuses { get; }
    }
}