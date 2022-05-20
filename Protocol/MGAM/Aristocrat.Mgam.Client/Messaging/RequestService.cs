namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Net;

    /// <summary>
    ///     VLTs can use this broadcast message to locate services on the site controller.
    /// </summary>
    public class RequestService : Request
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="RequestGuid"/> class.
        /// </summary>
        /// <param name="serviceName">VLT service name to locate.</param>
        /// <param name="responseAddress">Directory service response address.</param>
        public RequestService(string serviceName, IPEndPoint responseAddress)
        {
            ServiceName = serviceName;
            ResponseAddress = responseAddress;
        }

        /// <summary>
        ///     Gets or sets the name of the service to locate.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        ///     Gets or sets the UDP connection string where the response should be sent.
        /// </summary>
        /// <remarks>
        ///     This string is in the format host:port, i.e. "192.168.1.113:6559".
        /// </remarks>
        public IPEndPoint ResponseAddress { get; }
    }
}
