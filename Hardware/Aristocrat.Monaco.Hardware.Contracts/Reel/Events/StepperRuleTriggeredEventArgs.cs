namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The event arguments for stepper rule triggered events (user event) 
    /// </summary>
    public class StepperRuleTriggeredEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="StepperRuleTriggeredEventArgs"/> class.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="eventId">The event id</param>
        public StepperRuleTriggeredEventArgs(int reelId, int eventId)
        {
            ReelId = reelId;
            EventId = eventId;
        }

        /// <summary>
        ///     Gets the reel id.
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the event id
        /// </summary>
        public int EventId { get; }
    }
}
