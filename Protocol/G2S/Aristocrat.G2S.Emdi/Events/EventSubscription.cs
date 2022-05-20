namespace Aristocrat.G2S.Emdi.Events
{
    /// <summary>
    /// Event subscription
    /// </summary>
    public class EventSubscription
    {
        private readonly int _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscription"/> class.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="eventCode"></param>
        public EventSubscription(int port, string eventCode)
        {
            Port = port;
            EventCode = eventCode;

            _hash = 33 * Port.GetHashCode() + EventCode.GetHashCode();
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the event code
        /// </summary>
        public string EventCode { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is EventSubscription sub))
            {
                return false;
            }

            return sub.Port.Equals(Port) && sub.EventCode.Equals(EventCode);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _hash;
        }
    }
}