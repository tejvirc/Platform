namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class updates Link Progressive configuration for games enabled in operator menu
    /// </summary>
    public class LinkProgressiveLevelConfigurationService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        public LinkProgressiveLevelConfigurationService(IEventBus eventBus,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));

            ConfigureLinkProgressiveLevelInformation();

            eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, _ => ConfigureLinkProgressiveLevelInformation());
        }

        private void ConfigureLinkProgressiveLevelInformation()
        {
            try
            {
                var existingLinkedProgressiveLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels();
                var progressiveLevels = _protocolLinkedProgressiveAdapter.ViewProgressiveLevels();

                var games = _gameProvider.GetEnabledGames();

                //Get all combinations of game variation and progressive level
                var linkProgressiveGamePool = progressiveLevels
                    .Where(level => level.LevelType == ProgressiveLevelType.LP)
                    .Join(games, level => level.GameId, game => game.Id, (level, game) => new { game, level })
                    .Where(
                        gameLevel =>
                            //But only for the denominations specified in progressive config
                            gameLevel.game.ActiveDenominations.ToList().Intersect(gameLevel.level.Denomination).Any() &&
                            (
                                gameLevel.level.Variation == "ALL" ||
                                gameLevel.level.Variation
                                    .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Any(c => string.Equals(c.TrimStart('0'), gameLevel.game.VariationId.TrimStart('0')))
                            )
                    )
                    .GroupBy(
                        gameLevel => new
                        {
                            gameLevel.level.GameId,
                            PackName = gameLevel.level.ProgressivePackName,
                            Denomination = gameLevel.game.ActiveDenominations.First()
                        })
                    .OrderBy(gameLevelPool => gameLevelPool.Key.GameId)
                    .ThenBy(gameLevelPool => gameLevelPool.Key.PackName)
                    .ThenBy(gameLevelPool => gameLevelPool.Key.Denomination)
                    .ToList();

                //Prepare the configuration we want to pass down to the game layer
                var levelsAndAssignments = linkProgressiveGamePool
                    .SelectMany(
                        m => m.Where(w => w.level is ProgressiveLevel)
                            .Select(s =>
                            {
                                var linkedProgressiveLevel = BuildLinkedProgressiveLevel(s.level.LevelId, existingLinkedProgressiveLevels.SingleOrDefault(e => e.LevelId == s.level.LevelId)?.Amount ?? 0);
                                var assignableProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked, linkedProgressiveLevel.LevelName);

                                return new
                                {
                                    Game = s.game,
                                    ProgressiveLevel = BuildProgressiveLevel(s.level, assignableProgressiveId),
                                    LinkedProgressiveLevel = linkedProgressiveLevel
                                };
                            }))
                    .Select(s => new
                    {
                        s.LinkedProgressiveLevel,
                        ProgressiveLevelAssignments = s.Game.ActiveDenominations.Select(d => new ProgressiveLevelAssignment(s.Game, d, s.ProgressiveLevel, s.ProgressiveLevel.AssignedProgressiveId, existingLinkedProgressiveLevels.SingleOrDefault(e => e.LevelId == s.LinkedProgressiveLevel.LevelId)?.Amount ?? 0))
                    })
                    .ToList();

                //Represents the levels being handled by the progressive controller
                var linkedProgressiveLevels = levelsAndAssignments
                    .Select(s => s.LinkedProgressiveLevel)
                    .GroupBy(g => g.LevelName, (k, g) => g.First())
                    .ToList();

                //Binds the progressive levels to game variations
                var levelAssignments = levelsAndAssignments.GroupBy(
                        g => g.LinkedProgressiveLevel.LevelName,
                        (key, g) => g.First())
                    .SelectMany(s => s.ProgressiveLevelAssignments)
                    .ToList();

                if (!linkedProgressiveLevels.Any() && !levelAssignments.Any()) return;

                //Register/update levels with the provider
                if (linkedProgressiveLevels.Any()) _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(linkedProgressiveLevels, ProtocolNames.DACOM);

                //Register denomination/level mappings with game
                if (levelAssignments.Any()) _protocolLinkedProgressiveAdapter.AssignLevelsToGame(levelAssignments, ProtocolNames.DACOM);

                //Notify downstream subscribers
                _eventBus.Publish(new LinkProgressiveLevelConfigurationAppliedEvent());
            }
            catch (Exception ex)
            {
                Log.Error("Unable to determine LP level information from game and progressive configuration", ex);
            }
        }

        private static ProgressiveLevel BuildProgressiveLevel(IViewableProgressiveLevel level, AssignableProgressiveId assignableProgressiveId)
        {
            //Create a copy of the object and change AssignedProgressiveId
            return new ProgressiveLevel
            {
                LevelId = level.LevelId,
                LevelName = level.LevelName,
                AssignedProgressiveId = assignableProgressiveId,
                WagerCredits = level.WagerCredits,
                AllowTruncation = level.AllowTruncation,
                BetOption = level.BetOption,
                CanEdit = level.CanEdit,
                CreationType = level.CreationType,
                CurrentState = level.CurrentState,
                CurrentValue = level.CurrentValue,
                Denomination = level.Denomination.ToList(),
                DeviceId = level.DeviceId,
                ProgressiveId = level.ProgressiveId,
                Errors = level.Errors,
                FundingType = level.FundingType,
                GameId = level.GameId,
                HiddenIncrementRate = level.HiddenIncrementRate,
                HiddenValue = level.HiddenValue,
                IncrementRate = level.IncrementRate,
                InitialValue = level.InitialValue,
                LevelRtp = level.LevelRtp,
                LevelType = level.LevelType,
                LineGroup = level.LineGroup,
                MaximumValue = level.MaximumValue,
                Overflow = level.Overflow,
                OverflowTotal = level.OverflowTotal,
                Probability = level.Probability,
                ProgressivePackId = level.ProgressivePackId,
                ProgressivePackName = level.ProgressivePackName,
                ProgressivePackRtp = level.ProgressivePackRtp == null ? null : new ProgressiveRtp
                {
                    BaseAndReset = level.ProgressivePackRtp.BaseAndReset,
                    BaseAndResetAndIncrement = level.ProgressivePackRtp.BaseAndResetAndIncrement,
                    Increment = level.ProgressivePackRtp.Increment,
                    Reset = level.ProgressivePackRtp.Reset
                },
                ResetValue = level.ResetValue,
                Residual = level.Residual,
                TriggerControl = level.TriggerControl,
                Turnover = level.Turnover,
                Variation = level.Variation
            };
        }

        private static LinkedProgressiveLevel BuildLinkedProgressiveLevel(int levelId, long amount)
        {
            return new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.DACOM,
                LevelId = levelId,

                ProgressiveGroupId = ProgressiveConstants.ProgressiveGroupId,
                Amount = amount,

                Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                CurrentErrorStatus = ProgressiveErrors.None
            };
        }
    }
}