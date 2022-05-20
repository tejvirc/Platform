namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     An event to notify that the directory port number has changed
    /// </summary>
    public class HostConfigurationChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationChangedEvent" /> class.
        /// </summary>
        /// <param name="serviceName">VLT service name.</param>
        /// <param name="directoryAddress">Directory service IP address.</param>
        /// <param name="directoryPort">Directory service port.</param>
        /// <param name="useUdpBroadcasting">Use UDP broadcasting to communicate with the Directory service.</param>
        public HostConfigurationChangedEvent(string serviceName, string directoryAddress, int directoryPort, bool useUdpBroadcasting)
        {
            ServiceName = serviceName;
            DirectoryAddress = directoryAddress;
            DirectoryPort = directoryPort;
            UseUdpBroadcasting = useUdpBroadcasting;
        }

        /// <summary>
        ///     Gets the VLT service name.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        ///     Gets the Directory IP address.
        /// </summary>
        public string DirectoryAddress { get; }

        /// <summary>
        ///     Gets the port number for the directory service
        /// </summary>
        public int DirectoryPort { get; }

        /// <summary>
        ///     Gets a value that indicates whether UDP broadcasting is used to communicate with the Directory service.
        /// </summary>
        public bool UseUdpBroadcasting { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (Service Name: {ServiceName}, Directory IP Address: {DirectoryAddress}, Directory Port: {DirectoryPort}, Use UDP Broadcasting: {UseUdpBroadcasting})]";
        }
    }
}
