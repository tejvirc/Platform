namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The event that is raised when a stepper rule is triggered
    /// </summary>
    public class StepperRuleTriggeredEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="StepperRuleTriggeredEvent"/> class.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="eventId">The event id</param>
        public StepperRuleTriggeredEvent(int reelId, int eventId)
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

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [EventId={EventId}]");
        }
    }
}
