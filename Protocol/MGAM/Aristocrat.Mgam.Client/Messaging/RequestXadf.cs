namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Net;

    /// <summary>
    ///     On boot-up, the device will first query the Directory Service(s) to discover what software it should be running.
    /// </summary>
    public class RequestXadf : Request
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="RequestXadf"/> class.
        /// </summary>
        /// <param name="responseAddress">Directory service response address.</param>
        /// <param name="deviceName">Device name.</param>
        /// <param name="manufacturerName">Manufacturer name.</param>
        public RequestXadf(IPEndPoint responseAddress, string deviceName, string manufacturerName)
        {
            ResponseAddress = responseAddress;
            DeviceName = deviceName;
            ManufacturerName = manufacturerName;
        }

        /// <summary>
        ///     Gets or sets the UDP connection string where the response should be sent.
        /// </summary>
        /// <remarks>
        ///     This string is in the format host:port, i.e. "192.168.1.113:6559".
        /// </remarks>
        public IPEndPoint ResponseAddress { get; }

        /// <summary>
        ///     Gets or sets the Device Name.
        /// </summary>
        /// <remarks>
        ///     Cabinet Serial Number.
        /// </remarks>
        public string DeviceName { get; }

        /// <summary>
        ///     Gets or sets the Manufacturer Name.
        /// </summary>
        /// <remarks>
        ///     Name of the manufacturer of the device.
        /// </remarks>
        public string ManufacturerName { get; }
    }
}
