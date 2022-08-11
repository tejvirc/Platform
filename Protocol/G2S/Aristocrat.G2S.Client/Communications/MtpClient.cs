namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Monaco.Protocol.Common;

    /// <summary>
    ///     Wrapper class around <see cref="UdpClient"/> for receiving MTP messages.
    /// </summary>
    public class MtpClient : UdpClient, IMtpClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MtpClient"/> class.
        /// </summary>
        /// <param name="endPoint">Service address.</param>
        public MtpClient(IPEndPoint endPoint)
            : base(endPoint)
        {
        }
    }
}
