
namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// CashInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CashInRewardLevel : RewardLevelViewModel
    {
        private long _thresholdInCents;

        [CustomValidation(typeof(CashInRewardLevel), nameof(ValidateCashInRewardLevel))]
        public override long ThresholdInCents
        {
            get => _thresholdInCents;
            set => SetProperty(ref _thresholdInCents, value, true);
        }

        public static ValidationResult ValidateCashInRewardLevel(long threshhold, ValidationContext context)
        {
            var instance = (CashInRewardLevel)context.ObjectInstance;
            var errors = threshhold
                .CentsToDollars()
                .Validate(
                    false,
                    instance.PropertiesManager?.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue) ?? long.MaxValue
                );
            return string.IsNullOrEmpty(errors) ? ValidationResult.Success : new(errors);
        }
    }
}
