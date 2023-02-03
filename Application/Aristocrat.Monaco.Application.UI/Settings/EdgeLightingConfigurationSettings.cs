namespace Aristocrat.Monaco.Application.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts.Localization;
    using Application.Contracts.Settings;
    using Aristocrat.Monaco.Localization.Properties;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     Brightness configuration settings.
    /// </summary>
    public sealed class EdgeLightingConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _propertiesManager;

        private IEdgeLightingController _edgeLightingController;

        private readonly List<StripPriority> _stripPrioritiesForBrightnessLimit =
            ((StripPriority[])Enum.GetValues(typeof(StripPriority)))
            .Where(x => x <= StripPriority.AuditMenu).ToList();

        /// <summary>
        ///     Initializes a new instance of <see cref="EdgeLightingConfigurationSettings"/> class.
        /// </summary>
        public EdgeLightingConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="EdgeLightingConfigurationSettings"/> class.
        /// </summary>
        /// <param name="propertiesManager">Gets or sets properties.</param>
        public EdgeLightingConfigurationSettings(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public string Name => "Lighting";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine;

        /// <inheritdoc />
        public async Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            if (settings is EdgeLightSettings edgeLightSettings)
            {
                await ApplySettings(edgeLightSettings);
            }
            else
            {
                throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }
        }

        /// <inheritdoc />
        public async Task<object> Get(ConfigurationGroup configGroup)
        {
            object settings;

            switch (configGroup)
            {
                case ConfigurationGroup.Machine:
                    settings = await GetEdgeLightSettings();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(configGroup), configGroup, null);
            }

            return settings;
        }

        /// <inheritdoc />
        public async Task Initialize()
        {
            Execute.OnUIThread(
                () =>
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Aristocrat.Monaco.Application.UI;component/Settings/EdgeLightingMachineSettings.xaml",
                            UriKind.RelativeOrAbsolute)
                    };

                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                });

            await Task.CompletedTask;
        }

        private async Task<EdgeLightSettings> GetEdgeLightSettings()
        {
            return await Task.FromResult(
                new EdgeLightSettings
                {
                    LightingOverrideColorSelection = _propertiesManager.GetValue(
                        ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent)),

                    MaximumAllowedEdgeLightingBrightness = _propertiesManager.GetValue(
                        ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                        100),

                    BottomEdgeLightingEnabled = _propertiesManager.GetValue(
                        ApplicationConstants.BottomEdgeLightingOnKey,
                        false)
                });
        }

        private async Task ApplySettings(EdgeLightSettings settings)
        {
            if (settings != null)
            {
                _propertiesManager.SetProperty(
                    ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                    settings.LightingOverrideColorSelection);

                _propertiesManager.SetProperty(
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    settings.MaximumAllowedEdgeLightingBrightness);

                _propertiesManager.SetProperty(
                    ApplicationConstants.BottomEdgeLightingOnKey,
                    settings.BottomEdgeLightingEnabled);

                _edgeLightingController = _edgeLightingController ??
                                          ServiceManager.GetInstance().GetService<IEdgeLightingController>();

                var brightnessLimit = new EdgeLightingBrightnessLimits
                {
                    MaximumAllowed = settings.MaximumAllowedEdgeLightingBrightness,
                    MinimumAllowed = _propertiesManager.GetValue(
                        ApplicationConstants.EdgeLightingBrightnessControlMin,
                        0)
                };

                _stripPrioritiesForBrightnessLimit.ForEach(strip =>
                    _edgeLightingController.SetBrightnessLimits(brightnessLimit, strip));
            }

            await Task.CompletedTask;
        }
    }
}
