namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.ComponentModel.DataAnnotations;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using Contracts.Barkeeper;

    /// <summary>
    /// CashInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CashInRewardLevel : RewardLevelViewModel
    {
        private long _thresholdInCents;

        [CustomValidation(typeof(CashInRewardLevel), nameof(CashInRewardLevelValidate))]
        public long ThresholdInCents
        {
            get => _thresholdInCents;
            set => SetProperty(ref _thresholdInCents, value, true, nameof(ThresholdInCents));
        }

        public static ValidationResult CashInRewardLevelValidate(long threshhold, ValidationContext context)
        {
            CashInRewardLevel instance = (CashInRewardLevel)context.ObjectInstance;
            var errors = ((decimal)threshhold.CentsToDollars()).Validate(
                false,
                instance.PropertiesManager?.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue) ?? long.MaxValue);

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }
    }
}
