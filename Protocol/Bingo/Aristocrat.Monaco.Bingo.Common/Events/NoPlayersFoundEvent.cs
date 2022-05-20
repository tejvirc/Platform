namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class NoPlayersFoundEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "No Players Found.";
        }
    }
}