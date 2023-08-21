namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class InvalidConfigurationReceivedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "The configuration received from the Bingo Server was invalid";
        }
    }
}