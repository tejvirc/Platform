namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Common;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Kernel;
    using Localization.Properties;

    public class ProgressiveErrorProvider : IProgressiveErrorProvider, IService
    {
        private static readonly DisplayableMessage UpdateTimeoutMessage = new DisplayableMessage(
            () => Localizer.DynamicCulture().GetString(ResourceKeys.ProgressiveFaultTypes_ProgUpdateTimeout),
            DisplayableMessageClassification.SoftError,
            DisplayableMessagePriority.Immediate,
            ApplicationConstants.ProgressiveUpdateTimeoutGuid);

        private static readonly DisplayableMessage DisconnectedErrorMessage = new DisplayableMessage(
            () => Localizer.DynamicCulture().GetString(ResourceKeys.ProgressiveFaultTypes_ProgDisconnected),
            DisplayableMessageClassification.SoftError,
            DisplayableMessagePriority.Immediate,
            ApplicationConstants.ProgressiveDisconnectErrorGuid);

        private static readonly DisplayableMessage ClaimTimeoutErrorMessage = new DisplayableMessage(
            () => Localizer.DynamicCulture().GetString(ResourceKeys.ProgressiveFaultTypes_ProgCommitTimeout),
            DisplayableMessageClassification.SoftError,
            DisplayableMessagePriority.Immediate,
            ApplicationConstants.ProgressiveCommitTimeoutGuid);

        private static readonly DisplayableMessage MinimumThresholdErrorMessage = new DisplayableMessage(
            () => Localizer.DynamicCulture().GetString(ResourceKeys.ProgressiveFaultTypes_MinimumThresholdNotReached),
            DisplayableMessageClassification.SoftError,
            DisplayableMessagePriority.Immediate,
            ApplicationConstants.MinimumThresholdErrorGuid);

        private readonly IGamePlayState _gamePlayState;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IProgressiveConfigurationProvider _progressiveProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly ILinkedProgressiveProvider _linkedProgressiveProvider;
        private readonly IGameHistory _gameHistory;
        private readonly IEventBus _eventBus;

        public ProgressiveErrorProvider(
            IGamePlayState gamePlayState,
            ISystemDisableManager systemDisable,
            IMessageDisplay messageDisplay,
            IProgressiveConfigurationProvider progressiveProvider,
            IProgressiveGameProvider progressiveGameProvider,
            ILinkedProgressiveProvider linkedProgressiveProvider,
            IGameHistory gameHistory,
            IEventBus eventBus)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _linkedProgressiveProvider = linkedProgressiveProvider ??
                                         throw new ArgumentNullException(nameof(linkedProgressiveProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            RecoverErrors();
        }

        public string Name => typeof(IProgressiveErrorProvider).FullName;

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IProgressiveErrorProvider) };

        public void Initialize()
        {
        }

        public void ReportProgressiveDisconnectedError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            AddError(DisconnectedErrorMessage, levels);
        }

        public void ClearProgressiveDisconnectedError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            RemoveError(ProgressiveErrors.ProgressiveDisconnected, DisconnectedErrorMessage, levels);
        }

        public IEnumerable<int> ViewProgressiveDisconnectedErrors()
        {
            return _progressiveProvider.ViewLinkedProgressiveLevels()
                .Where(x => x.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveDisconnected))
                .Select(x => x.ProgressiveGroupId).Distinct();
        }

        public void ReportProgressiveClaimTimeoutError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            if (!_gamePlayState.InGameRound)
            {
                // We only should post this in game round
                return;
            }

            AddError(ClaimTimeoutErrorMessage, levels);
        }

        public void ClearProgressiveClaimError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            RemoveError(ProgressiveErrors.ProgCommitTimeout, ClaimTimeoutErrorMessage, levels);
        }

        public void ReportMinimumThresholdError(IEnumerable<IViewableProgressiveLevel> levels)
        {
            AddError(MinimumThresholdErrorMessage, levels);
        }

        public void ClearMinimumThresholdError(IEnumerable<IViewableProgressiveLevel> levels)
        {
            RemoveError(ProgressiveErrors.MinimumThresholdNotReached, MinimumThresholdErrorMessage, levels);
        }

        public void ReportProgressiveUpdateTimeoutError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            AddError(UpdateTimeoutMessage, levels);
        }

        public void ClearProgressiveUpdateError(IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            RemoveError(ProgressiveErrors.ProgressiveUpdateTimeout, UpdateTimeoutMessage, levels);
        }

        public IEnumerable<int> ViewProgressiveUpdateTimeoutErrors()
        {
            return _progressiveProvider.ViewLinkedProgressiveLevels()
                .Where(x => x.CurrentErrorStatus.HasFlag(ProgressiveErrors.ProgressiveUpdateTimeout))
                .Select(x => x.ProgressiveGroupId).Distinct();
        }

        public void CheckProgressiveLevelErrors(IEnumerable<IViewableProgressiveLevel> levels)
        {
            var gameErrors = levels.Where(
                    x => x.Errors != ProgressiveErrors.None ||
                         (x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                          _linkedProgressiveProvider.ViewLinkedProgressiveLevel(
                              x.AssignedProgressiveId.AssignedProgressiveKey,
                              out var linkedLevel) && _progressiveProvider.ValidateLinkedProgressive(linkedLevel, x) != ProgressiveErrors.None))
                .SelectMany(l => l.Denomination.Select(d => (GameId: l.GameId, Denom: d, BetOption: l.BetOption))).Distinct();
            ReportAnyProgressiveGameDisabled(gameErrors);
        }

        private void RecoverErrors()
        {
            var existingErrors = _progressiveProvider.ViewConfiguredProgressiveLevels()
                .Where(x => x.Errors.HasFlag(ProgressiveErrors.MinimumThresholdNotReached)).ToList();
            if (existingErrors.Any())
            {
                ReportMinimumThresholdError(existingErrors);
            }

            var linkErrors = _linkedProgressiveProvider.ViewLinkedProgressiveLevels()
                .Where(x => x.CurrentErrorStatus != ProgressiveErrors.None).SelectMany(
                    l => l.CurrentErrorStatus.GetFlags().Select(e => (Error: e, Level: l)))
                .GroupBy(x => x.Error);
            foreach (var error in linkErrors)
            {
                switch (error.Key)
                {
                    case ProgressiveErrors.ProgressiveUpdateTimeout:
                        ReportProgressiveUpdateTimeoutError(error.Select(x => x.Level));
                        break;
                    case ProgressiveErrors.ProgressiveDisconnected:
                        ReportProgressiveDisconnectedError(error.Select(x => x.Level));
                        break;
                }
            }
        }

        private void ReportAnyProgressiveGameDisabled(IEnumerable<(int gameId, long denom, string betOption)> disabledGames)
        {
            foreach (var (gameId, denom, betOption) in (disabledGames ?? Enumerable.Empty<(int, long, string)>()))
            {
                _eventBus.Publish(new ProgressiveGameDisabledEvent(gameId, denom, betOption));
            }
        }

        private void ReportAnyProgressiveGameEnabled(IEnumerable<(string PackName, int GameId, long Denom, string BetOption)> enabledGames)
        {
            foreach (var (packName, gameId, denom, betOption) in enabledGames)
            {
                if (_progressiveProvider.ViewProgressiveLevels(gameId, denom, packName)
                    .All(x => x.Errors == ProgressiveErrors.None &&
                              (x.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.Linked ||
                               (_linkedProgressiveProvider.ViewLinkedProgressiveLevel(
                                    x.AssignedProgressiveId.AssignedProgressiveKey,
                                    out var linkedLevel) &&
                                _progressiveProvider.ValidateLinkedProgressive(linkedLevel, x) == ProgressiveErrors.None))))
                {
                    _eventBus.Publish(new ProgressiveGameEnabledEvent(gameId, denom, betOption));
                }
            }
        }

        private void AddError(DisplayableMessage message, IEnumerable<IViewableProgressiveLevel> levels)
        {
            if ((_gamePlayState.InGameRound &&
                 _progressiveGameProvider.GetActiveProgressiveLevels().Any(
                     activeLevel => levels.Any(level => level.LevelName == activeLevel.LevelName))) ||
                (_gameHistory.IsRecoveryNeeded && _progressiveProvider
                    .ViewConfiguredProgressiveLevels(_gameHistory.CurrentLog.GameId, _gameHistory.CurrentLog.DenomId)
                    .Any(x => levels.Any(level => level.LevelName == x.LevelName))))
            {
                _systemDisable.Disable(message.Id, SystemDisablePriority.Immediate, message.MessageCallback);
            }
            else if (!_systemDisable.CurrentDisableKeys.Contains(message.Id))
            {
                _messageDisplay.DisplayMessage(message);
            }

            var progressiveLevels = levels.Select(x => x.LevelName).ToHashSet();
            var disabledGames = _progressiveProvider.ViewProgressiveLevels()
                .Where(level => progressiveLevels.Contains(level.LevelName))
                .SelectMany(l => l.Denomination.Select(d => (GameId: l.GameId, Denom: d, BetOption: l.BetOption)))
                .Distinct();
            ReportAnyProgressiveGameDisabled(disabledGames);
        }

        private void RemoveError(
            ProgressiveErrors error,
            DisplayableMessage message,
            IEnumerable<IViewableProgressiveLevel> levels)
        {
            if (_systemDisable.CurrentDisableKeys.Contains(message.Id) &&
                !_progressiveGameProvider.GetActiveProgressiveLevels()
                    .Any(activeLevel => activeLevel.Errors.HasFlag(error)))
            {
                _systemDisable.Enable(message.Id);
                if (_progressiveProvider.ViewProgressiveLevels().Any(x => x.Errors.HasFlag(error)))
                {
                    // Display the warning message we still have errors
                    _messageDisplay.DisplayMessage(message);
                }
            }
            else if (!_progressiveProvider.ViewProgressiveLevels().Any(x => x.Errors.HasFlag(error)))
            {
                _messageDisplay.RemoveMessage(message);
            }

            var progressiveLevels = levels.Where(x => x.Errors == ProgressiveErrors.None)
                .Select(x => x.LevelName).ToHashSet();
            if (progressiveLevels.Any())
            {
                var enabledGames = _progressiveProvider.ViewProgressiveLevels()
                    .Where(level => progressiveLevels.Contains(level.LevelName))
                    .SelectMany(l => l.Denomination.Select(d => (PackName: l.ProgressivePackName, GameId: l.GameId, Denom: d, BetOption: l.BetOption)))
                    .Distinct();
                ReportAnyProgressiveGameEnabled(enabledGames);
            }
        }

        private void AddError(DisplayableMessage message, IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            if ((_gamePlayState.InGameRound &&
                _progressiveGameProvider.GetActiveLinkedProgressiveLevels().Any(
                    activeLevel => levels.Any(level => level.LevelName == activeLevel.LevelName))) ||
                (_gameHistory.IsRecoveryNeeded && _progressiveProvider
                    .ViewConfiguredProgressiveLevels(_gameHistory.CurrentLog.GameId, _gameHistory.CurrentLog.DenomId)
                    .Any(
                        x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                             levels.Any(
                                 linked => linked.LevelName == x.AssignedProgressiveId.AssignedProgressiveKey))))
            {
                _systemDisable.Disable(message.Id, SystemDisablePriority.Immediate, message.MessageCallback);
            }
            else if (!_systemDisable.CurrentDisableKeys.Contains(message.Id))
            {
                _messageDisplay.DisplayMessage(message);
            }

            var progressiveLevels = levels.Select(x => x.LevelName).ToHashSet();
            var disabledGames = _progressiveProvider.ViewProgressiveLevels()
                .Where(x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                            progressiveLevels.Contains(x.AssignedProgressiveId.AssignedProgressiveKey))
                .SelectMany(l => l.Denomination.Select(d => (GameId: l.GameId, Denom: d, BetOption: l.BetOption)))
                .Distinct();
            ReportAnyProgressiveGameDisabled(disabledGames);
        }

        private void RemoveError(
            ProgressiveErrors error,
            DisplayableMessage message,
            IEnumerable<IViewableLinkedProgressiveLevel> levels)
        {
            if (_systemDisable.CurrentDisableKeys.Contains(message.Id) &&
                !_progressiveGameProvider.GetActiveLinkedProgressiveLevels()
                    .Any(activeLevel => activeLevel.CurrentErrorStatus.HasFlag(error)))
            {
                _systemDisable.Enable(message.Id);
                if (_progressiveProvider.ViewLinkedProgressiveLevels().Any(x => x.CurrentErrorStatus.HasFlag(error)))
                {
                    // Display the warning message we still have errors
                    _messageDisplay.DisplayMessage(message);
                }
            }
            else if (!_progressiveProvider.ViewLinkedProgressiveLevels().Any(x => x.CurrentErrorStatus.HasFlag(error)))
            {
                _messageDisplay.RemoveMessage(message);
            }

            var progressiveLevels = levels.Where(x => x.CurrentErrorStatus == ProgressiveErrors.None)
                .Select(x => x.LevelName).ToHashSet();
            if (progressiveLevels.Any())
            {
                var enabledGames = _progressiveProvider.ViewProgressiveLevels()
                    .Where(x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                                progressiveLevels.Contains(x.AssignedProgressiveId.AssignedProgressiveKey))
                    .SelectMany(l => l.Denomination.Select(d => (PackName: l.ProgressivePackName, GameId: l.GameId, Denom: d, BetOption: l.BetOption)))
                    .Distinct();
                ReportAnyProgressiveGameEnabled(enabledGames);
            }
        }
    }
}