namespace Aristocrat.Monaco.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Persistence;
    using Contracts.PWM;
    using Kernel;
    using log4net;

    public class HardwarePropertyProvider : IPropertyProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Static;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _blockExists;

        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        private readonly IPersistentStorageManager _storageManager;

        public HardwarePropertyProvider()
        {
            _storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var defaultProvider = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            _blockExists = _storageManager.BlockExists(GetBlockName());

            // The Tuple is structured as value (Item1) and persisted
            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                {
                    HardwareConstants.HardMetersEnabledKey,
                    Tuple.Create((object)InitFromStorage<bool>(HardwareConstants.HardMetersEnabledKey), true)
                },
                {
                    HardwareConstants.Display1, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.Display1) ??
                        defaultProvider.GetValue<string>(HardwareConstants.Display1, null),
                        true)
                },
                {
                    HardwareConstants.Display2, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.Display2) ??
                        defaultProvider.GetValue<string>(HardwareConstants.Display2, null),
                        true)
                },
                {
                    HardwareConstants.Display3, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.Display3) ??
                        defaultProvider.GetValue<string>(HardwareConstants.Display3, null),
                        true)
                },
                {
                    HardwareConstants.Display4, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.Display4) ??
                        defaultProvider.GetValue<string>(HardwareConstants.Display4, null),
                        true)
                },
                {
                    HardwareConstants.Display5, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.Display5) ??
                        defaultProvider.GetValue<string>(HardwareConstants.Display5, null),
                        true)
                },
                {
                    HardwareConstants.TouchDevice1, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.TouchDevice1) ??
                        defaultProvider.GetValue<string>(HardwareConstants.TouchDevice1, null),
                        true)
                },
                {
                    HardwareConstants.TouchDevice2, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.TouchDevice2) ??
                        defaultProvider.GetValue<string>(HardwareConstants.TouchDevice2, null),
                        true)
                },
                {
                    HardwareConstants.TouchDevice3, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.TouchDevice3) ??
                        defaultProvider.GetValue<string>(HardwareConstants.TouchDevice3, null),
                        true)
                },
                {
                    HardwareConstants.TouchDevice4, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.TouchDevice4) ??
                        defaultProvider.GetValue<string>(HardwareConstants.TouchDevice4, null),
                        true)
                },
                {
                    HardwareConstants.TouchDevice5, Tuple.Create(
                        (object)InitFromStorage<string>(HardwareConstants.TouchDevice5) ??
                        defaultProvider.GetValue<string>(HardwareConstants.TouchDevice5, null),
                        true)
                },
                {
                    HardwareConstants.Battery1Low, Tuple.Create(
                        (object)InitFromStorage(HardwareConstants.Battery1Low, true),
                        true)
                },
                {
                    HardwareConstants.Battery2Low, Tuple.Create(
                        (object)InitFromStorage(HardwareConstants.Battery2Low, true),
                        true)
                },
                {
                    HardwareConstants.BellEnabledKey,
                    Tuple.Create((object)InitFromStorage<bool>(HardwareConstants.BellEnabledKey), true)
                },
                {
                    HardwareConstants.DoorAlarmEnabledKey, Tuple.Create((object)true, false)
                },
                {
                    HardwareConstants.CoinAcceptorEnabledKey,
                    Tuple.Create((object)InitFromStorage<bool>(HardwareConstants.CoinAcceptorEnabledKey, false), true)
                },
                {
                    HardwareConstants.CoinAcceptorFaults,
                    Tuple.Create((object)InitFromStorage<int>(HardwareConstants.CoinAcceptorFaults, (int)CoinFaultTypes.None), true)
                },
            };

            if (!_blockExists)
            {
                // Set the defaults
                SetProperty(HardwareConstants.DoorAlarmEnabledKey, true);
            }

            _blockExists = true;
        }

        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown hardware property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (value.Item1 != propertyValue)
            {
                // NOTE:  Not all properties are persisted
                if (value.Item2)
                {
                    Logger.Debug($"Setting property {propertyName} to {propertyValue}");

                    var accessor = GetAccessor();
                    accessor[propertyName] = propertyValue;
                }

                _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
            }
        }

        private IPersistentStorageAccessor GetAccessor(string name = null, int blockSize = 1)
        {
            var blockName = GetBlockName(name);

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(Level, blockName, blockSize);
        }

        private string GetBlockName(string name = null)
        {
            var baseName = GetType().ToString();
            return string.IsNullOrEmpty(name) ? baseName : $"{baseName}.{name}";
        }

        private T InitFromStorage<T>(string propertyName, T defaultValue = default(T))
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