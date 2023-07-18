namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Barkeeper;
    using Contracts.Barkeeper;
    using Localization.Properties;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Newtonsoft.Json;

    /// <summary>
    ///     Barkeeper settings.
    /// </summary>
    internal class BarkeeperSettings : BaseObservableObject, IBarkeeperSettings
    {
        private bool _enabled;
        private RewardLevel _cashInRewardLevel;
        private CoinInRate _coinInRate;


        /// <summary>
        ///     Creates an instance of barkeeper settings.
        /// </summary>
        public BarkeeperSettings()
        {
        }

        /// <summary>
        ///     Creates an instance of barkeeper settings.
        /// </summary>
        /// <param name="data">The data to update the settings.</param>
        public BarkeeperSettings(BarkeeperStorageData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var barkeeperRewardLevels = data.BarkeeperRewardLevels;
            Enabled = barkeeperRewardLevels.Enabled;
            CoinInRate = barkeeperRewardLevels.CoinInStrategy.CoinInRate;
            CashInRewardLevel =
                barkeeperRewardLevels.RewardLevels.FirstOrDefault(x => x.TriggerStrategy == BarkeeperStrategy.CashIn);
            CoinInRewardLevels = new ObservableCollection<RewardLevel>(barkeeperRewardLevels.RewardLevels.Where(x => x.TriggerStrategy == BarkeeperStrategy.CoinIn));
        }

        /// <summary>
        ///     Gets or sets if enabled.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;

            set => SetProperty(ref _enabled, value);
        }

        /// <summary>
        ///     Gets or sets cash in reward level.
        /// </summary>
        public RewardLevel CashInRewardLevel
        {
            get => _cashInRewardLevel;

            set => SetProperty(ref _cashInRewardLevel, value);
        }

        /// <summary>
        ///     Gets <see cref="CashInRewardLevel" /> to text.
        /// </summary>
        [JsonIgnore]
        public string CashInRewardLevelText => BarkeeperRewardLevelToText(CashInRewardLevel);

        /// <summary>
        ///     Gets or sets the coin in reward levels.
        /// </summary>
        public IEnumerable<RewardLevel> CoinInRewardLevels { get; set; } =
            Enumerable.Empty<RewardLevel>();

        /// <summary>
        ///     Gets the reward levels to text.
        /// </summary>
        [JsonIgnore]
        public string CoinInRewardLevelsText =>
            string.Join(Environment.NewLine, CoinInRewardLevels.Select(BarkeeperRewardLevelToText));

        /// <summary>
        ///     Gets or sets the coin in rate.
        /// </summary>
        public CoinInRate CoinInRate
        {
            get => _coinInRate;

            set => SetProperty(ref _coinInRate, value);
        }

        /// <summary>
        ///     Gets the coin in rate text.
        /// </summary>
        [JsonIgnore]
        public string CoinInRateText =>
            CoinInRate is null
                ? string.Empty
                : ToText(
                    new Dictionary<string, string>
                    {
                        [GetOperatorLocalized(ResourceKeys.Enabled)] = CoinInRate.Enabled.ToString(),
                        [GetOperatorLocalized(ResourceKeys.CoinInAmount)] = CentsToCurrency(CoinInRate.Amount),
                        [GetOperatorLocalized(ResourceKeys.SessionRateMs)] = CoinInRate.SessionRateInMs.ToString()
                    });

        private static string BarkeeperRewardLevelToText(RewardLevel level)
        {
            if (level is null)
            {
                return string.Empty;
            }

            return ToText(
                new Dictionary<string, string>
                {
                    [GetOperatorLocalized(ResourceKeys.Enabled)] = level.Enabled.ToString(),
                    [GetOperatorLocalized(ResourceKeys.NameLabel)] = level.Name,
                    [GetOperatorLocalized(ResourceKeys.ButtonAlert)] = level.Alert.ToString(),
                    [GetOperatorLocalized(ResourceKeys.ButtonColor)] = ColorToString(level.Color.ToColor()),
                    [GetOperatorLocalized(ResourceKeys.Threshold)] = CentsToCurrency(level.ThresholdInCents),
                    [GetOperatorLocalized(ResourceKeys.Lights)] = string.Join(", ", level.Led)
                });
        }

        private static string CentsToCurrency(long value)
        {
            return value.CentsToDollars().FormattedCurrencyString();
        }

        private static string ColorToString(Color color)
        {
            return color.IsKnownColor
                ? color.Name
                : color.ToString();
        }

        private static string GetOperatorLocalized(string resourceKey)
        {
            return string.IsNullOrWhiteSpace(resourceKey)
                ? string.Empty
                : Localizer.For(CultureFor.Operator).GetString(resourceKey);
        }

        private static string ToText(Dictionary<string, string> values)
        {
            if (values is null || values.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            foreach (var kvp in values)
            {
                builder.AppendLine($"{kvp.Key} = {kvp.Value}");
            }

            return builder.ToString();
        }
    }
}