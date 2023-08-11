namespace Aristocrat.Monaco.Accounting.Contracts.Hopper
{
    using System;
    using Kernel;
    
    /// <summary>Definition of the Hopper Refill Completed event class.</summary>
    [Serializable]
    public class HopperRefillCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Hopper Refill completed Event.
        /// </summary>
        public HopperRefillCompletedEvent(DateTime lastRefillTime)
        {
            LastRefillTime = lastRefillTime;
        }

        /// <summary>
        ///     Last Hopper Refill Time
        /// </summary>
        public DateTime LastRefillTime { get; set; }
    }
}
