namespace Aristocrat.Monaco.Hhr.Events
{
    using Client.WorkFlow;
    using Kernel;
    using System.Globalization;
    public class UnexpectedOrNoResponseEvent : BaseEvent
    {
        public UnexpectedOrNoResponseEvent(IRequestTimeout requestTimeout) => _requestTimeout = requestTimeout;

        private readonly IRequestTimeout _requestTimeout;

        public override string ToString()
        {
            return _requestTimeout is LockupRequestTimeout lockupRequestTimeout
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    $"{GetType().Name} ({lockupRequestTimeout.LockupString})")
                : GetType().Name;
        }
    }
}