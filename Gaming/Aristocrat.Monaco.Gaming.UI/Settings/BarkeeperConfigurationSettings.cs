namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts;
    using Application.Contracts.Settings;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Barkeeper;
    using Contracts;
    using Contracts.Barkeeper;
    using Kernel;

    /// <summary>
    ///     Barkeeper configuration settings.
    /// </summary>
    public sealed class BarkeeperConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;
        /// <inheritdoc />
        public string Name => "Barkeeper";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BarkeeperConfigurationSettings"/> class.
        /// </summary>
        public BarkeeperConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BarkeeperConfigurationSettings"/> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager"/> instance.</param>
        public BarkeeperConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
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
                            "/Aristocrat.Monaco.Gaming.UI;component/Settings/BarkeeperMachineSettings.xaml",
                            UriKind.RelativeOrAbsolute)
                    };

                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                });

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!_properties.GetValue(ApplicationConstants.BarkeeperEnabled, false))
                return;

            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            if (settings is BarkeeperSettings barkeeperSettings)
            {
                await ApplySettings(barkeeperSettings);
            }
            else
            {
                throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }
        }

        /// <inheritdoc />
        public async Task<object> Get(ConfigurationGroup configGroup)
        {
            if (!_properties.GetValue(ApplicationConstants.BarkeeperEnabled, false))
                return null;

            object settings;

            if (configGroup == ConfigurationGroup.Machine)
            {
                settings = await GetBarkeeperSettings();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup), configGroup, null);
            }

            return settings;
        }

        private static Task<BarkeeperSettings> GetBarkeeperSettings()
        {
            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var rewardLevels = BarkeeperRewardLevelHelper.ToRewards(
                manager.GetValue(GamingConstants.BarkeeperRewardLevels, string.Empty));
            var settings = new BarkeeperSettings
            {
                Enabled = rewardLevels.Enabled,
                CoinInRate = rewardLevels.CoinInStrategy.CoinInRate,
                CoinInRewardLevels = rewardLevels.RewardLevels.Where(x => x.TriggerStrategy == BarkeeperStrategy.CoinIn).ToList(),
                CashInRewardLevel = rewardLevels.RewardLevels.FirstOrDefault(x => x.TriggerStrategy == BarkeeperStrategy.CashIn)
            };

            return Task.FromResult(settings);
        }

        private static Task ApplySettings(BarkeeperSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var rewardLevels = new BarkeeperRewardLevels
            {
                Enabled = settings.Enabled,
                CoinInStrategy = new CoinInStrategy() { CoinInRate = settings.CoinInRate },
                CashInStrategy = new CashInStrategy(),
                RewardLevels = settings.CoinInRewardLevels.Append(settings.CashInRewardLevel).ToArray()
            };

            manager.SetProperty(
                GamingConstants.BarkeeperRewardLevels,
                rewardLevels.ToXml());

            return Task.CompletedTask;
        }
    }
}
