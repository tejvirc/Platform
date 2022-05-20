namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using System;
    using Kernel;

    public class WaitingForPlayersEvent : BaseEvent
    {
        public WaitingForPlayersEvent(DateTime startTimeUtc, TimeSpan waitingDuration)
        {
            StartTimeUtc = startTimeUtc;
            WaitingDuration = waitingDuration;
        }

        public DateTime StartTimeUtc { get; }

        public TimeSpan WaitingDuration { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Waiting for players started: {StartTimeUtc}";
        }
    }
}