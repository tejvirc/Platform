namespace Aristocrat.G2S.Emdi.Meters
{
    /// <summary>
    /// Meter subscriber
    /// </summary>
    public class MeterSubscriber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeterSubscriber"/> class.
        /// </summary>
        public MeterSubscriber(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Gets the port
        /// </summary>
        public int Port { get; }
    }
}