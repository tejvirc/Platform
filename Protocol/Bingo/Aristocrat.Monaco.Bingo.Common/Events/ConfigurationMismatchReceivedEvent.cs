namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class ConfigurationMismatchReceivedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "The configuration received from the Bingo Server has changed";
        }
    }
}