namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.G2S.Protocol.v21;
    using log4net;

    /// <summary>
    ///     Client for receiving MTP messages from a remote G2S host's multicast groups.
    /// </summary>
    public partial class CommunicationsDevice : IMtpClient, IDisposable
    {
        private MessageBuilder _messageBuilder;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMessageConsumer _consumer;
        private ConcurrentDictionary<string, IMtpDeviceConnection> _mtpConnections;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public void ConfigureMtpClient()
        {
            _messageBuilder = new MessageBuilder();
            _messageBuilder.LoadSecurityNamespace(SchemaVersion.v21, null);
            _messageBuilder.LoadNamespace(SchemaVersion.v21, null);
            _mtpConnections = new ConcurrentDictionary<string, IMtpDeviceConnection>();
        }

        /// <inheritdoc />
        public void CreateDeviceConnection(string multicastId, int deviceId, int communicationId, string deviceClass, string address, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId)
        {
            RemoveDeviceConnection(multicastId);

            var newConnection = new MtpDeviceConnection(multicastId, deviceId, communicationId, deviceClass, _messageBuilder, EndpointUtilities.CreateIPEndPoint(address), currentKey, currentMsgId, newKey, keyChangeId, lastMessageId);
            newConnection.ConnectConsumer(_consumer);
            if (!_mtpConnections.TryAdd(multicastId, newConnection))
            {
                newConnection.Dispose();
                Logger.Info($"Failed to create MTP connection for multicastId {multicastId}.");
            }
        }

        /// <inheritdoc />
        public void UpdateSecurityParameters(string multicastId, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId)
        {
            IMtpDeviceConnection connection;
            if (_mtpConnections.TryGetValue(multicastId, out connection))
                connection.SetMtpSecurityParameters(currentKey, currentMsgId, newKey, keyChangeId, lastMessageId);
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}.");
        }

        /// <inheritdoc />
        public void Open()
        {
            foreach (var connection in _mtpConnections)
            {
                if(!connection.Value.Connected)
                    connection.Value.Open();
            }
            Logger.Info("MTP connections opened.");
        }

        /// <inheritdoc />
        public void Open(string multicastId)
        {
            IMtpDeviceConnection connection;
            if (_mtpConnections.TryGetValue(multicastId, out connection))
            {
                if(!connection.Connected)
                    connection.Open();
            }
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId} to open.");
        }

        /// <inheritdoc />
        public void CloseMtpConnections()
        {
            foreach (var connection in _mtpConnections)
                connection.Value.Close();
            Logger.Info("MTP connections closed.");
        }

        /// <inheritdoc />
        public void CloseMtpConnection(string multicastId)
        {
            IMtpDeviceConnection connection;
            if (_mtpConnections.TryGetValue(multicastId, out connection))
                connection.Close();
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}, so cannot close it.");
        }

        /// <inheritdoc />
        public void RemoveDeviceConnection(string multicastId)
        {
            if (_mtpConnections.ContainsKey(multicastId))
            {
                IMtpDeviceConnection connection;
                if (_mtpConnections.TryRemove(multicastId, out connection))
                    connection.Dispose();
            }
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}, so cannot remove it.");
        }

        /// <inheritdoc />
        public void ConnectConsumer(IMessageConsumer consumer)
        {
            _consumer = consumer;
            foreach (var connection in _mtpConnections)
                connection.Value.ConnectConsumer(_consumer);
        }

        /// <inheritdoc />
        public void OpenMtp()
        {
            Open();
        }

        /// <inheritdoc />
        public Session SendKeyUpdateRequest(object command)
        {
            var getKeyUpdate = command as getMcastKeyUpdate;
            var keyUpdateRequest = InternalCreateClass();
            keyUpdateRequest.Item = getKeyUpdate;
            return Queue.SendRequest(keyUpdateRequest);
        }
    }
}