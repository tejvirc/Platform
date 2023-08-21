namespace Aristocrat.Monaco.Hhr
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Client.Messages;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Property definitions for the HHR protocol
    /// </summary>
    public class HHRPropertyProvider : IPropertyProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _blockExists;
        private readonly Dictionary<string, (object Value, bool IsPersistent)> _properties;
        private readonly IPersistentStorageManager _storageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRPropertyProvider" /> class.
        /// </summary>
        public HHRPropertyProvider()
        {
            _storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            _blockExists = _storageManager.BlockExists(GetBlockName());

            _properties = new Dictionary<string, (object, bool)>
            {
                {
                    HHRPropertyNames.ServerTcpIp,
                    (InitFromStorage(HHRPropertyNames.ServerTcpIp, HhrConstants.DefaultServerTcpIp), true)
                },
                {
                    HHRPropertyNames.ServerTcpPort,
                    (InitFromStorage(HHRPropertyNames.ServerTcpPort, HhrConstants.DefaultServerTcpPort), true)
                },
                {
                    HHRPropertyNames.ServerUdpPort,
                    (InitFromStorage(HHRPropertyNames.ServerUdpPort, HhrConstants.DefaultServerUdpPort), true)
                },
                {
                    HHRPropertyNames.EncryptionKey,
                    (InitFromStorage(HHRPropertyNames.EncryptionKey, HhrConstants.DefaultEncryptionKey), true)
                },
                {
                    HHRPropertyNames.HeartbeatInterval,
                    (InitFromStorage(HHRPropertyNames.HeartbeatInterval, HhrConstants.HeartbeatInterval), false)
                },
                {
                    HHRPropertyNames.FailedRequestRetryTimeout,
                    (HhrConstants.FailedRequestRetryTimeout, false)
                },
                {
                    HHRPropertyNames.LastGamePlayTime,
                    (InitFromStorage(HHRPropertyNames.LastGamePlayTime, 0u), false)
                },
                {
                    HHRPropertyNames.SequenceId,
                    (InitFromStorage(HHRPropertyNames.SequenceId, 1u), true)
                },
                {
                    HHRPropertyNames.ManualHandicapMode,
                    (InitFromStorage(HHRPropertyNames.ManualHandicapMode, HhrConstants.DetectPickMode), true)
                },
            };

            _blockExists = true;
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Value)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var property))
            {
                return property.Value;
            }

            var errorMessage = "Unknown HHR property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = "Cannot set unknown HHR property: " + propertyName;
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (value.Value == propertyValue)
            {
                return;
            }

            // NOTE: Not all properties are persisted
            if (value.IsPersistent)
            {
                var accessor = GetAccessor();
                accessor[propertyName] = propertyValue;
            }

            _properties[propertyName] = (propertyValue, value.IsPersistent);
        }

        private IPersistentStorageAccessor GetAccessor()
        {
            var blockName = GetBlockName();

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(Level, blockName, 1);
        }

        private string GetBlockName()
        {
            return GetType().ToString();
        }

        private T InitFromStorage<T>(string propertyName, T defaultValue = default)
        {
            var accessor = GetAccessor();
            if (!_blockExists)
            {
                accessor[propertyName] = defaultValue;
            }

            return (T)accessor[propertyName];
        }
    }
}