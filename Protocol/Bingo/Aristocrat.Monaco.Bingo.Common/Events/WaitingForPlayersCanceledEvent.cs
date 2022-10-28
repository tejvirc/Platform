namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class WaitingForPlayersCanceledEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Waiting For Players Canceled";
        }
    }
}