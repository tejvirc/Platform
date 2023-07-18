namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts.Barkeeper;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class RewardLevelViewModel : ObservableObject
    {
        private bool _enabledField;
        private long _thresholdInCents;
        private IPropertiesManager _propertiesManager;

        public string Name { get; set; }

        public long ThresholdInCents
        {
            get => _thresholdInCents;
            set
            {
                if (SetProperty(ref _thresholdInCents, value, nameof(ThresholdInCents)))
                {
                    ValidateThresholdInCents();
                }
            }
        }

        public bool ThresholdError => HasErrors;

        protected IPropertiesManager PropertiesManager => _propertiesManager ??= ServiceManager.GetInstance().TryGetService<IPropertiesManager>();

        public ColorOptions Color { get; set; }

        public BarkeeperAlertOptions Alert { get; set; }

        public bool Enabled
        {
            get => _enabledField;
            set => SetProperty(ref _enabledField, value, nameof(Enabled));
        }

        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }
        }

        public abstract bool ValidateThresholdInCents();
    }
}
