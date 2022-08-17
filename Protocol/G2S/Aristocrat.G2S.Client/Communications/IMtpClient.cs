namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Protocol.v21;
    using Devices;
    using Devices.v21;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Wrapper class around <see cref="UdpConnection"/>
    /// </summary>
    public interface IMtpClient: IMtpCapableDevice, IMulticastCoordinator
    {
        /// <summary>
        ///     Joins the multicast group.
        /// </summary>
        void Open(ICommunicationsDevice device);

        /// <summary>
        ///     Leaves the multicast group.
        /// </summary>
        void Close();

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        public void Dispose();

        /// <summary>
        ///     Connects a message consumer.
        /// </summary>
        /// <param name="consumer">Message Consumer instance that is supposed to be connected.</param>
        void ConnectConsumer(IMessageConsumer consumer);
    }
}
