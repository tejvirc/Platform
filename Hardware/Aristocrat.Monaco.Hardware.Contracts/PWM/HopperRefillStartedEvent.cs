namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;
    using Kernel;

    /// <summary>Definition of the Hopper Refill STarted event class.</summary>
    [Serializable]
    public class HopperRefillStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Hopper Refill Event.
        /// </summary>
        public HopperRefillStartedEvent()
        {
        }

    }
}
