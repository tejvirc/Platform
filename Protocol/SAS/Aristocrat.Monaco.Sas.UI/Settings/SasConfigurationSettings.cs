namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts.Protocol;
    using Application.Contracts.Settings;
    using Contracts.SASProperties;
    using Kernel;
    using MVVM;
    using Storage.Models;

    /// <summary>
    ///     Implements <see cref="IConfigurationSettings"/> for SAS settings.
    /// </summary>
    public class SasConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SasConfigurationSettings"/> class.
        /// </summary>
        public SasConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SasConfigurationSettings"/> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager"/> instance.</param>
        public SasConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public string Name => "SAS";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine;

        /// <inheritdoc />
        public async Task Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri("/Aristocrat.Monaco.Sas.UI;component/Settings/MachineSettings.xaml", UriKind.RelativeOrAbsolute)
                    };

                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                });

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!configGroup.HasFlag(ConfigurationGroup.Machine))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            if (!(settings is MachineSettings machineSettings))
            {
                throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }

            await ApplySettings(machineSettings);
        }

        /// <inheritdoc />
        public async Task<object> Get(ConfigurationGroup configGroup)
        {
            if (ServiceManager.GetInstance()
                .GetService<IMultiProtocolConfigurationProvider>()
                .MultiProtocolConfiguration.All(x => x.Protocol != CommsProtocol.SAS))
            {
                return null;
            }

            if (!configGroup.HasFlag(ConfigurationGroup.Machine))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            return await GetSettings();
        }

        private async Task<MachineSettings> GetSettings()
        {
            var hosts = _properties.GetValue(SasProperties.SasHosts, Enumerable.Empty<Host>()).ToList();
            var portAssignment = _properties.GetValue(SasProperties.SasPortAssignments, new PortAssignment());
            var featureSettings = _properties.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());

            var sasHostSettings = hosts.Select(x => (SasHostSetting)x);

            return await Task.FromResult(
                new MachineSettings
                {
                    SasHostSettings = new ObservableCollection<SasHostSetting>(sasHostSettings),
                    PortAssignmentSetting = (PortAssignmentSetting)portAssignment,
                    SasFeaturesSettings = (SasFeaturesSettings)featureSettings
                });
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _properties.SetProperty(SasProperties.SasFeatureSettings, (SasFeatures)settings.SasFeaturesSettings);
            _properties.SetProperty(SasProperties.SasPortAssignments, (PortAssignment)settings.PortAssignmentSetting);
            _properties.SetProperty(SasProperties.SasHosts, settings.SasHostSettings.Select(x => (Host)x).ToList());
            await Task.CompletedTask;
        }
    }
}
