namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Tickets;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Kernel;
    using Localization.Properties;
    using Progressives;

    public class ProgressiveSetupAndMetersTicket : AuditTicket
    {
        private readonly IGameDetail _game;
        private readonly long _denomMillicents;

        public ProgressiveSetupAndMetersTicket(IGameDetail game, long denomMillicents, string titleOverride = null)
            : base(titleOverride)
        {
            _game = game;
            _denomMillicents = denomMillicents;

            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.ProgressiveSetupAndMeters));
            }
        }

        public override void AddTicketContent()
        {
            AddLabeledLine(ResourceKeys.GameName, _game.ThemeName);
            AddLabeledLine(ResourceKeys.GameId, _game.Version);
            AddLabeledLine(ResourceKeys.PaytableId, _game.PaytableId);
            AddLabeledLine(ResourceKeys.Denomination, _denomMillicents.MillicentsToDollars().FormattedCurrencyString());
            AddLabeledLine(ResourceKeys.MaxBet, GetMaxBet());
            AddLine(null, null, null);

            var meterManager = ServiceManager.TryGetService<IProgressiveMeterManager>();
            var progressiveProvider = ServiceManager.TryGetService<IProgressiveConfigurationProvider>();
            var progressives = progressiveProvider.ViewProgressiveLevels().ToList();
            var linkedLevels = progressiveProvider.ViewLinkedProgressiveLevels();

            foreach (var progressive in progressives.Where(p => p.GameId == _game.Id && p.Denomination.Contains(_denomMillicents) && p.CurrentState != ProgressiveLevelState.Init))
            {
                string resetValue, incrementRate, hiddenIncrementRate, hiddenValue, overflow, overflowTotal;

                if (progressive.LevelType == ProgressiveLevelType.Selectable && progressive.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.None)
                {
                    continue;
                }

                var currentValue = progressive.CurrentValue.MillicentsToDollars().FormattedCurrencyString() ??
                                       TicketLocalizer.GetString(ResourceKeys.NotAvailable);

                // For linked progressive levels, the platform doesn't know about resetValue/incrementRate which is controlled by Host.
                if (progressive.LevelType == ProgressiveLevelType.LP ||
                    progressive.LevelType == ProgressiveLevelType.Selectable && progressive.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked)
                {
                    resetValue = incrementRate = hiddenIncrementRate = hiddenValue = overflow = overflowTotal = TicketLocalizer.GetString(ResourceKeys.NotAvailable);
                    var linkedLevel = linkedLevels
                        .FirstOrDefault(
                            x => x.LevelName == progressive.AssignedProgressiveId.AssignedProgressiveKey);

                    currentValue = linkedLevel?.Amount.CentsToDollars().FormattedCurrencyString(true) ?? currentValue;
                }
                else
                {
                    if (progressive.AssignedProgressiveId.AssignedProgressiveType ==
                        AssignableProgressiveType.AssociativeSap ||
                        progressive.LevelType == ProgressiveLevelType.Selectable &&
                        progressive.AssignedProgressiveId.AssignedProgressiveType ==
                        AssignableProgressiveType.CustomSap)
                    {
                        var sharedSapLevel = progressiveProvider.ViewSharedSapLevels().FirstOrDefault(
                            x => x.LevelAssignmentKey == progressive.AssignedProgressiveId.AssignedProgressiveKey);

                        currentValue = sharedSapLevel.CurrentValue.MillicentsToDollars().FormattedCurrencyString() ??
                                       TicketLocalizer.GetString(ResourceKeys.NotAvailable);
                        resetValue = sharedSapLevel.ResetValue.MillicentsToDollars().FormattedCurrencyString();
                        incrementRate = (sharedSapLevel.IncrementRate.ToPercentage() / 100M).ToString(
                            "P",
                            TicketLocalizer.CurrentCulture);
                        hiddenIncrementRate =
                            (sharedSapLevel.HiddenIncrementRate.ToPercentage() / 100M).ToString(
                                "P",
                                TicketLocalizer.CurrentCulture);
                        hiddenValue = sharedSapLevel.HiddenValue.MillicentsToDollars().FormattedCurrencyString();
                        overflow = sharedSapLevel.Overflow.MillicentsToDollars().FormattedCurrencyString();
                        overflowTotal = sharedSapLevel.OverflowTotal.MillicentsToDollars().FormattedCurrencyString();
                    }
                    else
                    {
                        resetValue = progressive.ResetValue.MillicentsToDollars().FormattedCurrencyString();
                        incrementRate = (progressive.IncrementRate.ToPercentage() / 100M).ToString(
                            "P",
                            TicketLocalizer.CurrentCulture);
                        hiddenIncrementRate =
                            (progressive.HiddenIncrementRate.ToPercentage() / 100M).ToString(
                                "P",
                                TicketLocalizer.CurrentCulture);
                        hiddenValue = progressive.HiddenValue.MillicentsToDollars().FormattedCurrencyString();
                        overflow = progressive.Overflow.MillicentsToDollars().FormattedCurrencyString();
                        overflowTotal = progressive.OverflowTotal.MillicentsToDollars().FormattedCurrencyString();
                    }
                }

                AddLabeledLine(TicketLocalizer.GetString(ResourceKeys.Progressive), progressive.ProgressivePackName);
                AddLabeledLine(ResourceKeys.ProgressiveLevel, progressive.LevelName);
                AddLabeledLine(ResourceKeys.ResetValue, resetValue);
                AddLabeledLine(ResourceKeys.IncrementRate, incrementRate);
                AddLabeledLine(ResourceKeys.HiddenIncrementRate, hiddenIncrementRate);
                AddLabeledLine(ResourceKeys.HiddenValue, hiddenValue);
                AddLabeledLine(ResourceKeys.TotalHidden,
                    FormattedMeterLifetimeValue(meterManager.GetMeter(progressive.DeviceId, progressive.LevelId,
                    ProgressiveMeters.ProgressiveLevelHiddenTotal)));
                AddLabeledLine(ResourceKeys.OverflowText, overflow);
                AddLabeledLine(ResourceKeys.TotalOverflow, overflowTotal);
                AddLabeledLine(ResourceKeys.CurrentValue, currentValue);
                AddLabeledLine(ResourceKeys.WagerCategoryWageredCaption,
                    FormattedMeterLifetimeValue(meterManager.GetMeter(progressive.DeviceId, progressive.LevelId,
                        ProgressiveMeters.ProgressiveLevelWageredAmount)));
                AddLabeledLine(ResourceKeys.Hit,
                    FormattedMeterLifetimeValue(meterManager.GetMeter(progressive.DeviceId, progressive.LevelId,
                        ProgressiveMeters.ProgressiveLevelWinOccurrence)));
                AddLabeledLine(ResourceKeys.CreditsWonText,
                    FormattedMeterLifetimeValue(meterManager.GetMeter(progressive.DeviceId, progressive.LevelId,
                        ProgressiveMeters.ProgressiveLevelWinAccumulation)));
                AddLine(null, null, null);
            }
        }

        private static string FormattedMeterLifetimeValue(IMeter meter)
        {
            return meter.Classification.CreateValueString(meter.Lifetime);
        }

        private string GetMaxBet()
        {
            var denom = _game.Denominations.Single(x => x.Value == _denomMillicents);
            var denomMultiplier = (decimal)PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            return (_game.MaximumWagerCredits(denom) * _denomMillicents / denomMultiplier).FormattedCurrencyString();
        }
    }
}