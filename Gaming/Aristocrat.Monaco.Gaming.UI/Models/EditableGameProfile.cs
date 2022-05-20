namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Monaco.UI.Common.Extensions;
    using MVVM.ViewModel;

    public class EditableGameProfile : BaseViewModel, IDisposable
    {
        private readonly bool _enableRtpScaling;

        public EditableGameProfile(string themeId, string themeName, IEnumerable<EditableGameConfiguration> configs, bool enableRtpScaling)
        {
            ThemeId = themeId;
            ThemeName = themeName;
            GameConfigurations = new ObservableCollection<EditableGameConfiguration>(configs.OrderBy(c => c.Denom));
            _enableRtpScaling = enableRtpScaling;

            foreach (var config in GameConfigurations)
            {
                config.PropertyChanged += ConfigOnPropertyChanged;
            }
        }

        public string ThemeId { get; }

        public string ThemeName { get; }

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
            return GameConfigurations.Any(c => c.HasChanges());
        }

        public void Refresh()
        {
            RaisePropertyChanged(nameof(GameConfigurations));
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