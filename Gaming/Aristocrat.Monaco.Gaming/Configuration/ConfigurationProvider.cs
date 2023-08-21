namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Configuration;
    using PackageManifest.Models;

    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly ConcurrentDictionary<string, List<IConfigurationRestriction>> _restrictionsByThemeId =
            new ConcurrentDictionary<string, List<IConfigurationRestriction>>();

        private readonly ConcurrentDictionary<string, IConfigurationRestriction> _defaultRestrictionByThemeId =
            new ConcurrentDictionary<string, IConfigurationRestriction>();

        public string Name => nameof(ConfigurationProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(IConfigurationProvider) };

        public void Initialize()
        {
        }

        public IEnumerable<IConfigurationRestriction> GetByThemeId(string themeId)
        {
            if (themeId == null)
            {
                return Enumerable.Empty<IConfigurationRestriction>();
            }

            return !_restrictionsByThemeId.TryGetValue(themeId, out var configurations)
                ? Enumerable.Empty<IConfigurationRestriction>()
                : configurations;
        }

        public IConfigurationRestriction GetDefaultByThemeId(string themeId)
        {
            if (themeId == null)
            {
                return null;
            }

            return _defaultRestrictionByThemeId.TryGetValue(themeId, out var configurationRestriction)
                ? configurationRestriction
                : null;
        }

        public void Load(string gameThemeId, IEnumerable<Configuration> configurations, Configuration defaultConfiguration = null)
        {
            if (configurations == null)
            {
                throw new ArgumentNullException(nameof(configurations));
            }

            var restrictions = configurations.Select(Map).ToList();

            _restrictionsByThemeId.AddOrUpdate(gameThemeId, restrictions, (key, values) => restrictions);

            if (defaultConfiguration != null)
            {
                var defaultRestriction = Map(defaultConfiguration);
                _defaultRestrictionByThemeId.TryAdd(gameThemeId, defaultRestriction);
            }
        }

        private static IConfigurationRestriction Map(Configuration configuration)
        {
            return new ConfigurationRestriction(configuration.Name)
            {
                RestrictionDetails = new RestrictionDetails
                {
                    Name = configuration.GameConfiguration.Name,
                    Editable = configuration.GameConfiguration.Editable,
                    Id = configuration.GameConfiguration.Id,
                    MinimumPaybackPercent = configuration.GameConfiguration.MinPaybackPercent,
                    MaximumPaybackPercent = configuration.GameConfiguration.MaxPaybackPercent,
                    MinDenomsEnabled = configuration.GameConfiguration.MinDenomsEnabled,
                    MaxDenomsEnabled = configuration.GameConfiguration.MaxDenomsEnabled,
                    Mapping = configuration.GameConfiguration.ConfigurationMapping?.Select(
                            c => new DenomToPaytable
                            {
                                Active = c.Active,
                                Denomination = c.Denomination,
                                Editable = c.Editable,
                                EnabledByDefault = c.Enabled,
                                VariationId = c.Variation,
                                DefaultBetLinePresetId = c.DefaultBetLinePreset,
                                BetLinePresets = c.BetLinePresets.ToList()
                            })
                        .ToList()
                }
            };
        }
    }
}