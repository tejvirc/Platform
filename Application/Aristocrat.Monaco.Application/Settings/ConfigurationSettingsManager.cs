namespace Aristocrat.Monaco.Application.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Settings;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    public sealed class ConfigurationSettingsManager : IConfigurationSettingsManager, IService, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string SettingsProvidersExtensionPath = "/Application/Configuration/Settings";

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;

        private readonly Dictionary<string, IConfigurationSettings> _providers =
            new Dictionary<string, IConfigurationSettings>();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigurationSettingsManager"/> class.
        /// </summary>
        public ConfigurationSettingsManager()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigurationSettingsManager"/> class.
        /// </summary>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        public ConfigurationSettingsManager(IPropertiesManager properties, IEventBus eventBus)
        {
            _properties = properties;
            _eventBus = eventBus;
        }

        /// <inheritdoc />
        ~ConfigurationSettingsManager()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[]
        {
            typeof(IConfigurationSettingsManager)
        };

        /// <inheritdoc />
        public void Initialize()
        {
            try
            {
                var providers = MonoAddinsHelper.GetSelectedNodes<ConfigurationSettingsNode>(SettingsProvidersExtensionPath)
                    .Select(x => (IConfigurationSettings)x.CreateInstance());

                foreach (var provider in providers)
                {
                    provider.Initialize();
                    _providers.Add(provider.GetType().FullName ?? throw new InvalidOperationException(), provider);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred initializing ConfigurationSettingsManager, {ex.Message}", ex);
                // throw;
            }
        }

        /// <inheritdoc />
        public async Task Export(ConfigurationGroup configGroup)
        {
            foreach (var grp in configGroup.GetFlags().Where(x => x != ConfigurationGroup.None))
            {
                var settings = await ExportSettings(grp);

                _eventBus.Publish(new ConfigurationSettingsExportedEvent(grp, settings));
            }
        }

        /// <inheritdoc />
        public IEnumerable<(ConfigurationGroup group, IDictionary<string, object> settings)> Preview(
            ConfigurationGroup configGroup,
            string providerName = null)
        {
            foreach (var grp in configGroup.GetFlags()
                .Where(x => x != ConfigurationGroup.None))
            {
                var settings = ReadSettings(grp);

                yield return ((grp, settings));
            }
        }

        /// <inheritdoc />
        public async Task Import(ConfigurationGroup configGroup, string providerName)
        {
            foreach (var grp in configGroup.GetFlags().Where(x => x != ConfigurationGroup.None))
            {
                var settings = await ImportSettings(grp, providerName);

                _eventBus.Publish(new ConfigurationSettingsImportedEvent(grp, settings));
            }

            var machineSettingsImported = _properties.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
            machineSettingsImported |= ImportMachineSettings.Imported;
            _properties.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
        }

        public async Task Import(
            ConfigurationGroup group,
            IReadOnlyDictionary<string, object> settings,
            string providerName)
        {
            await ImportSettings(group, providerName, settings);
            _eventBus.Publish(new ConfigurationSettingsImportedEvent(group, settings));
        }

        public async Task Summary(ConfigurationGroup configGroup)
        {
            foreach (var grp in configGroup.GetFlags().Where(x => x != ConfigurationGroup.None))
            {
                var settings = await SettingsSummary(grp);

                _eventBus.Publish(new ConfigurationSettingsSummaryEvent(grp, settings));
            }
        }

        /// <inheritdoc />
        public bool IsConfigurationImportFilePresent(ConfigurationGroup configGroup)
        {
            var numberOfConfigGroups = configGroup.GetFlags()
                .Count(flag => flag != ConfigurationGroup.None);

            if (numberOfConfigGroups != 1)
            {
                return false;
            }

            var drive = _properties.GetValue(ApplicationConstants.EKeyDrive, null as string);
            var fileName = $"{Enum.GetName(typeof(ConfigurationGroup), configGroup)}.json";

            return File.Exists($@"{drive}/{fileName}");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private async Task<Dictionary<string, object>> ExportSettings(ConfigurationGroup configGroup)
        {
            var settings = new Dictionary<string, object>();

            foreach (var provider in _providers.Values.Where(x => x.Groups.HasFlag(configGroup)))
            {
                var sectionSettings = await provider.Get(configGroup);

                if (sectionSettings != null)
                    settings.Add(provider.Name, sectionSettings);
            }

            if (!settings.Any())
            {
                return settings;
            }

            var json = JsonConvert.SerializeObject(
                settings,
                Formatting.None,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });


            using (var writer = GetWriter($"{Enum.GetName(typeof(ConfigurationGroup), configGroup)}.json"))
            {
                await writer.WriteAsync(json);
            }

            return settings;
        }

        private async Task<Dictionary<string, object>> SettingsSummary(ConfigurationGroup configGroup)
        {
            var settings = new Dictionary<string, object>();

            foreach (var provider in _providers.Values.Where(x => x.Groups.HasFlag(configGroup)))
            {
                var sectionSettings = await provider.Get(configGroup);

                if (sectionSettings != null)
                    settings.Add(provider.Name, sectionSettings);
            }
            return settings;
        }

        private async Task<Dictionary<string, object>> ImportSettings(ConfigurationGroup configGroup, string providerName)
        {
            var settings = ReadSettings(configGroup);

            await ImportSettings(configGroup, providerName, settings);

            return settings;
        }

        private async Task ImportSettings(ConfigurationGroup configGroup, string providerName, IReadOnlyDictionary<string, object> settings)
        {
            foreach (var section in settings)
            {
                if (!string.IsNullOrEmpty(providerName) && providerName != section.Key)
                {
                    continue;
                }

                var provider = _providers.Values.FirstOrDefault(x => x.Name == section.Key);

                if (provider == null)
                {
                    throw new InvalidOperationException($"Invalid section, {section.Key}");
                }

                await provider.Apply(configGroup, section.Value);
            }
        }

        private Dictionary<string, object> ReadSettings(ConfigurationGroup configGroup)
        {
            using (var reader = GetStreamReader($"{Enum.GetName(typeof(ConfigurationGroup), configGroup)}.json"))
            {
                var json = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    json,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
        }

        private StreamWriter GetWriter(string fileName)
        {
            var drive = _properties.GetValue<string>(ApplicationConstants.EKeyDrive, null);

            if (string.IsNullOrWhiteSpace(drive))
            {
                throw new InvalidOperationException("EKey not inserted or not verified");
            }

            var fs = File.Open($@"{drive}/{fileName}", FileMode.Create, FileAccess.Write, FileShare.Read);

            return new StreamWriter(fs, Encoding.UTF8, 4096, false);
        }

        private StreamReader GetStreamReader(string fileName)
        {
            var drive = _properties.GetValue<string>(ApplicationConstants.EKeyDrive, null);

            if (string.IsNullOrWhiteSpace(drive))
            {
                throw new InvalidOperationException("EKey not inserted or not verified");
            }

            var fs = File.OpenRead($@"{drive}/{fileName}");
            return new StreamReader(fs, Encoding.UTF8, false, 4096, false);
        }
    }
}
