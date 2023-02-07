namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.ComponentModel.DataAnnotations;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Contracts.Barkeeper;

    /// <summary>
    /// CoinInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CoinInRewardLevel : RewardLevelViewModel
    {
        private long _thresholdInCents;

        [CustomValidation(typeof(CoinInRewardLevel), nameof(CoinInRewardLevelValidate))]
        public long ThresholdInCents
        {
            get => _thresholdInCents;
            set => SetProperty(ref _thresholdInCents, value, true, nameof(ThresholdInCents));
                
        }

        public static ValidationResult CoinInRewardLevelValidate(long threshhold, ValidationContext context)
        {
            CoinInRewardLevel instance = (CoinInRewardLevel)context.ObjectInstance;
            var errors = threshhold.CentsToDollars().Validate(
                false,
                instance.PropertiesManager?.GetValue(GamingConstants.GambleWagerLimit, GamingConstants.DefaultGambleWagerLimit) ?? GamingConstants.DefaultGambleWagerLimit);

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }
    }
}
