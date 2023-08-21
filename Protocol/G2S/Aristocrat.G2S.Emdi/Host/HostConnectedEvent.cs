namespace Aristocrat.G2S.Emdi.Host
{
    using Monaco.Kernel;

    /// <summary>
    /// Event raised when host is connected
    /// </summary>
    public class HostConnectedEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostConnectedEvent"/> class.
        /// </summary>
        /// <param name="port"></param>
        public HostConnectedEvent(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; set; }
    }
}