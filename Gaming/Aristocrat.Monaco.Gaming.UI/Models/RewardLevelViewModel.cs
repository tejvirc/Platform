namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts.Barkeeper;
    using Aristocrat.Monaco.Kernel;
    using CommunityToolkit.Mvvm.ComponentModel;

    public abstract class RewardLevelViewModel : ObservableValidator
    {
        private bool _enabledField;
        private IPropertiesManager _propertiesManager;

        public string Name { get; set; }

        public abstract long ThresholdInCents { get; set; }

        public bool ThresholdError => HasErrors;

        protected IPropertiesManager PropertiesManager => _propertiesManager ??= ServiceManager.GetInstance().TryGetService<IPropertiesManager>();

        public ColorOptions Color { get; set; }

        public BarkeeperAlertOptions Alert { get; set; }

        public bool Enabled
        {
            get => _enabledField;
            set => SetProperty(ref _enabledField, value, nameof(Enabled));
        }
    }
}
