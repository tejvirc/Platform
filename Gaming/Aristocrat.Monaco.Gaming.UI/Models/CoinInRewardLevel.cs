
namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// CoinInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CoinInRewardLevel : RewardLevelViewModel
    {
        private long _thresholdInCents;

        [CustomValidation(typeof(CoinInRewardLevel), nameof(ValidateCoinInRewardLevel))]
        public long ThresholdInCents
        {
            get => _thresholdInCents;
            set => SetProperty(ref _thresholdInCents, value, true, nameof(ThresholdInCents));
        }

        public static ValidationResult ValidateCoinInRewardLevel(long threshhold, ValidationContext context)
        {
            var instance = (CoinInRewardLevel)context.ObjectInstance;
            var errors = threshhold
                .CentsToDollars()
                .Validate(
                    false,
                    instance.PropertiesManager?.GetValue(GamingConstants.GambleWagerLimit, GamingConstants.DefaultGambleWagerLimit) ?? GamingConstants.DefaultGambleWagerLimit
                );
            return string.IsNullOrEmpty(errors) ? ValidationResult.Success : new(errors);
        }
    }
}
