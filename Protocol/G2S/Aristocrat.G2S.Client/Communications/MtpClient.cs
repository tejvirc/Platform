namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     Client for receiving MTP messages from a remote G2S host's multicast groups.
    /// </summary>
    public class MtpClient : IMtpClient, IDisposable
    {
        private readonly MessageBuilder _messageBuilder;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMessageConsumer _consumer;
        private ConcurrentDictionary<string, IMtpDeviceConnection> _connections;
        private bool _disposed;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public MtpClient()
        {
            _messageBuilder = new MessageBuilder();
            _messageBuilder.LoadSecurityNamespace(SchemaVersion.v21, null);
            _messageBuilder.LoadNamespace(SchemaVersion.v21, null);
            _connections = new ConcurrentDictionary<string, IMtpDeviceConnection>();
        }

        /// <inheritdoc />
        public void CreateDeviceConnection(string multicastId, int deviceId, int communicationId, string deviceClass, string address, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId)
        {
            RemoveDeviceConnection(multicastId);

            var newConnection = new MtpDeviceConnection(multicastId, deviceId, communicationId, deviceClass, _messageBuilder, EndpointUtilities.CreateIPEndPoint(address), currentKey, currentMsgId, newKey, keyChangeId, lastMessageId);
            newConnection.ConnectConsumer(_consumer);
            if (!_connections.TryAdd(multicastId, newConnection))
            {
                newConnection.Dispose();
                Logger.Info($"Failed to create MTP connection for multicastId {multicastId}.");
            }
        }

        /// <inheritdoc />
        public void UpdateSecurityParameters(string multicastId, byte[] currentKey, long currentMsgId, byte[] newKey, long keyChangeId, long lastMessageId)
        {
            IMtpDeviceConnection connection;
            if (_connections.TryGetValue(multicastId, out connection))
                connection.SetMtpSecurityParameters(currentKey, currentMsgId, newKey, keyChangeId, lastMessageId);
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}.");
        }

        /// <inheritdoc />
        public void Open()
        {
            foreach (var connection in _connections)
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
            if (_connections.TryGetValue(multicastId, out connection))
            {
                if(!connection.Connected)
                    connection.Open();
            }
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId} to open.");
        }

        /// <inheritdoc />
        public void Close()
        {
            foreach (var connection in _connections)
                connection.Value.Close();
            Logger.Info("MTP connections closed.");
        }

        /// <inheritdoc />
        public void Close(string multicastId)
        {
            IMtpDeviceConnection connection;
            if (_connections.TryGetValue(multicastId, out connection))
                connection.Close();
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}, so cannot close it.");
        }

        /// <inheritdoc />
        public void RemoveDeviceConnection(string multicastId)
        {
            if (_connections.ContainsKey(multicastId))
            {
                IMtpDeviceConnection connection;
                if (_connections.TryRemove(multicastId, out connection))
                    connection.Dispose();
            }
            else
                Logger.Info($"There is no MtpDeviceConnection for multicastId {multicastId}, so cannot remove it.");
        }

        /// <inheritdoc />
        public void ConnectConsumer(IMessageConsumer consumer)
        {
            _consumer = consumer;
            foreach (var connection in _connections)
                connection.Value.ConnectConsumer(_consumer);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
            }

            foreach (var connection in _connections)
                connection.Value.Dispose();

            _messageBuilder.Dispose();

            _disposed = true;
        }

        /// <inheritdoc />
        public void OpenMtp()
        {
            Open();
        }

        /// <inheritdoc />
        public Session SendKeyUpdateRequest(object command)
        {
            throw new NotImplementedException();
        }
    }
}