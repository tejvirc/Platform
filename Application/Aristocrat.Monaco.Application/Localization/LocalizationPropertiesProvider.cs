namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class LocalizationPropertiesProvider : ILocalizationPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, (object Value, bool Persist, Action<object> Callback)> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationPropertiesProvider"/> class.
        /// </summary>
        public LocalizationPropertiesProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationPropertiesProvider"/> class.
        /// </summary>
        /// <param name="storageManager"></param>
        public LocalizationPropertiesProvider(IPersistentStorageManager storageManager)
        {
            var storageName = GetType().ToString();

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Static, storageName, 1);

            _properties = new Dictionary<string, (object, bool, Action<object>)>
            {
                {
                    ApplicationConstants.LocalizationState,
                    (InitFromStorage(ApplicationConstants.LocalizationState), true, null)
                }
            };
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Value)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var property))
            {
                return property.Value;
            }

            var errorMessage = "Unknown game property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var property))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (property.Persist)
            {
                Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");

                _persistentStorageAccessor[propertyName] = propertyValue;
            }

            property.Callback?.Invoke(propertyValue);

            _properties[propertyName] = (propertyValue, property.Persist, property.Callback);
        }

        /// <inheritdoc />
        public void AddProperty<T>(string propertyName, T propertyValue, Action<object> callback = null)
        {
            if (_properties.ContainsKey(propertyName))
            {
                throw new ArgumentException($@"Property already exists -- {propertyName}", nameof(propertyName));
            }

            _properties.Add(propertyName, (propertyValue, false, callback));
        }

        private object InitFromStorage(string propertyName, Action<object> onInitialized = null)
        {
            var value = _persistentStorageAccessor[propertyName];

            onInitialized?.Invoke(value);

            return value;
        }
    }
}
