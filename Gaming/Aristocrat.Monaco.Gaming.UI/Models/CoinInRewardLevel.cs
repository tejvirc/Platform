
namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM.ViewModel;
    using Contracts.Barkeeper;

    /// <summary>
    /// CoinInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CoinInRewardLevel : RewardLevelViewModel
    {
        public override bool ValidateThresholdInCents()
        {
            var thresholdValidate = (ThresholdInCents.CentsToDollars()).Validate(
                false,
                PropertiesManager?.GetValue(GamingConstants.GambleWagerLimit, GamingConstants.DefaultGambleWagerLimit) ?? GamingConstants.DefaultGambleWagerLimit);

            SetError(nameof(ThresholdInCents), thresholdValidate);
            return string.IsNullOrEmpty(thresholdValidate);
        }
    }
}
