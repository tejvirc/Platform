namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class LocalizationConfigurationPropertiesProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private const string LocaleConfigurationExtensionPath = "/Locale/Configuration";

        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationPropertiesProvider"/> class.
        /// </summary>
        public LocalizationConfigurationPropertiesProvider()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationPropertiesProvider"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/> instance.</param>
        /// <param name="properties"></param>
        public LocalizationConfigurationPropertiesProvider(
            IEventBus eventBus,
            IPropertiesManager properties)
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var storageName = GetType().ToString();

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            var configPath = GetLocaleConfigurationPath();

            var config = DeserializeConfiguration<LocaleConfiguration>(configPath);

            var operatorTicketLocale = config.OperatorTicket?.Locale ?? ApplicationConstants.DefaultLanguage;
            var operatorTicketSelectable = config.OperatorTicket?.Selectable ?? new[] { operatorTicketLocale };
            var operatorTicketDateFormat = config.OperatorTicket?.DateFormat ?? ApplicationConstants.DefaultDateFormat;
            var operatorDateFormat = config.Operator?.DateFormat ?? ApplicationConstants.DefaultDateFormat;

            var playerTicketLocale = config.PlayerTicket?.Locale ?? ApplicationConstants.DefaultLanguage;
            var playerTicketDateFormat = config.PlayerTicket?.DateFormat ?? ApplicationConstants.DefaultPlayerTicketDateFormat;

            var playerTicketSelectable = config.PlayerTicket?.Selectable ??
                new []
                {
                    new PlayerTicketSelectionArrayEntry
                    {
                        Locale = playerTicketLocale,
                        CurrencyValueLocale = string.Empty,
                        CurrencyWordsLocale = string.Empty
                     }
                };

            var playerTicketPrintLanguageSettingVisible = config.PlayerTicket?.LanguageSetting?.Visible ?? false;
            var playerTicketPrintLanguageSettingOperatorOverride = config.PlayerTicket?.LanguageSetting?.OperatorOverride ?? true;
            var playerTicketPrintLanguageSettingShowCheckBox = config.PlayerTicket?.LanguageSetting?.ShowCheckBox ?? false;

            var operatorAvailable = config.Operator?.Available ?? new[] { ApplicationConstants.DefaultLanguage };
            var operatorDefault = config.Operator?.Default ?? operatorAvailable.First();

            var playerAvailable = config.Player?.Available ?? new[] { ApplicationConstants.DefaultLanguage };
            var playerPrimary = config.Player?.Primary ?? playerAvailable.First();

            // The Tuple is structured as value (Item1) and storageKey (Item2)
            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                {
                    ApplicationConstants.LocalizationOperatorTicketLocale,
                    Tuple.Create((object)operatorTicketLocale, false)
                },
                {
                    ApplicationConstants.LocalizationOperatorTicketSelectable,
                    Tuple.Create((object)operatorTicketSelectable, false)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketOverride,
                    Tuple.Create(InitFromStorage(ApplicationConstants.LocalizationPlayerTicketOverride), true)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketLocale,
                    Tuple.Create(InitFromStorage(ApplicationConstants.LocalizationPlayerTicketLocale), true)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketDateFormat,
                    Tuple.Create(InitFromStorage(ApplicationConstants.LocalizationPlayerTicketDateFormat), true)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketSelectable,
                    Tuple.Create((object)playerTicketSelectable, false)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketLanguageSettingVisible,
                    Tuple.Create((object)playerTicketPrintLanguageSettingVisible, false)
                },
                {
                    ApplicationConstants.LocalizationPlayerTicketLanguageSettingShowCheckBox,
                    Tuple.Create((object)playerTicketPrintLanguageSettingShowCheckBox, false)
                },
                {
                    ApplicationConstants.LocalizationOperatorAvailable,
                    Tuple.Create((object)operatorAvailable, false)
                },
                {
                    ApplicationConstants.LocalizationOperatorDefault,
                    Tuple.Create((object)operatorDefault, false)
                },
                {
                    ApplicationConstants.LocalizationPlayerAvailable,
                    Tuple.Create((object)playerAvailable, false)
                },
                {
                    ApplicationConstants.LocalizationPlayerPrimary,
                    Tuple.Create((object)playerPrimary, false)
                },
                {
                    ApplicationConstants.LocalizationOperatorDateFormat,
                    Tuple.Create((object)operatorDateFormat, false)
                },
                {
                    ApplicationConstants.LocalizationOperatorTicketDateFormat,
                    Tuple.Create((object)operatorTicketDateFormat, false)
                }
            };

            if (!blockExists)
            {
                SetProperty(ApplicationConstants.LocalizationPlayerTicketOverride, playerTicketPrintLanguageSettingOperatorOverride);
                SetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, playerTicketLocale);
                SetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, playerTicketDateFormat);
            }

            properties.AddPropertyProvider(this);

            // post the jurisdiction's overriding resource folders to the localization framework
            eventBus.Publish(new LocalizationConfigurationEvent(configPath, config.Overrides ?? Array.Empty<string>()));
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown game property: " + propertyName;
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

            // NOTE:  Not all properties are persisted
            if (value.Item2)
            {
                Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");

                _persistentStorageAccessor[propertyName] = propertyValue;
            }

            _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
        }

        private static T DeserializeConfiguration<T>(string configurationFileName)
            where T : class, new()
        {
            T configuration;

            try
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(T))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(typeof(T), theXmlRootAttribute ?? new XmlRootAttribute(nameof(T)));
                using (var reader = new StreamReader(configurationFileName))
                {
                    configuration = serializer.Deserialize(reader) as T;
                }
            }
            catch (ArgumentException exception)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    "Exception occurred while deserializing Locale configuration. Exception: {0}.",
                    exception.ToString());
                throw;
            }

            return configuration;
        }

        private static string GetLocaleConfigurationPath()
        {
            string path;

            try
            {
                var node =
                    MonoAddinsHelper.GetSingleSelectedExtensionNode<FilePathExtensionNode>(
                        LocaleConfigurationExtensionPath);
                path = node.FilePath;
                Logger.DebugFormat(
                    CultureInfo.CurrentCulture,
                    "Found {0} node: {1}",
                    LocaleConfigurationExtensionPath,
                    node.FilePath);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    "Extension path {0} not found",
                    LocaleConfigurationExtensionPath);
                throw;
            }

            return path;
        }

        private object InitFromStorage(string propertyName, Action<object> onInitialized = null)
        {
            var value = _persistentStorageAccessor[propertyName];

            onInitialized?.Invoke(value);

            return value;
        }
    }
}
