namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     This event is sent for bingo test timing.
    /// </summary>
    public class BingoTestTimingEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoTestTimingEvent"/>
        /// </summary>
        /// <param name="type">The type of event</param>
        public BingoTestTimingEvent(BingoTestEventType type)
        {
            Type = type;
        }

        /// <summary>
        ///     Get the the type of event.
        /// </summary>
        public BingoTestEventType Type { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(BingoTestTimingEvent)}({Type})";
        }
    }

    public enum BingoTestEventType
    {
        Idle = 0,
        ButtonPressed,
        GameStarted,
        SentToClient,
        ResponseFromClient,
        FakeDelay,
        CardDisplayed
    }
}