namespace Aristocrat.Monaco.Hhr.Services.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Client.Messages;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Protocol.Common.Logging;
    using Events;
    using Kernel;
    using log4net;

    public class ProgressiveAssociation : IProgressiveAssociation
    {
        private const int Million = 1000000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveUpdateService _progressiveUpdateService;
        private readonly IPropertiesManager _propertiesManager;

        public ProgressiveAssociation(
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IEventBus eventBus,
            IProgressiveUpdateService progressiveUpdateService,
            IPropertiesManager propertiesManager)
        {
            _gameProvider = gameProvider
                ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter
                ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveUpdateService = progressiveUpdateService
                ?? throw new ArgumentNullException(nameof(progressiveUpdateService));
            _propertiesManager = propertiesManager
                ?? throw new ArgumentNullException(nameof(propertiesManager));

            if (eventBus == null)
            {
                throw new ArgumentNullException(nameof(eventBus));
            }

            var enabledGames = gameProvider.GetEnabledGames();

            foreach (var game in enabledGames)
            {
                if (game.WagerCategories.Count() == game.WagerCategories.Distinct().Count())
                {
                    continue;
                }

                eventBus.Publish(new GameConfigurationNotSupportedEvent(game.Id));

                break;
            }
        }

        /// <summary>
        ///     This function tries to match/associate progressive information received from server to levels defined by game.
        ///     As per current understanding, the matching is done based on fields : WagerAmount, LevelId, ResetValue and
        ///     IncrementRate
        /// </summary>
        public Task<bool> AssociateServerLevelsToGame(ProgressiveInfoResponse serverDefinedLevel,
            GameInfoResponse gameInfo,
            IList<ProgressiveLevelAssignment> levelAssignments)
        {
            var gameLevels = _protocolLinkedProgressiveAdapter.ViewProgressiveLevels()
                .Where(x => x.LevelType == ProgressiveLevelType.LP).ToList();

            Logger.Debug($"[PROG] Server Level - [{serverDefinedLevel.ToJson()}]");


            // Check the max bet for this game denom. If it exceeds the maximum bet setting, then
            // the progressive levels for this denom won't exist.
            var maxBetForServerGame = gameInfo.Denomination * gameInfo.ProgCreditsBet.Max();
            var maxBetForMachine = _propertiesManager.GetValue(
                    AccountingConstants.MaxBetLimit,
                    AccountingConstants.DefaultMaxBetLimit)
                .MillicentsToCents();

            if (maxBetForServerGame > maxBetForMachine)
            {
                Logger.Warn("[PROG] Ignoring server level as this denom exceeds max bet.");
                return Task.FromResult(true);
            }

            // Filtering Game levels based on Level Id and wager amount from server level.
            // Game progressive level Ids are indexed from 0, whereas the ones from server are index from 1.
            var gameLevel = gameLevels.FirstOrDefault(
                level => level.WagerCredits == serverDefinedLevel.ProgCreditsBet
                         && level.LevelId + 1 == serverDefinedLevel.ProgLevel
                         && level.CreationType != LevelCreationType.Default
                         && level.Denomination.Contains(gameInfo.Denomination * GamingConstants.Millicents)
                         && Convert.ToInt32(_gameProvider.GetGame(level.GameId)?.ReferenceId) == gameInfo.GameId);

            if (gameLevel == null || !(gameLevel.IncrementRate == (serverDefinedLevel.ProgContribPercent * Million) &&
                  gameLevel.ResetValue == serverDefinedLevel.ProgResetValue * GamingConstants.Millicents))
            {
                Logger.Error(
                    $"[PROG] Progressive validation failed : {(gameLevel == null ? "Game level not found" : "Level Mismatch")}");
                foreach (var gameLvl in gameLevels)
                {
                    Logger.Error($"[PROG] {gameLvl.ToJson()}");
                }
                return Task.FromResult(false);
            }

            var game = _gameProvider.GetGame(gameLevel.GameId);

            var noOfGameLevels = gameLevels.Count(
                x =>
                    x.Variation.Equals(game.VariationId, StringComparison.InvariantCultureIgnoreCase));

            var noOfServerLevels = gameInfo.ProgressiveIds.Count(x => x != 0);

            if (noOfGameLevels != noOfServerLevels)
            {
                Logger.Error(
                    "[PROG] Progressive validation failed : The number of levels defined in" +
                    $" game variation {game.VariationId} is {noOfGameLevels} and defined in server is {noOfServerLevels}");

                return Task.FromResult(false);
            }

            var linkedLevel = serverDefinedLevel.ToProgressiveLevel();

            levelAssignments.Add(new ProgressiveLevelAssignment(
                game,
                gameLevel.Denomination.First(),
                gameLevel,
                new AssignableProgressiveId(AssignableProgressiveType.Linked, linkedLevel.LevelName),
                gameLevel.ResetValue,
                linkedLevel.WagerCredits));

            if (!string.IsNullOrEmpty(
                    gameLevel.AssignedProgressiveId
                        .AssignedProgressiveKey) || gameLevel.AssignedProgressiveId.AssignedProgressiveType !=
                AssignableProgressiveType.None)
            {
                if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                    gameLevel.AssignedProgressiveId.AssignedProgressiveKey, out var tempLinkedLevel))
                {
                    Logger.Error(
                        "[PROG] Progressive validation failed : Could not find the linked level" +
                        $" Associated key = {gameLevel.AssignedProgressiveId.AssignedProgressiveKey}" +
                        $" Associated type = {gameLevel.AssignedProgressiveId.AssignedProgressiveType}");

                    return Task.FromResult(false);
                }

                if (tempLinkedLevel.ProgressiveGroupId == serverDefinedLevel.ProgressiveId &&
                    tempLinkedLevel.LevelId == serverDefinedLevel.ProgLevel)
                {
                    // if the game progressive has already a correct associated server side then return true.
                    //The scenario will come when we were in the middle of associating the sever and game side
                    //progressive levels, and we reboot during that time
                    UpdateCurrentAmount(linkedLevel);
                    return Task.FromResult(true);
                }

                Logger.Error(
                    "[PROG] Progressive validation failed : Level Mismatch");

                return Task.FromResult(false);
            }

            UpdateCurrentAmount(linkedLevel);

            return Task.FromResult(true);
        }

        private void UpdateCurrentAmount(LinkedProgressiveLevel linkedLevel)
        {
            if(_progressiveUpdateService.IsProgressiveLevelUpdateLocked(linkedLevel))
            {
                return;
            }

            _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                new[] { linkedLevel },
                ProtocolNames.HHR);
        }
    }
}
