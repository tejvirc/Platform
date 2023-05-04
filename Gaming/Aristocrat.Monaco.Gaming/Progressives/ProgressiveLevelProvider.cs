namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using log4net;
    using PackageManifest.Models;
    using ProgressiveRtp = Contracts.Progressives.ProgressiveRtp;

    public class ProgressiveLevelProvider : IProgressiveLevelProvider, IService
    {
        private const string All = "ALL";

        private readonly IGameStorage _gameStorage;
        private readonly IIdProvider _idProvider;
        private readonly IProgressiveMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly ISharedSapProvider _sharedSapProvider;

        private readonly List<ProgressiveLevel> _levels = new List<ProgressiveLevel>();
        private readonly object _sync = new object();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ProgressiveLevelProvider(
            IGameStorage gameStorage,
            IIdProvider idProvider,
            IProgressiveMeterManager meters,
            IPropertiesManager properties,
            ISharedSapProvider sharedSapProvider)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _sharedSapProvider = sharedSapProvider ?? throw new ArgumentNullException(nameof(sharedSapProvider));
        }

        public void LoadProgressiveLevels(IGameDetail gameDetails, IEnumerable<ProgressiveDetail> progressiveDetails)
        {
            if (gameDetails == null)
            {
                throw new ArgumentNullException(nameof(gameDetails));
            }

            if (progressiveDetails == null)
            {
                throw new ArgumentNullException(nameof(progressiveDetails));
            }

            lock (_sync)
            {
                _sharedSapProvider.AutoGenerateAssociatedLevels(gameDetails, progressiveDetails);

                foreach (var detail in progressiveDetails)
                {
                    var denominations = ToDenoms(gameDetails, detail.Denomination.ToList()).ToList();

                    _levels.AddRange(
                        ToProgressiveLevels(
                            gameDetails.Id,
                            denominations,
                            gameDetails.BetOptionList,
                            detail,
                            gameDetails.WagerCategories.ToList()));
                }
            }
        }

        public IReadOnlyCollection<ProgressiveLevel> GetProgressiveLevels()
        {
            lock (_sync)
            {
                return _levels.ToList();
            }
        }

        public IReadOnlyCollection<ProgressiveLevel> GetProgressiveLevels(string packName, int gameId, long denomination, long wagerCredits = 0)
        {
            lock (_sync)
            {
                return _levels.Where(l => Filter(l, packName, gameId, denomination, wagerCredits)).ToList();
            }
        }

        public void UpdateProgressiveLevels(
            IEnumerable<(string packName, int gameId, long denomination, IEnumerable<ProgressiveLevel> levels)> levelUpdates)
        {
            lock (_sync)
            {
                var newLevels = levelUpdates
                    .SelectMany(x => SetupNewLevels(x.levels, x.gameId, x.denomination, x.packName))
                    .ToList();

                if (newLevels.Any())
                {
                    // Allocate meters for all new levels
                    _meters.AddProgressives(newLevels);
                }
            }
        }

        public void UpdateProgressiveLevels(
            string packName,
            int gameId,
            long denomination,
            IEnumerable<ProgressiveLevel> levels)
        {
            lock (_sync)
            {
                var newLevels = SetupNewLevels(levels, gameId, denomination, packName).ToList();
                if (newLevels.Any())
                {
                    // Allocate meters for all new levels
                    _meters.AddProgressives(newLevels);
                }
            }
        }

        public string Name => nameof(ProgressiveLevelProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveLevelProvider) };

        public void Initialize()
        {
            _meters.UpdateLPCompositeMeters();
        }

        private static bool Filter(IViewableProgressiveLevel level, string packName, int gameId, long denomination, long wagerCredits)
        {
            return level.GameId == gameId && level.Denomination.Contains(denomination) &&
                   level.ProgressivePackName.Equals(packName, StringComparison.InvariantCultureIgnoreCase) &&
                (wagerCredits == 0 || level.WagerCredits == wagerCredits);
        }

        private static IEnumerable<long> ToDenoms(IGameDetail game, IReadOnlyCollection<string> progressiveDenoms)
        {
            if (progressiveDenoms.Contains(All))
            {
                foreach (var denom in game.SupportedDenominations)
                {
                    yield return denom;
                }
            }
            else
            {
                foreach (var denom in progressiveDenoms)
                {
                    yield return Convert.ToInt64(denom).CentsToMillicents();
                }
            }
        }

        private ProgressiveLevel ToProgressiveLevel(
            int gameId,
            IEnumerable<long> denominations,
            BetOption betOption,
            ProgressiveDetail progressive,
            LevelDetail level,
            IViewableProgressiveLevel current,
            long wagerCredits)
        {
            var denominationList = denominations.ToList();
            var resetValueInMillicents = level.ResetValue(denominationList, betOption).CentsToMillicents();
            var levelType = (ProgressiveLevelType)level.ProgressiveType;

            return new ProgressiveLevel
            {
                ProgressiveId = progressive.Id,
                ProgressivePackName = progressive.Name,
                ProgressivePackId = progressive.PackId,
                GameId = gameId,
                Denomination = denominationList,
                BetOption = betOption?.Name,
                AllowTruncation = level.AllowTruncation,
                LevelId = level.LevelId,
                IncrementRate = level.IncrementRate.ToPercentage(),
                HiddenIncrementRate = level.HiddenIncrementRate.ToPercentage(),
                HiddenValue = current?.HiddenValue ?? 0,
                LevelName = level.Name,
                Probability = level.Probability.ToPercentage(),
                MaximumValue = level.MaximumValue(denominationList).CentsToMillicents(),
                ResetValue = resetValueInMillicents,
                Variation = progressive.Variation,
                ProgressivePackRtp =
                    new ProgressiveRtp
                    {
                        Reset =
                            new RtpRange(
                                progressive.ReturnToPlayer.ResetRtpMin,
                                progressive.ReturnToPlayer.ResetRtpMax),
                        Increment =
                            new RtpRange(
                                progressive.ReturnToPlayer.IncrementRtpMin,
                                progressive.ReturnToPlayer.IncrementRtpMax),
                        BaseAndReset =
                            new RtpRange(
                                progressive.ReturnToPlayer.BaseRtpAndResetRtpMin ?? 0M,
                                progressive.ReturnToPlayer.BaseRtpAndResetRtpMax ?? 0M),
                        BaseAndResetAndIncrement = new RtpRange(
                            progressive.ReturnToPlayer.BaseRtpAndResetRtpAndIncRtpMin ?? 0M,
                            progressive.ReturnToPlayer.BaseRtpAndResetRtpAndIncRtpMax ?? 0M)
                    },
                LevelRtp = level.Rtp.ToPercentage(),
                LevelType = levelType,
                LineGroup = level.LineGroup,
                TriggerControl = (TriggerType)level.Trigger,
                FundingType = (SapFundingType)level.SapFundingType,
                Errors = ProgressiveErrors.None,
                AssignedProgressiveId = GenerateAssignableId(current, progressive, denominationList.First(), level.Name),
                Turnover = 0,
                CurrentState = current == null ? ProgressiveLevelState.Init : ProgressiveLevelState.Ready,
                CurrentValue = current?.CurrentValue ?? resetValueInMillicents,
                InitialValue = current?.InitialValue ?? resetValueInMillicents,
                DeviceId = current?.DeviceId ?? 0,
                Overflow = current?.Overflow ?? 0,
                OverflowTotal = current?.OverflowTotal ?? 0,
                Residual = current?.Residual ?? 0,
                CanEdit = current?.CanEdit ?? true,
                CreationType = (LevelCreationType)progressive.CreationType,
                WagerCredits = wagerCredits
            };
        }

        private IEnumerable<ProgressiveLevel> GenerateProgressiveLevelsPerGame(
            int gameId,
            IReadOnlyCollection<long> denominations,
            ProgressiveDetail progressive,
            LevelDetail levelDetail,
            IReadOnlyCollection<ProgressiveLevel> persistedLevels)
        {
            var currentLevel = persistedLevels.FirstOrDefault(
                l => l.LevelId == levelDetail.LevelId);

            yield return ToProgressiveLevel(gameId, denominations, null, progressive, levelDetail, currentLevel, 0);
        }

        private IEnumerable<ProgressiveLevel> GenerateProgressiveLevelsPerGamePerDenomPerBetOption(
            int gameId,
            IReadOnlyCollection<long> denominations,
            BetOptionList betOptions,
            ProgressiveDetail progressive,
            LevelDetail levelDetail,
            IReadOnlyCollection<ProgressiveLevel> persistedLevels)
        {
            foreach (var denomination in denominations)
            {
                var singleDenomList = new List<long> { denomination };
                foreach (var betOption in betOptions)
                {
                    var currentLevel = persistedLevels.FirstOrDefault(
                        l => l.LevelId == levelDetail.LevelId &&
                             l.Denomination.Count() == 1 &&
                             l.Denomination.First() == denomination &&
                             l.BetOption.Equals(betOption.Name, StringComparison.InvariantCulture));

                    yield return ToProgressiveLevel(
                        gameId,
                        singleDenomList,
                        betOption,
                        progressive,
                        levelDetail,
                        currentLevel,
                        0);
                }
            }
        }

        private AssignableProgressiveId GenerateAssignableId(
            IViewableProgressiveLevel level,
            ProgressiveDetail detail,
            long denom,
            string name)
        {
            if (!detail.UseLevels.All(l => l.Equals(All)))
            {
                var levelName = SharedSapProviderExtensions.GeneratedLevelName(
                    detail.Name,
                    detail.PackId,
                    name,
                    denom,
                    null,
                    detail.UseLevels,
                    detail.LevelPack);

                var sharedLevel = _sharedSapProvider.ViewSharedSapLevels()
                    .FirstOrDefault(s => s.Name.Equals(levelName));

                if (sharedLevel != null)
                {
                    return new AssignableProgressiveId(
                        AssignableProgressiveType.AssociativeSap,
                        sharedLevel.Id.ToString("B"));
                }
            }

            return new AssignableProgressiveId(
                level?.AssignedProgressiveId.AssignedProgressiveType ?? AssignableProgressiveType.None,
                level?.AssignedProgressiveId.AssignedProgressiveKey ?? string.Empty);
        }

        private IEnumerable<ProgressiveLevel> SetupNewLevels(
            IEnumerable<ProgressiveLevel> levels,
            int gameId,
            long denomination,
            string packName)
        {
            var progressiveLevels = levels.ToList();
            foreach (var level in progressiveLevels.Where(level => level.DeviceId <= 0))
            {
                level.DeviceId = _idProvider.GetNextDeviceId<IViewableProgressiveLevel>();
                yield return level;
            }

            var mappingDenom = GetLowestSharedDenom(levels) ?? denomination;

            if (_gameStorage.TryGetValues(
                gameId,
                mappingDenom,
                packName,
                out IEnumerable<ProgressiveLevel> persistedValues))
            {
                foreach (var level in persistedValues)
                {
                    if (!progressiveLevels.Any(l => l.LevelId == level.LevelId))
                    {
                        progressiveLevels.Add(level);
                    }
                }
            }

            // store levels based on game(variation) and denom
            _gameStorage.SetValue(gameId, mappingDenom, packName, progressiveLevels);
        }

        private IEnumerable<ProgressiveLevel> ToProgressiveLevels(
            int gameId,
            IReadOnlyCollection<long> denominations,
            BetOptionList betOptions,
            ProgressiveDetail progressive,
            IList<IWagerCategory> wagerCategories)
        {
            var currentValues = GetPersistedLevels(gameId, denominations, progressive.Name);

            Logger.Debug(
                $"Got {currentValues.Count} levels for gameId {gameId}, denomination {denominations.First()}, packName {progressive.Name}");

            var progressiveLevels = new List<ProgressiveLevel>();

            var poolCreationType = (ProgressivePoolCreation)_properties.GetProperty(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            foreach (var level in progressive.Levels)
            {
                if (poolCreationType == ProgressivePoolCreation.WagerBased &&
                    progressive.CreationType != PackageManifest.Ati.poolCreationType.Default)
                {
                    progressiveLevels.AddRange(
                        GenerateLevelsPerGamePerDenomPerWager(
                            gameId,
                            denominations,
                            progressive,
                            level,
                            currentValues,
                            wagerCategories));
                }
                else if (level.StartupValue.IsCredit)
                {
                    progressiveLevels.AddRange(
                        GenerateProgressiveLevelsPerGamePerDenomPerBetOption(
                            gameId,
                            denominations,
                            betOptions,
                            progressive,
                            level,
                            currentValues));
                }
                else
                {
                    progressiveLevels.AddRange(
                        GenerateProgressiveLevelsPerGame(
                            gameId,
                            denominations,
                            progressive,
                            level,
                            currentValues));
                }
            }

            return progressiveLevels;
        }

        private IReadOnlyCollection<ProgressiveLevel> GetPersistedLevels(
            int gameId,
            IReadOnlyCollection<long> denominations,
            string packName)
        {
            var currentValues = new List<ProgressiveLevel>();

            foreach (var denom in denominations)
            {
                if (_gameStorage.TryGetValues(
                    gameId,
                    denom,
                    packName,
                    out IEnumerable<ProgressiveLevel> persistedValues))
                {
                    currentValues.AddRange(persistedValues);
                }
            }

            return currentValues;
        }

        private IEnumerable<ProgressiveLevel> GenerateLevelsPerGamePerDenomPerWager(
            int gameId,
            IReadOnlyCollection<long> denominations,
            ProgressiveDetail progressive,
            LevelDetail levelDetail,
            IReadOnlyCollection<ProgressiveLevel> persistedLevels,
            IList<IWagerCategory> wagerCategories)
        {
            var poolCreationType = (ProgressivePoolCreation)_properties.GetProperty(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            foreach (var denomination in denominations)
            {
                var singleDenomList = new List<long> { denomination };

                if (poolCreationType == ProgressivePoolCreation.WagerBased &&
                    progressive.CreationType == PackageManifest.Ati.poolCreationType.All)
                {
                    foreach (var wagerCategory in wagerCategories.DistinctBy(x => x.Id))
                    {
                        if (wagerCategory.MaxWagerCredits == null)
                        {
                            continue;
                        }

                        var currentLevel = persistedLevels.FirstOrDefault(
                            l => l.LevelId == levelDetail.LevelId &&
                                 l.Denomination.Count() == 1 &&
                                 l.Denomination.First() == denomination &&
                                 l.WagerCredits == wagerCategory.MaxWagerCredits.Value);

                        yield return ToProgressiveLevel(
                            gameId,
                            singleDenomList,
                            null,
                            progressive,
                            levelDetail,
                            currentLevel,
                            wagerCategory.MaxWagerCredits.Value);
                    }
                }
                else if (poolCreationType == ProgressivePoolCreation.WagerBased &&
                         progressive.CreationType == PackageManifest.Ati.poolCreationType.Max)
                {
                    var filteredWagerCategories = wagerCategories.Where(x => x.MaxWagerCredits != null);

                    var max = filteredWagerCategories
                        .Where(item => item.MaxWagerCredits != null)
                        .OrderByDescending(item => item.MaxWagerCredits.Value)
                        .FirstOrDefault();

                    if (max?.MaxWagerCredits == null)
                    {
                        continue;
                    }

                    var currentLevel = persistedLevels.FirstOrDefault(
                        l => l.LevelId == levelDetail.LevelId &&
                             l.Denomination.Count() == 1 &&
                             l.Denomination.First() == denomination &&
                             l.WagerCredits == max.MaxWagerCredits.Value);

                    yield return ToProgressiveLevel(
                        gameId,
                        singleDenomList,
                        null,
                        progressive,
                        levelDetail,
                        currentLevel,
                        max.MaxWagerCredits.Value);
                }
            }
        }

        private long? GetLowestSharedDenom(IEnumerable<ProgressiveLevel> levels)
        {
            var lowestSharedDenom = levels
                .SelectMany(l1 => l1.Denomination)
                .Distinct()
                .OrderBy(d1 => d1)
                .FirstOrDefault(d2 => levels.All(l2 => l2.Denomination.Contains(d2)));

            return lowestSharedDenom == default(long) ? null : lowestSharedDenom;
        }
    }
}