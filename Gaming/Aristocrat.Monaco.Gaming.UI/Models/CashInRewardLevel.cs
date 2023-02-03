namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Barkeeper;

    /// <summary>
    /// CashInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CashInRewardLevel : ObservableObject
    {
        private bool _enabledField;

        public string Name { get; set; }

        public long ThresholdInCents { get; set; }

        public ColorOptions Color { get; set; }

        public BarkeeperAlertOptions Alert { get; set; }

        public bool Enabled
        {
            get => _enabledField;
            set => SetProperty(ref _enabledField, value, nameof(Enabled));
        }
    }
}
