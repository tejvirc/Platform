using System.Net;

namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Provides a mechanism to control a single MTP connection instance.
    /// </summary>
    public interface IMtpDeviceConnection
    {
        /// <summary>
        ///     Gets the multicast address.
        /// </summary>
        IPEndPoint MulticastAddress { get; }

        /// <summary>
        ///     Gets the multicastId.
        /// </summary>
        string MulticastId { get; }

        /// <summary>
        ///     Gets the G2S device ID.
        /// </summary>
        int DeviceId { get; }

        /// <summary>
        ///     Gets the G2S class.
        /// </summary>
        string DeviceClass { get; }

        /// <summary>
        ///     Gets the connection state.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        ///     Joins the multicast group.
        /// </summary>
        void Open();

        /// <summary>
        ///     Leaves the multicast group.
        /// </summary>
        void Close();

        /// <summary>
        ///     Sets the MTP security parameters
        /// </summary>
        void SetMtpSecurityParameters(byte[] key, long messageId, byte[] nextKey, long keyChangeId, long lastMessageId);

        /// <summary>
        ///     Sets the multicast address.
        /// </summary>
        /// <param name="address">The new keep alive interval.</param>
        void SetMulticastAddress(string address);

        /// <summary>
        ///     Connects a message consumer.
        /// </summary>
        /// <param name="consumer">Message Consumer that will be routed MTP messages.</param>
        void ConnectConsumer(IMessageConsumer consumer);

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        public void Dispose();
    }
}