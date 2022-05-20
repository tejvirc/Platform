namespace Aristocrat.G2S.Emdi.Host
{
    using Monaco.Kernel;

    /// <summary>
    /// Event raised when host is connected
    /// </summary>
    public class HostDisconnectedEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostDisconnectedEvent"/> class.
        /// </summary>
        /// <param name="port"></param>
        public HostDisconnectedEvent(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; set; }
    }
}