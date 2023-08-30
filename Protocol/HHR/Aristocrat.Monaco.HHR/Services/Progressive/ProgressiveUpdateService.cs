namespace Aristocrat.Monaco.Hhr.Services.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Client.Communication;
    using Client.Messages;
    using Events;
    using Storage.Helpers;
    using Storage.Models;
    using Kernel;
    using Aristocrat.Monaco.Protocol.Common.Logging;
    using log4net;

    /// <summary>
    ///     This service is responsible for handling incoming progressive updates
    ///     including when progressive hit is in progress or recovery.
    ///     When progressive hit is already in progress for an incoming update,
    ///     the updated amounts are persisted and when the progressives have been awarded
    ///     the pending updates are applied.
    /// </summary>
    public class ProgressiveUpdateService : IProgressiveUpdateService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly IEventBus _eventBus;

        //This message contains which all progressives are hit and the number of times they are hit.
        //This will help us to store the updates for that level onto a backup till all the
        //progressives for that progId is awarded to the player.
        // Since HHR has no concept of ProgressiveGroupID the server side unique progressive ID
        // is stored in ProgressiveGroupId
        private readonly Dictionary<long, ProgressiveUpdateEntity> _progressivesUpdatedValue =
            new ();

        private readonly IProgressiveUpdateEntityHelper _progressiveUpdateEntityHelper;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IGameHistory _gameHistory;

        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly List<IDisposable> _subscribersList = new List<IDisposable>();

        private readonly object _sync = new object();
        private readonly ITransactionHistory _transactions;
        private bool _disposed;
        private bool _waitingForPrizeInformation;

        public ProgressiveUpdateService(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IEventBus eventBus,
            IProgressiveBroadcastService progressiveBroadcastService,
            ITransactionHistory transactions,
            IProgressiveUpdateEntityHelper progressiveUpdateEntityHelper,
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider,
            IGameHistory gameHistory)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            var tmpProgressiveBroadcastService = progressiveBroadcastService ??
                                                 throw new ArgumentNullException(nameof(progressiveBroadcastService));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _progressiveUpdateEntityHelper = progressiveUpdateEntityHelper ??
                                             throw new ArgumentNullException(nameof(progressiveUpdateEntityHelper));

            _subscribersList.Add(tmpProgressiveBroadcastService.ProgressiveUpdates.Subscribe(UpdateProgressive));

            _eventBus.Subscribe<LinkedProgressiveResetEvent>(this, Handle);
            _eventBus.Subscribe<PrizeInformationEvent>(this, Handle);
            _eventBus.Subscribe<GameEndedEvent>(this, Handle);
            _eventBus.Subscribe<GamePlayStateChangedEvent>(this, evt =>
            {
                if (evt.CurrentState == PlayState.PrimaryGameEscrow)
                {
                    LockProgressiveUpdates(true);
                }
            });
            _eventBus.Subscribe<OutcomeFailedEvent>(this,
                _ => {
                {
                    LockProgressiveUpdates(false);
                } });

            Initialize();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsProgressiveLevelUpdateLocked(LinkedProgressiveLevel linkedLevel)
        {
            lock (_sync)
            {
                if (!_progressivesUpdatedValue.ContainsKey(linkedLevel.ProgressiveGroupId))
                {
                    return false;
                }

                // Since this is called from initialization of progressives, we do not need to
                // save the update as it will be received again on startup.
                _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].CurrentValue = linkedLevel.Amount;
                Logger.Debug($"Updated pending level current amount: {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].ToJson()}");

                return true;
            }
        }

        private void Initialize()
        {
            LoadProgressiveUpdatesThatAreInProgress();
        }

        //Update the dictionary
        private void Handle(PrizeInformationEvent evt)
        {
            lock (_sync)
            {
                Logger.Debug("Handling PrizeInformationEvent");

                if (!evt.PrizeInformation.ProgressiveLevelsHit.Any())
                {
                    LockProgressiveUpdates(false);

                    return;
                }

                var progressiveLevels = evt.PrizeInformation.GetActiveProgressiveLevelsForWager(
                    _protocolLinkedProgressiveAdapter,
                    _gameProvider);

                // TODO: Find the level instead of having nested loops.
                foreach (var (levelId, count) in evt.PrizeInformation.ProgressiveLevelsHit)
                foreach (var progressiveLevel in progressiveLevels)
                {
                    if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                        progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        out var linkedLevel))
                    {
                        continue;
                    }

                    if (linkedLevel.LevelId != levelId + 1)
                    {
                        continue;
                    }

                    var currentProgressiveAmount = evt.PrizeInformation.ProgressiveWin.ElementAt(levelId);

                    if (_progressivesUpdatedValue.ContainsKey(linkedLevel.ProgressiveGroupId))
                    {
                       _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].RemainingHitCount = count;
                       _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].LockUpdates = true;
                       _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].AmountToBePaid = currentProgressiveAmount;

                        Logger.Debug(
                            $"Progressive Win {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].AmountToBePaid}." +
                            $" Existing Id = {linkedLevel.ProgressiveGroupId} " +
                            $"currentValue = {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].CurrentValue} updated");
                    }
                    else
                    {
                        Logger.Debug(
                            $"Progressive Win {currentProgressiveAmount}. Added Id = {linkedLevel.ProgressiveGroupId}");
                        _progressivesUpdatedValue.Add(
                            linkedLevel.ProgressiveGroupId,
                            new ProgressiveUpdateEntity
                            {
                                ProgressiveId = linkedLevel.ProgressiveGroupId,
                                RemainingHitCount = count,
                                CurrentValue =
                                    progressiveLevel.ResetValue
                                        .MillicentsToCents(), // After paying this hit, we update the current(update)value to ResetValue
                                AmountToBePaid = currentProgressiveAmount,
                                LockUpdates = true
                            });
                    }

                    Logger.Debug(
                        $"Progressive updated/added = {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].ToJson()}");

                    UpdateLinkedProgressiveLevels(
                        linkedLevel.ProgressiveGroupId,
                        linkedLevel.LevelId,
                        currentProgressiveAmount);
                }

                UpdateAndRemoveLevelsThatAreNotHit();
                SaveProgressivesUpdatesThatAreStillInProgress();

                _waitingForPrizeInformation = false;
            }
        }

        private void SaveProgressivesUpdatesThatAreStillInProgress()
        {
            lock (_sync)
            {
                var saveToDb = _progressivesUpdatedValue.Values.Where(x => x.LockUpdates).ToList();
                _progressiveUpdateEntityHelper.ProgressiveUpdates = saveToDb;
                Logger.Debug($"Saved pending progressive awards/updates : {saveToDb.Count}");
            }
        }

        private void LoadProgressiveUpdatesThatAreInProgress()
        {
            lock (_sync)
            {
                _progressivesUpdatedValue.Clear();

                var existingUpdates = _progressiveUpdateEntityHelper.ProgressiveUpdates.ToList();

                foreach (var progressiveUpdate in existingUpdates)
                {
                    var linkedLevel = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels().FirstOrDefault(
                        x => x.ProgressiveGroupId == progressiveUpdate.ProgressiveId);

                    if (linkedLevel == null)
                    {
                        Logger.Debug($"linkedLevel not found for ProgressiveGroupId {progressiveUpdate.ProgressiveId}");
                        continue;
                    }

                    _progressivesUpdatedValue.Add(progressiveUpdate.ProgressiveId, progressiveUpdate);

                    Logger.Debug(
                        $"Progressive added = {_progressivesUpdatedValue[progressiveUpdate.ProgressiveId].ToJson()}");

                    //Check if the outcome is present for the last game round.
                    //If it not then it means we would still receive the PrizeInformationEvent/OutcomeFailedEvent
                    //In this scenario do not apply the updates
                    var outcomesPresent = _gameHistory.CurrentLog?.Outcomes.Any() ?? false;

                    if (!outcomesPresent)
                    {
                        Logger.Debug("Have not received the outcomes, so no need to update the progressive levels");
                        continue;
                    }

                    var amountToBeUpdated = progressiveUpdate.RemainingHitCount > 0
                        ? progressiveUpdate.AmountToBePaid
                        : progressiveUpdate.CurrentValue;

                    Logger.Debug(
                        $"Applying the amountToBeUpdated={amountToBeUpdated} " +
                        $"for Level={linkedLevel.LevelId} and ProgressiveGroupId={linkedLevel.ProgressiveGroupId}" +
                        $" RemainingHitCount = {progressiveUpdate.RemainingHitCount}");

                    UpdateLinkedProgressiveLevels(
                        linkedLevel.ProgressiveGroupId,
                        linkedLevel.LevelId,
                        amountToBeUpdated);
                }

                Logger.Debug($"Loaded pending progressive awards/updates : {_progressivesUpdatedValue.Count}");
            }
        }

        private void UpdateProgressive(GameProgressiveUpdate progressivePrize)
        {
            lock (_sync)
            {
                Logger.Debug(
                    $"Got update : Id={progressivePrize.Id}, amount = {progressivePrize.Amount} _waitingForPrizeInformation = {_waitingForPrizeInformation}");

                var linkedLevel = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels().FirstOrDefault(
                    x => x.ProgressiveGroupId == progressivePrize.Id);

                if (linkedLevel == null)
                {
                    Logger.Debug($"linkedLevel not found for ProgressiveGroupId {progressivePrize.Id} not saving it");
                    return;
                }

                if (!_progressivesUpdatedValue.ContainsKey(progressivePrize.Id))
                {
                    UpdateLinkedProgressiveLevels(
                        linkedLevel.ProgressiveGroupId,
                        linkedLevel.LevelId,
                        progressivePrize.Amount);
                    return;
                }

                // if the progressive update is for an already hit ID, add to dict and store it,
                // so that it can be updated once the processing for that level is done
                Logger.Debug(
                    $"Existing Id = {progressivePrize.Id} updated with = {progressivePrize.Amount}");
                _progressivesUpdatedValue[progressivePrize.Id].CurrentValue = progressivePrize.Amount;
                _progressivesUpdatedValue[progressivePrize.Id].LockUpdates = true;

                SaveProgressivesUpdatesThatAreStillInProgress();
            }
        }

        private void Handle(GameEndedEvent endedEvent)
        {
            lock (_sync)
            {
                if (_progressivesUpdatedValue.Any(x => x.Value.RemainingHitCount == 0))
                {
                    Logger.Error("There are some progressives with RemainingHitCount as 0");
                    LockProgressiveUpdates(false);
                }

                if (!_progressivesUpdatedValue.Any())
                {
                    return;
                }

                var progressivesUpdatedValueDisplay = _progressivesUpdatedValue.Aggregate(
                    string.Empty,
                    (current, progressive) =>
                        current +
                        $" progressive Id {progressive.Key}, times remaining {progressive.Value.RemainingHitCount}");

                Logger.Error(
                    "The Game round has ended, progressives" +
                    $" not updated for {_progressivesUpdatedValue.Count}," +
                    $" these are {progressivesUpdatedValueDisplay}");
            }
        }

        private void Handle(LinkedProgressiveResetEvent resetEvent)
        {
            var transaction = _transactions.RecallTransaction<JackpotTransaction>(resetEvent.TransactionId);

            if (transaction == null)
            {
                Logger.Error($"Cannot find JackpotTransaction for transactionId {resetEvent.TransactionId}");
                return;
            }

            //Must have info about what level was hit and how many times.
            if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                transaction.AssignedProgressiveKey,
                out var linkedLevel))
            {
                Logger.Error($"Cannot find linkedLevel for key {transaction.AssignedProgressiveKey}");
                return;
            }

            lock (_sync)
            {
                if (!_progressivesUpdatedValue.ContainsKey(linkedLevel.ProgressiveGroupId))
                {
                    return;
                }

                //A level can be hit multiple times, if the number of times hit > 1,
                // then reduce it by one
                if (_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].RemainingHitCount > 1)
                {
                    var valueInCents = transaction.ResetValue.MillicentsToCents();

                    _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].RemainingHitCount -= 1;
                    _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].AmountToBePaid = valueInCents;
                    _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].LockUpdates = true;

                    Logger.Debug($"Processing successive hit at Reset Value = {valueInCents}");

                    // for each of the level hit more than once, for the second hit onwards,
                    // make sure that the ResetValue is awarded.
                    UpdateLinkedProgressiveLevels(
                        linkedLevel.ProgressiveGroupId,
                        linkedLevel.LevelId,
                        valueInCents);

                    SaveProgressivesUpdatesThatAreStillInProgress();

                    return;
                }

                var linkedLevelToBeUpdated = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                    .FirstOrDefault(
                        x => x.ProgressiveGroupId == linkedLevel.ProgressiveGroupId);

                if (linkedLevelToBeUpdated == null || _progressivesUpdatedValue == null ||
                    !_progressivesUpdatedValue.TryGetValue(linkedLevel.ProgressiveGroupId, out var progressiveUpdate))
                {
                    Logger.Error(
                        $"Cannot find linkedLevelToBeUpdated or level saved in the dictionary for key {linkedLevel.ProgressiveGroupId}");
                    return;
                }

                Logger.Debug($"Progressive awarded. Update amount = {progressiveUpdate.CurrentValue}");

                UpdateLinkedProgressiveLevels(
                    linkedLevelToBeUpdated.ProgressiveGroupId,
                    linkedLevelToBeUpdated.LevelId,
                    progressiveUpdate.CurrentValue);

                //remove it from the dictionary so that any further updates can be made directly
                _progressivesUpdatedValue.Remove(linkedLevel.ProgressiveGroupId);

                Logger.Debug($"Progressive awarded. Remove Id = {linkedLevel.ProgressiveGroupId}");

                SaveProgressivesUpdatesThatAreStillInProgress();
            }
        }

        private void UpdateLinkedProgressiveLevels(int progId, int levelId, long valueInCents)
        {
            var linkedLevel = new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.HHR,
                ProgressiveGroupId = progId,
                LevelId = levelId,
                Amount = valueInCents,
                Expiration = DateTime.MaxValue,
                CurrentErrorStatus = ProgressiveErrors.None
            };

            _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                new[] {linkedLevel},
                ProtocolNames.HHR);

            Logger.Debug(
                $"Updated linked progressive level: ProtocolName={linkedLevel.ProtocolName} " +
                $"ProgressiveGroupId={linkedLevel.ProgressiveGroupId} LevelName={linkedLevel.LevelName} " +
                $"LevelId={linkedLevel.LevelId} Amount={linkedLevel.Amount} " +
                $"ClaimStatus={linkedLevel.ClaimStatus} CurrentErrorStatus={linkedLevel.CurrentErrorStatus} " +
                $"Expiration={linkedLevel.Expiration}");
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var disposable in _subscribersList) disposable.Dispose();

                _progressivesUpdatedValue.Clear();

                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void UpdateAndRemoveLevelsThatAreNotHit()
        {
            lock (_sync)
            {
                var nonHitLevels = _progressivesUpdatedValue.Where(x => x.Value.RemainingHitCount == 0).ToList();

                foreach (var update in nonHitLevels)
                {
                    var linkedLevelToBeUpdated = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                        .FirstOrDefault(
                            x => x.ProgressiveGroupId == update.Value.ProgressiveId);

                    if (linkedLevelToBeUpdated == null)
                    {
                        _progressivesUpdatedValue.Remove(update.Value.ProgressiveId);
                        Logger.Debug(
                            $"linkedLevel not found for ProgressiveGroupId {update.Value.ProgressiveId} " +
                            $"removed = {_progressivesUpdatedValue[update.Value.ProgressiveId].ToJson()}");

                        continue;
                    }

                    Logger.Debug(
                        $"Applying the Update of Amount={update.Value.CurrentValue} from {_progressivesUpdatedValue[linkedLevelToBeUpdated.ProgressiveGroupId].ToJson()} ");

                    UpdateLinkedProgressiveLevels(
                        linkedLevelToBeUpdated.ProgressiveGroupId,
                        linkedLevelToBeUpdated.LevelId,
                        update.Value.CurrentValue);

                    _progressivesUpdatedValue.Remove(linkedLevelToBeUpdated.ProgressiveGroupId);
                }
            }
        }

        private void PopulateDictionaryWithCurrentGameRoundLevels()
        {
            lock (_sync)
            {
                if (_progressivesUpdatedValue.Any())
                {
                    Logger.Error("There are some pending updates");
                }

                _progressivesUpdatedValue.Clear();

                var betCreditsSaved = _propertiesManager.GetValue(GamingConstants.SelectedBetCredits, 0L);

                var viewableProgressiveLevels = _protocolLinkedProgressiveAdapter.GetActiveProgressiveLevels()
                    .Where(x => x.WagerCredits == betCreditsSaved);

                foreach (var progressiveLevel in viewableProgressiveLevels)
                {
                    if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                        progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                        out var linkedLevel))
                    {
                        continue;
                    }

                    if (_progressivesUpdatedValue.ContainsKey(linkedLevel.ProgressiveGroupId))
                    {
                        _progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].LockUpdates = true;

                        Logger.Debug(
                            $"Existing Id = {linkedLevel.ProgressiveGroupId} update added - {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].ToJson()}");
                    }
                    else
                    {
                        _progressivesUpdatedValue.Add(
                            linkedLevel.ProgressiveGroupId,
                            new ProgressiveUpdateEntity
                            {
                                ProgressiveId = linkedLevel.ProgressiveGroupId,
                                RemainingHitCount = 0,
                                CurrentValue = progressiveLevel.CurrentValue.MillicentsToCents(), // Value that must be updated once we receive the prize information
                                AmountToBePaid = 0,
                                LockUpdates = true
                            });

                        Logger.Debug(
                            $"New progressive update added - {_progressivesUpdatedValue[linkedLevel.ProgressiveGroupId].ToJson()}");
                    }
                }

                SaveProgressivesUpdatesThatAreStillInProgress();
            }
        }

        private void LockProgressiveUpdates(bool value)
        {
            lock (_sync)
            {
                Logger.Debug($"Setting _waitingForPrizeInformation to {value}");

                _waitingForPrizeInformation = value;

                if (_waitingForPrizeInformation)
                {
                    PopulateDictionaryWithCurrentGameRoundLevels();

                    return;
                }

                UpdateAndRemoveLevelsThatAreNotHit();
                SaveProgressivesUpdatesThatAreStillInProgress();
            }
        }
    }
}