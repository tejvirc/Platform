namespace Aristocrat.Monaco.Kernel
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     This class is the default property provider for the system.
    ///     Components would add new properties to this property provider.
    /// </summary>
    public sealed class DefaultPropertyProvider : IPropertyProvider
    {
        private const string PropertySettingsExtensionPath = "/Kernel/PropertySettings";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
        {
            get
            {
                // Look for properties in Mono-Addins extensions
                foreach (var node in AddinManager.GetExtensionNodes<PropertyNode>(PropertySettingsExtensionPath))
                {
                    _properties.TryAdd(node.PropertyName, node.PropertyValue);
                }

                // return a read only reference to our collection
                return _properties;
            }
        }

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (!_properties.TryGetValue(propertyName, out var property))
            {
                var error = $"Cannot get value of unrecognized property: {propertyName}";
                Logger.Error(error);
                throw new UnknownPropertyException(error);
            }

            Logger.Debug($"Getting property with key: {propertyName}, value is: {property}");

            return property;
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            Logger.Debug($"Setting property with key: {propertyName}, value: {propertyValue}");

            _properties.AddOrUpdate(propertyName, propertyValue, (key, value) => propertyValue);
        }
    }
}
