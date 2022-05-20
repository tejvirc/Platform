namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts.Configuration;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    public class GameConfigurationProvider : IGameConfigurationProvider
    {
        private const string RestrictionMapKey = @"RestrictionMap";

        private readonly IPersistentStorageAccessor _accessor;
        private readonly IConfigurationProvider _configurations;
        private readonly IPropertiesManager _properties;

        private readonly Dictionary<string, string> _restrictionMap;

        public GameConfigurationProvider(
            IConfigurationProvider configurations,
            IPersistentStorageManager storage,
            IPropertiesManager properties)
        {
            _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
            var persistentStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _accessor = persistentStorage.GetAccessor(StorageLevel, GetType().ToString());

            _restrictionMap = _accessor.GetList<RestrictionMap>(RestrictionMapKey)
                .ToDictionary(m => m.ThemeId, m => m.Name);
        }

        private PersistenceLevel StorageLevel => _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
            ? PersistenceLevel.Critical
            : PersistenceLevel.Static;

        public string Name => nameof(GameConfigurationProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameConfigurationProvider) };

        public void Initialize()
        {
        }

        public IConfigurationRestriction GetActive(string themeId)
        {
            if (themeId == null) return null;

            if (!_restrictionMap.TryGetValue(themeId, out var name))
            {
                var defaultRestriction = _configurations.GetDefaultByThemeId(themeId);
                if (defaultRestriction != null)
                {
                    Apply(themeId, defaultRestriction);
                }
                return defaultRestriction;
            }

            var restrictions = _configurations.GetByThemeId(themeId);

            return restrictions.FirstOrDefault(r => r.Name == name);
        }
        
        public void Apply(string themeId, IConfigurationRestriction restriction)
        {
            _restrictionMap[themeId] = restriction.Name;

            _accessor.UpdateList(
                RestrictionMapKey,
                _restrictionMap.Select(m => new RestrictionMap { ThemeId = m.Key, Name = m.Value }).ToList());
        }
    }
}