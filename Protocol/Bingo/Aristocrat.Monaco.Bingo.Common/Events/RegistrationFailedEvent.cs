namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    public class RegistrationFailedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Registration with the Bingo Server Failed";
        }
    }
}