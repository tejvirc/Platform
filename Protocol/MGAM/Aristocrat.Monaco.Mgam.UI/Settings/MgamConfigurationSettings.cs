namespace Aristocrat.Monaco.Mgam.UI.Settings
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts;
    using Application.Contracts.Protocol;
    using Application.Contracts.Settings;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Common;
    using Kernel;

    /// <summary>
    ///     Manages the import and export of settings.
    /// </summary>
    public class MgamConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamConfigurationSettings"/> class.
        /// </summary>
        public MgamConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamConfigurationSettings"/> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager"/> instance.</param>
        public MgamConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        public string Name => ProtocolNames.MGAM;

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
                        Source = new Uri("/Aristocrat.Monaco.Mgam.UI;component/Settings/MachineSettings.xaml", UriKind.RelativeOrAbsolute)
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
                .MultiProtocolConfiguration.All(x => x.Protocol != CommsProtocol.MGAM))
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
                    DirectoryPort =
                        _properties.GetValue(PropertyNames.DirectoryPort, 0),
                    ServiceName =
                        _properties.GetValue(PropertyNames.ServiceName, string.Empty),
                });
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _properties.SetProperty(PropertyNames.DirectoryPort, settings.DirectoryPort);
            _properties.SetProperty(PropertyNames.ServiceName, settings.ServiceName);

            await Task.CompletedTask;
        }
    }
}
