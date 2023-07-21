namespace Aristocrat.Monaco.Hhr.UI.Settings
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts;
    using Application.Contracts.Protocol;
    using Application.Contracts.Settings;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Client.Messages;
    using Kernel;

    public class HHRConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;
        private const string SettingsXamlUri = "/Aristocrat.Monaco.Hhr.UI;component/Settings/MachineSettings.xaml";
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRConfigurationSettings"/> class.
        /// </summary>
        public HHRConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRConfigurationSettings"/> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager"/> instance.</param>
        public HHRConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public string Name => ProtocolNames.HHR;

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine;

        /// <inheritdoc />
        public async Task Initialize()
        {
            Execute.OnUIThread(
                () =>
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(
                            SettingsXamlUri,
                            UriKind.RelativeOrAbsolute)
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
                .MultiProtocolConfiguration.All(x => x.Protocol != CommsProtocol.HHR))
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
            return await Task.FromResult(
                new MachineSettings
                {
                    CentralServerIpAddress = _properties.GetValue(HHRPropertyNames.ServerTcpIp, string.Empty),
                    CentralServerTcpPortNumber = _properties.GetValue(HHRPropertyNames.ServerTcpPort, 0),
                    CentralServerEncryptionKey = _properties.GetValue(HHRPropertyNames.EncryptionKey, string.Empty),
                    CentralServerUdpPortNumber = _properties.GetValue(HHRPropertyNames.ServerUdpPort, 0),
                    CentralServerHandicapMode = _properties.GetValue(HHRPropertyNames.ManualHandicapMode, HhrConstants.DetectPickMode)
                });
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _properties.SetProperty(HHRPropertyNames.ServerTcpIp, settings.CentralServerIpAddress);
            _properties.SetProperty(HHRPropertyNames.ServerTcpPort, settings.CentralServerTcpPortNumber);
            _properties.SetProperty(HHRPropertyNames.EncryptionKey, settings.CentralServerEncryptionKey);
            _properties.SetProperty(HHRPropertyNames.ServerUdpPort, settings.CentralServerUdpPortNumber);
            _properties.SetProperty(HHRPropertyNames.ManualHandicapMode, settings.CentralServerHandicapMode);
            await Task.CompletedTask;
        }
    }
}
