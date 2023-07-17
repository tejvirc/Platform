namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using Kernel;
    using System;

    /// <summary>Definition of the Hopper Refill STarted event class.</summary>
    [Serializable]
    public class HopperRefillStartedEvent : BaseEvent
    {
        /// <summary>
        /// Hopper Refill Event.
        /// </summary>
        public HopperRefillStartedEvent()
        {
        }

    }
}
