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
    public class ConfiguredAddinsPropertiesProvider : IPropertyProvider
    {
        private const string SelectableConfigurationNameKey = "SelectableConfigurationName";
        private const string SelectedConfigurationValueKey = "SelectedConfigurationValue";
        private const int MaxSelections = 5;
        private const PersistenceLevel Level = PersistenceLevel.Static;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly int _blockSize = MaxSelections * 2 * sizeof(int);
        private readonly Dictionary<string, string> _selectedConfigurationPropertyReimport = new Dictionary<string, string>();

        private Dictionary<string, string> _selectedConfigurationProperty = new Dictionary<string, string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfiguredAddinsPropertiesProvider" /> class.
        /// </summary>
        public ConfiguredAddinsPropertiesProvider()
        {
            //// Get block of NVRam
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();

            Logger.DebugFormat("Getting block: {0}", blockName);

            if (storageManager.BlockExists(blockName))
            {
                Logger.DebugFormat("{0} found", blockName);
                var block = storageManager.GetBlock(blockName);

                var selectableConfigurationNames = (string[])block[SelectableConfigurationNameKey];
                var selectedConfigurationValues = (string[])block[SelectedConfigurationValueKey];
                for (var i = 0; i < MaxSelections; ++i)
                {
                    if (string.IsNullOrEmpty(selectableConfigurationNames[i]) &&
                        string.IsNullOrEmpty(selectedConfigurationValues[i]))
                    {
                        continue;
                    }

                    if (i == 0)
                    {
                        _selectedConfigurationPropertyReimport.Add(
                            selectableConfigurationNames[i],
                            selectedConfigurationValues[i]);
                    }

                    _selectedConfigurationProperty.Add(
                        selectableConfigurationNames[i],
                        selectedConfigurationValues[i]);
                }
            }
        }

        /// <summary>
        ///     Gets a reference to a property provider collection of properties.
        /// </summary>
        /// <returns>A read only reference to a collection.</returns>
        public ICollection<KeyValuePair<string, object>> GetCollection => GetKeyValueCollection();

        /// <summary>
        ///     Gets the selected configuration property
        /// </summary>
        /// <param name="propertyName"> Should be the ConfigurationSelectedKey property. </param>
        /// <returns> A selected configuration property. </returns>
        public object GetProperty(string propertyName)
        {
            object returnObject = null;

            if (propertyName == ApplicationConstants.SelectedConfigurationKey)
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                if (propertiesManager.GetValue(ApplicationConstants.MachineSettingsReimport, false) &&
                    !propertiesManager.GetValue(ApplicationConstants.MachineSettingsReimported, false))
                {
                    returnObject = _selectedConfigurationPropertyReimport;
                }
                else
                {
                    returnObject = _selectedConfigurationProperty;
                }
            }

            return returnObject;
        }

        /// <summary>
        ///     Sets the list of selected configurations.
        /// </summary>
        /// <param name="propertyName">Should be the ConfigurationSelectedKey property.</param>
        /// <param name="propertyValue"> Should be a list of strings representing the configuration names. </param>
        /// <remarks>
        ///     This method works only once, and only before values are written to
        ///     persistent storage.  At that point, persistent storage needs to be cleared to reset
        ///     these values.
        /// </remarks>
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (propertyName != ApplicationConstants.SelectedConfigurationKey)
            {
                return;
            }

            if (!(propertyValue is Dictionary<string, string>))
            {
                var errorMessage = $"Value is incorrect format: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            var valueAsDictionary = (Dictionary<string, string>)propertyValue;
            if (valueAsDictionary.Count > MaxSelections)
            {
                var errorMessage = $"Value is over capacity: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            _selectedConfigurationProperty = valueAsDictionary;
            Save();
        }

        /// <summary>
        ///     Saves the selected configuration to persistent storage
        /// </summary>
        private void Save()
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();

            var block = storageManager.BlockExists(blockName) ? storageManager.GetBlock(blockName) : storageManager.CreateBlock(Level, blockName, _blockSize);

            var selectableConfigurationNames = Enumerable.Repeat(string.Empty, MaxSelections).ToArray();
            var selectedConfigurationValues = Enumerable.Repeat(string.Empty, MaxSelections).ToArray();

            _selectedConfigurationProperty.Keys.CopyTo(selectableConfigurationNames, 0);
            _selectedConfigurationProperty.Values.CopyTo(selectedConfigurationValues, 0);

            block[SelectableConfigurationNameKey] = selectableConfigurationNames;
            block[SelectedConfigurationValueKey] = selectedConfigurationValues;
        }

        /// <summary>
        ///     Creates a KeyValuePair collection for GetCollection
        /// </summary>
        /// <returns>The KeyValuePair collection of property names and property values as objects</returns>
        private ICollection<KeyValuePair<string, object>> GetKeyValueCollection()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(ApplicationConstants.SelectedConfigurationKey, _selectedConfigurationProperty)
            };
        }
    }
}