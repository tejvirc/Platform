namespace Aristocrat.Monaco.Application
{
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     A class that provides the properties for selected addin configurations.
    /// </summary>
    public class InitialSetupPropertiesProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private readonly Dictionary<string, object> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitialSetupPropertiesProvider" /> class.
        /// </summary>
        public InitialSetupPropertiesProvider()
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();
            
            _properties = new Dictionary<string, object>
            {
                { ApplicationConstants.ConfigWizardLastPageViewedIndex, 0 },
                { ApplicationConstants.ConfigWizardSelectionPagesDone, false }
            };

            if (storageManager.BlockExists(blockName))
            {
                _persistentStorageAccessor = storageManager.GetBlock(blockName);

                var keys = _properties.Keys.ToArray();
                foreach (var key in keys)
                {
                    _properties[key] = _persistentStorageAccessor[key];
                }
            }
            else
            {
                _persistentStorageAccessor = storageManager.CreateBlock(PersistenceLevel.Static, blockName, 1);
                using (var trans = _persistentStorageAccessor.StartTransaction())
                {
                    foreach (var prop in _properties)
                    {
                        trans[prop.Key] = prop.Value;
                    }

                    trans.Commit();
                }
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => _properties.ToArray();

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            var errorMessage = "Unknown config wizard property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (value == propertyValue)
            {
                return;
            }

            Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");

            _persistentStorageAccessor[propertyName] = propertyValue;
            _properties[propertyName] = propertyValue;
        }
    }
}