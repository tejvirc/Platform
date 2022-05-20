namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class PlayersFoundEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Players Found";
        }
    }
}