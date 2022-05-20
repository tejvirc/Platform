namespace Aristocrat.G2S.Emdi.Events
{
    /// <summary>
    /// Event subscriber
    /// </summary>
    public class EventSubscriber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriber"/> class.
        /// </summary>
        /// <param name="port"></param>
        public EventSubscriber(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; }
    }
}