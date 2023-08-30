using System.Net;

namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Methods required to manage MTP device connections.
    /// </summary>
    public interface IMtpConnectionControl
    {
        /// <summary>
        ///     Creates a new MTP connection.
        /// </summary>
        /// <param name="multicastId">Multicast identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="communicationId">The communication device identifier.</param>
        /// <param name="deviceClass">The G2S class.</param>
        /// <param name="address">The Multicast address of the connection.</param>
        /// <param name="currentKey">The current encryption key.</param>
        /// <param name="currentMsgId">The current MTP message ID.</param>
        /// <param name="newKey">The next encryption key.</param>
        /// <param name="keyChangeId">The new message ID after a planned key change.</param>
        /// <param name="lastMessageId">The last message ID processed with the first decryption key.</param>
        void CreateDeviceConnection(string multicastId, int deviceId, int communicationId, string deviceClass, string address, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId);

        /// <summary>
        ///     Removes an existing MTP connection.
        /// </summary>
        /// <param name="multicastId">Multicast identifier.</param>
        void RemoveDeviceConnection(string multicastId);

        /// <summary>
        ///     Connects a message consumer.
        /// </summary>
        /// <param name="consumer">Message Consumer that will be routed MTP messages.</param>
        void ConnectConsumer(IMessageConsumer consumer);

        /// <summary>
        ///     Opens the MTP connections.
        /// </summary>
        void OpenMtp();

        /// <summary>
        ///     Updates MTP security parameters for the specified multicastId.
        /// </summary>
        /// <param name="multicastId">Multicast identifier.</param>
        /// <param name="currentKey">The current encryption key.</param>
        /// <param name="currentMsgId">The current MTP message ID.</param>
        /// <param name="newKey">The next encryption key.</param>
        /// <param name="keyChangeId">The new message ID after the planned key change.</param>
        /// <param name="lastMessageId">The last message ID decrypted with the first key.</param>
        void UpdateSecurityParameters(string multicastId, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId);

        /// <summary>
        ///     Requests updated MTP security parameters for a particular connection.
        /// </summary>
        /// <param name="command">getMcastKeyUpdate Command.</param>
        /// <returns>Sending session.</returns>
        Session SendKeyUpdateRequest(object command);
    }
}