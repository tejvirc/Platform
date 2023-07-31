namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using Properties;
    using System;
    using Kernel;
    using static System.FormattableString;

    /// <summary>Definition of the HopperBaseEvent class.</summary>
    /// <remarks>All other hopper events are derived from this event.</remarks>
    [Serializable]
    public class HopperBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperBaseEvent"/> class.
        /// </summary>
        protected HopperBaseEvent()
        {
            HopperId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperBaseEvent"/> class.
        /// </summary>
        /// <param name="hopperId">The ID of the hopper associated with the event.</param>
        protected HopperBaseEvent(int hopperId)
        {
            HopperId = hopperId;
        }

        /// <summary>
        ///     Gets the ID of the hopper associated with the event.
        /// </summary>
        public int HopperId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"Hopper {GetType().Name}");
        }
    }
}
