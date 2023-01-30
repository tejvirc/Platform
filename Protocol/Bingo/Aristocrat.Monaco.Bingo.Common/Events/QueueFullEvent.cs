namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class QueueFullEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Transaction Queue Full";
        }
    }
}
