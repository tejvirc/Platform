namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     This class manages all of the property providers.
    /// </summary>
    public sealed class PropertiesManager : IPropertiesManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DefaultPropertyProvider _defaultProvider = new DefaultPropertyProvider();

        private readonly ConcurrentDictionary<string, IPropertyProvider> _propertyProvider =
            new ConcurrentDictionary<string, IPropertyProvider>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertiesManager" /> class.
        /// </summary>
        public PropertiesManager()
        {
            Logger.Debug("Adding the default property provider");

            // add the default property provider to our provider list
            AddPropertyProvider(_defaultProvider);
        }

        /// <summary>
        ///     Gets the number of keys in our property collection.
        ///     This method is only intended to be used in unit tests.
        /// </summary>
        public int Count => _propertyProvider.Count;

        /// <inheritdoc />
        public void Dispose()
        {
            _propertyProvider.Clear();
        }

        /// <inheritdoc />
        public void AddPropertyProvider(IPropertyProvider provider)
        {
            Logger.Debug($"Adding property provider: {provider}");

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            // iterate thru the list of properties being provided and add them to our collection
            foreach (var property in provider.GetCollection)
            {
                _propertyProvider[property.Key] = provider;
                eventBus.Publish(new PropertyChangedEvent(property.Key));
            }
        }

        /// <inheritdoc />
        public object GetProperty(string propertyName, object defaultValue)
        {
            // check if the property key is in our dictionary
            if (_propertyProvider.TryGetValue(propertyName, out var possiblePropertyProvider))
            {
                return possiblePropertyProvider.GetProperty(propertyName);
            }

            Logger.Warn($"No property found for property name: {propertyName}, returning default value");

            // nothing matched if we got to here.
            return defaultValue;
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            Logger.Info($"Setting property: {propertyName} with value {propertyValue}");

            PropertySet(propertyName, propertyValue);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue, bool isConfig)
        {
            if (isConfig)
            {
                Logger.Info($"[CONFIG] Setting property: {propertyName} with value {propertyValue}");
            }
            else
            {
                Logger.Debug($"Setting property: {propertyName} with value {propertyValue}");
            }

            PropertySet(propertyName, propertyValue);
        }

        /// <inheritdoc />
        public string Name => "Properties Manager";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPropertiesManager) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void PropertySet(string propertyName, object propertyValue)
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            // check if the property key is already in our dictionary
            if (_propertyProvider.TryGetValue(propertyName, out var possiblePropertyProvider))
            {
                var oldValue = possiblePropertyProvider.GetProperty(propertyName);
                possiblePropertyProvider.SetProperty(propertyName, propertyValue);

                if (oldValue == null)
                {
                    if (propertyValue != null)
                    {
                        eventBus.Publish(new PropertyChangedEvent(propertyName));
                    }
                }
                else if (!oldValue.Equals(propertyValue))
                {
                    eventBus.Publish(new PropertyChangedEvent(propertyName));
                }
            }
            else
            {
                Logger.Info($"Did not find property {propertyName} - adding it to the default property provider");

                // Didn't find the property key. Assume its a new property and add it to the default property provider
                _defaultProvider.SetProperty(propertyName, propertyValue);

                // Also add it to our dictionary
                _propertyProvider.TryAdd(propertyName, _defaultProvider);

                eventBus.Publish(new PropertyChangedEvent(propertyName));
            }
        }
    }
}
