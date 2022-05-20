namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class HostDisconnectedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Bingo Host Disconnected";
        }
    }
}