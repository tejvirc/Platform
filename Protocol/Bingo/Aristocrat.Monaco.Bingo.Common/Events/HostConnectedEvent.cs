namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class HostConnectedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Bingo Host Connected";
        }
    }
}