namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Contracts.Configuration;
    using Monaco.UI.Common.Extensions;
    using MVVM.ViewModel;

    public class EditableGameProfile : BaseViewModel, IDisposable
    {
        private readonly bool _enableRtpScaling;
        private IConfigurationRestriction _selectedRestriction;

        public EditableGameProfile(
            string themeId,
            string themeName,
            IEnumerable<EditableGameConfiguration> configs,
            bool enableRtpScaling,
            IConfigurationProvider restrictionsProvider,
            IGameConfigurationProvider gameConfiguration)
        {
            ThemeId = themeId;
            ThemeName = themeName;
            GameConfigurations = new ObservableCollection<EditableGameConfiguration>(configs.OrderBy(c => c.Denom));
            _enableRtpScaling = enableRtpScaling;
            Restrictions = restrictionsProvider.GetByThemeId(themeId).ToList();
            var selected = gameConfiguration.GetActive(themeId);

            OriginalRestriction = _selectedRestriction =
                Restrictions.FirstOrDefault(r => r.Name.Equals(selected?.Name)) ?? Restrictions.FirstOrDefault();
            foreach (var config in GameConfigurations)
            {
                config.PropertyChanged += ConfigOnPropertyChanged;
            }
        }

        public string ThemeId { get; }

        public string ThemeName { get; }

        public IReadOnlyList<IConfigurationRestriction> Restrictions { get; }

        public IConfigurationRestriction SelectedRestriction
        {
            get => _selectedRestriction;
            set => SetProperty(ref _selectedRestriction, value);
        }

        public IConfigurationRestriction OriginalRestriction { get; private set; }

        public IReadOnlyList<IConfigurationRestriction> ValidRestrictions { get; private set; }

        public bool Enabled => GameConfigurations.Any(c => c.Enabled);

        public int EnabledGameConfigurationsCount => GameConfigurations.Count(c => c.Enabled);

        public ObservableCollection<EditableGameConfiguration> GameConfigurations { get; }

        public void Dispose()
        {
            foreach (var config in GameConfigurations)
            {
                config.PropertyChanged -= ConfigOnPropertyChanged;
                config.Dispose();
            }

            GameConfigurations.Clear();
        }

        public bool HasChanges()
        {
            return HasRestrictionChanges() || GameConfigurations.Any(c => c.HasChanges());
        }

        public bool HasRestrictionChanges()
        {
            return OriginalRestriction is not null && !Equals(OriginalRestriction, SelectedRestriction);
        }

        public void UpdateValidRestrictions()
        {
            if (ValidRestrictions is not null)
            {
                return;
            }

            ValidRestrictions = new List<IConfigurationRestriction>(GetValidRestrictions());
            if (ValidRestrictions.Any() && !ValidRestrictions.Contains(_selectedRestriction))
            {
                SelectedRestriction = ValidRestrictions.FirstOrDefault();
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(nameof(GameConfigurations));
        }

        public void Reset()
        {
            SelectedRestriction = OriginalRestriction;
        }

        public void OnSave()
        {
            OriginalRestriction = SelectedRestriction;
        }

        public void AddConfigs(IEnumerable<EditableGameConfiguration> configs)
        {
            foreach (var config in configs)
            {
                GameConfigurations.Add(config);
                config.PropertyChanged += ConfigOnPropertyChanged;
            }

            // If we are scaling RTPs it is much easier to visualize the scaling with the denoms in order
            if (_enableRtpScaling)
            {
                var sortedConfigs = GameConfigurations.OrderBy(g => g.Denom).ToList();
                GameConfigurations.Clear();
                GameConfigurations.AddRange(sortedConfigs);
            }
        }

        private IEnumerable<IConfigurationRestriction> GetValidRestrictions()
        {
            return Restrictions
                .Where(
                    restriction => restriction.RestrictionDetails.Mapping.Any(m => m.Active) ||
                                   restriction.RestrictionDetails.MaxDenomsEnabled.HasValue)
                .Where(AreAllGamesMappedForRestriction);
        }

        private bool AreAllGamesMappedForRestriction(IConfigurationRestriction restriction)
        {
            // Check whether each of the configs in the pack has a game configuration object created.
            // These won't be created if a denomination's max bet exceeds the jurisdiction setting.
            return restriction.RestrictionDetails.Mapping
                .Select(mapping => GameConfigurations.FirstOrDefault(c => c.BaseDenom == mapping.Denomination))
                .All(gameConfig => gameConfig != null);
        }

        private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Enabled))
            {
                return;
            }

            RaisePropertyChanged(nameof(Enabled));
            RaisePropertyChanged(nameof(EnabledGameConfigurationsCount));
        }
    }
}