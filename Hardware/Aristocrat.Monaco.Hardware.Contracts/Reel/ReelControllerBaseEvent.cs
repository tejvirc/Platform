namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using Kernel;
    using Properties;
    using static System.FormattableString;

    /// <summary>
    ///     The base event for all reel controller events
    /// </summary>
    [Serializable]
    public abstract class ReelControllerBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelControllerBaseEvent"/> class.
        /// </summary>
        protected ReelControllerBaseEvent()
        {
            ReelControllerId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelControllerBaseEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the reel controller associated with the event.</param>
        protected ReelControllerBaseEvent(int reelControllerId)
        {
            ReelControllerId = reelControllerId;
        }

        /// <summary>
        ///     Gets the ID of the reel controller associated with the event.
        /// </summary>
        public int ReelControllerId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.ReelControllerText} {GetType().Name}");
        }
    }
}