namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Jackpot(Progressives) events/transactions.
    /// </summary>
    public class JackpotEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        private readonly double _multiplier;

        public JackpotEventLogAdapter()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = (double)properties.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0);
        }

        public string LogType => EventLogType.Progressive.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var jackpotTransactions = transactionHistory.RecallTransactions<JackpotTransaction>()
                .OrderByDescending(x => x.TransactionDateTime);
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var protocolLinkedProgressiveAdapter =
                ServiceManager.GetInstance().GetService<IProtocolLinkedProgressiveAdapter>();

            var events = from transaction in jackpotTransactions
                         let amount = transaction.WinAmount > 0 && _multiplier > 0
                             ? transaction.WinAmount / _multiplier
                             : transaction.WinAmount
                         let additionalInfo = BuildAdditionalInfos(transaction)
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveWin),
                             amount.FormattedCurrencyString())
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             transaction.TransactionId,
                             transaction.TransactionDateTime,
                             additionalInfo);
            return events;

            (string, string)[] BuildAdditionalInfos(JackpotTransaction transaction)
            {
                var amount = transaction.WinAmount > 0 && _multiplier > 0
                    ? transaction.WinAmount / _multiplier
                    : transaction.WinAmount;
                var multiplier = _multiplier < 1 ? 1 : _multiplier;
                var result = new List<(string, string)>();
                var (wagerCredits, progressiveGroupId) = GetLinkedProgressiveLevelData(protocolLinkedProgressiveAdapter, transaction);
                result.Add(GetDateAndTimeHeader(transaction.TransactionDateTime));
                result.Add((ResourceKeys.Amount, amount.FormattedCurrencyString()));
                var levelProvider = ServiceManager.GetInstance().GetService<IProgressiveLevelProvider>();
                var gameId = transaction.GameId;
                var denomId = transaction.DenomId;
                var level = levelProvider.GetProgressiveLevels().First(
                p => p.GameId == transaction.GameId && p.Denomination.Contains(transaction.DenomId) &&
                         p.ProgressiveId == transaction.ProgressiveId && p.LevelId == transaction.LevelId &&
                         (wagerCredits == 0 || wagerCredits == p.WagerCredits));
                if (level.LevelType == ProgressiveLevelType.LP)
                {
                    result.Add((ResourceKeys.ProgGroupIdLabel, progressiveGroupId.ToString()));
                }

                var game = gameProvider.GetGame(gameId);
                var partNum = game.PaytableName;
                string incrementRate, hiddenIncrementRate, hiddenTotal, resetVal, maxValue, overflow;
                var sharedProvider = ServiceManager.GetInstance().GetService<ISharedSapProvider>();
                if (level.CreationType != LevelCreationType.Default)
                {
                    incrementRate = $"{level.IncrementRate.ToPercentage()}%";
                    hiddenIncrementRate = $"{level.HiddenIncrementRate.ToPercentage()}%";
                    hiddenTotal = transaction.HiddenTotal.MillicentsToDollars().FormattedCurrencyString();
                    resetVal = (transaction.ResetValue / multiplier).FormattedCurrencyString();
                    maxValue = overflow = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                }
                else if (level.LevelType == ProgressiveLevelType.LP ||
                         level.LevelType == ProgressiveLevelType.Selectable &&
                         (AssignableProgressiveType)transaction.AssignableProgressiveType == AssignableProgressiveType.Linked)
                {
                    incrementRate = hiddenIncrementRate = hiddenTotal = maxValue = resetVal = overflow =
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                }
                else if (((AssignableProgressiveType)transaction.AssignableProgressiveType == AssignableProgressiveType.CustomSap ||
                          (AssignableProgressiveType)transaction.AssignableProgressiveType == AssignableProgressiveType.AssociativeSap) &&
                         sharedProvider.ViewSharedSapLevel(transaction.AssignableProgressiveType.ToString(), out var outLevel))
                {
                    incrementRate = $"{outLevel.IncrementRate.ToPercentage()}%";
                    hiddenIncrementRate = $"{outLevel.HiddenIncrementRate.ToPercentage()}%";
                    hiddenTotal = transaction.HiddenTotal.MillicentsToDollars().FormattedCurrencyString();
                    resetVal = (transaction.ResetValue / multiplier).FormattedCurrencyString();
                    maxValue = (outLevel.MaximumValue / multiplier).FormattedCurrencyString();
                    overflow = (transaction.Overflow / multiplier).FormattedCurrencyString();
                }
                else
                {
                    incrementRate = $"{level.IncrementRate.ToPercentage()}%";
                    hiddenIncrementRate = $"{level.HiddenIncrementRate.ToPercentage()}%";
                    hiddenTotal = transaction.HiddenTotal.MillicentsToDollars().FormattedCurrencyString();
                    resetVal = (transaction.ResetValue / multiplier).FormattedCurrencyString();
                    maxValue = (level.MaximumValue / multiplier).FormattedCurrencyString();
                    overflow = (transaction.Overflow / multiplier).FormattedCurrencyString();
                }

                result.Add((ResourceKeys.ProgressiveLevel, $"{level.LevelName}"));
                result.Add((ResourceKeys.IncrementRate, incrementRate));
                result.Add((ResourceKeys.HiddenIncrementRate, hiddenIncrementRate));
                result.Add((ResourceKeys.TotalHidden, hiddenTotal));
                result.Add((ResourceKeys.ResetValue, resetVal));
                result.Add((ResourceKeys.MaxValue, maxValue));
                result.Add((ResourceKeys.OverflowText, overflow));
                result.Add((ResourceKeys.GameName, $"{gameProvider.GetGame(transaction.GameId).ThemeName} / {partNum} / {(denomId / multiplier).FormattedCurrencyString()}"));

                return result.ToArray();
            }
        }

        public long GetMaxLogSequence()
        {
            var transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
            var jackpotTransactions = transactionHistory.RecallTransactions<JackpotTransaction>()
                .OrderByDescending(x => x.LogSequence).ToList();
            return jackpotTransactions.Any() ? jackpotTransactions.First().LogSequence : -1;
        }

        private (long, int) GetLinkedProgressiveLevelData(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            JackpotTransaction transaction)
        {
            IViewableLinkedProgressiveLevel linkedLevel = null;

            if (!string.IsNullOrEmpty(transaction.AssignedProgressiveKey) &&
                transaction.AssignableProgressiveType == (int)AssignableProgressiveType.Linked)
            {
                protocolLinkedProgressiveAdapter?.ViewLinkedProgressiveLevel(
                    transaction.AssignedProgressiveKey,
                    out linkedLevel);
            }

            return (linkedLevel?.WagerCredits ?? 0, linkedLevel?.ProgressiveGroupId ?? 0);
        }
    }
}