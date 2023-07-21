﻿
namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using CommunityToolkit.Mvvm;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Contracts.Barkeeper;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    /// CashInRewardLevel which binds to BarkeeperConfigurationViewModel for RewardsLevels.
    /// This class is needed as RewardLevel is not publishing PropertyChanged when it is modified.
    /// </summary>
    public class CashInRewardLevel : RewardLevelViewModel
    {
        public override bool ValidateThresholdInCents()
        {
            var thresholdValidate = (ThresholdInCents.CentsToDollars()).Validate(
                false,
                PropertiesManager?.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue) ?? long.MaxValue);

            SetError(nameof(ThresholdInCents), thresholdValidate);
            return string.IsNullOrEmpty(thresholdValidate);
        }
    }
}
