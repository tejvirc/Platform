namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Metering;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the GameMeterProvider class.
    /// </summary>
    public class GameMeterProvider : BaseMeterProvider, IDisposable
    {
        private const PersistenceLevel ProviderPersistenceLevel = PersistenceLevel.Critical;

        private const string GameMeterProviderExtensionPoint = "/Gaming/Metering/GameMeterProvider";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();
        private readonly IGameMeterManager _meterManager;

        private readonly char[] _operators = { '+', '-', '*', '/', '(', ')' };

        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;
        private readonly bool _rolloverTest;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterProvider" /> class.
        /// </summary>
        /// <param name="persistentStorage">The persistent storage manager</param>
        /// <param name="properties">The properties manager</param>
        /// <param name="meterManager">The meter manager</param>
        public GameMeterProvider(
            IPersistentStorageManager persistentStorage,
            IPropertiesManager properties,
            IGameMeterManager meterManager)
            : base(typeof(GameMeterProvider).ToString())
        {
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));

            _rolloverTest = _properties.GetValue(@"maxmeters", "false") == "true";

            Initialize();

            _meterManager.GameAdded += OnGameAdded;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _meterManager.GameAdded -= OnGameAdded;
            }

            _disposed = true;
        }

        private void OnGameAdded(object sender, GameAddedEventArgs gameAddedEventArgs)
        {
            Initialize();

            // Refresh the published meters
            Invalidate();
        }

        private static IReadOnlyCollection<string> GameMeterList(IEnumerable<string> symbols)
        {
            return symbols.Select(symbol => symbol.Trim())
                .Where(trimmed => !string.IsNullOrEmpty(trimmed) && !IsNumeric(trimmed)).ToList();
        }

        private static bool IsNumeric(string input)
        {
            return long.TryParse(input, out _);
        }

        private Func<MeterTimeframe, long> BuildGameMeterExpression(IReadOnlyCollection<string> meters)
        {
            return timeFrame =>
            {
                return meters.Sum(meter => GetMeter(meter).GetValue(timeFrame));
            };
        }

        private Func<MeterTimeframe, long> BuildPercentMeterExpression(IReadOnlyCollection<string> meters)
        {
            return timeFrame =>
            {
                return (long)Math.Round(meters.Sum(meter => GetMeter(meter).GetValue(timeFrame) / PercentageMeterClassification.Multiplier) / meters.Count);
            };
        }

        private void Initialize()
        {
            var atomicMeters = MonoAddinsHelper.GetSelectedNodes<GameAtomicMeterNode>(GameMeterProviderExtensionPoint);

            lock (_lock)
            {
                // Allocate storage for the atomic meters
                Allocate(atomicMeters);

                // Add the "dynamic" gaming meters
                AddAtomicMeters(atomicMeters);

                AddCompositeMeters(MonoAddinsHelper.GetSelectedNodes<GameCompositeMeterNode>(GameMeterProviderExtensionPoint).ToList());
            }

            Logger.Debug("Initialized all game meters");
        }

        private void Allocate(IEnumerable<GameAtomicMeterNode> meters)
        {
            var blockName = GetType().ToString();

            foreach (var meter in meters)
            {
                var meterBlockName = blockName + "." + meter.Name;
                var blockSize = 0;

                if (meter.Group.Equals("performance", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.GameCount;
                }
                else if (meter.Group.Equals("denomination", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.DenominationCount;
                }
                else if (meter.Group.Equals("wagerCategory", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.WagerCategoryCount;
                }

                if (_persistentStorage.BlockExists(meterBlockName))
                {
                    var block = _persistentStorage.GetBlock(meterBlockName);
                    if (block.Count < blockSize)
                    {
                        _persistentStorage.ResizeBlock(meterBlockName, blockSize);
                    }
                }
                else
                {
                    var blockFormat = new BlockFormat();

                    blockFormat.AddFieldDescription(new FieldDescription(FieldType.Int64, 0, "Lifetime"));
                    blockFormat.AddFieldDescription(new FieldDescription(FieldType.Int64, 0, "Period"));

                    _persistentStorage.CreateDynamicBlock(
                        ProviderPersistenceLevel,
                        meterBlockName,
                        blockSize == 0 ? 1 : blockSize,
                        blockFormat);
                }
            }
        }

        private void AddAtomicMeters(IEnumerable<GameAtomicMeterNode> meters)
        {
            var blockName = GetType().ToString();

            var games = _properties.GetValues<IGameDetail>(GamingConstants.AllGames).OrderBy(g => g.Id).ToList();

            foreach (var meter in meters)
            {
                var meterBlockName = blockName + "." + meter.Name;

                var block = _persistentStorage.GetBlock(meterBlockName);

                if (meter.Group.Equals("performance", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicPerformanceMeter(meter, block, games);
                }
                else if (meter.Group.Equals("denomination", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicDenominationMeter(meter, block, games);
                }
                else if (meter.Group.Equals("wagerCategory", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicWagerCategoryMeter(meter, block, games);
                }
            }
        }

        private void AddAtomicPerformanceMeter(
            GameAtomicMeterNode meter,
            IPersistentStorageAccessor block,
            IEnumerable<IGame> games)
        {
            var currentValues = block.GetAll();

            var gameMeters = new List<string>();
            foreach (var game in games)
            {
                var meterName = _meterManager.GetMeterName(game.Id, meter.Name);

                gameMeters.Add(meterName);

                if (Contains(meterName))
                {
                    continue;
                }

                var blockIndex = _meterManager.GetBlockIndex(game.Id);

                var lifetime = 0L;
                var period = 0L;

                if (currentValues.TryGetValue(blockIndex, out var current))
                {
                    lifetime = (long)current["Lifetime"];
                    period = (long)current["Period"];
                }

                var atomicMeter = new AtomicMeter(meterName, block, blockIndex, meter.Classification, this, lifetime, period);

                SetupMeterRolloverTest(atomicMeter);
                AddMeter(atomicMeter);
            }

            CreateSummaryMeter(meter.Name, meter.Classification, gameMeters);
        }

        private void AddAtomicDenominationMeter(
            GameAtomicMeterNode meter,
            IPersistentStorageAccessor block,
            IEnumerable<IGameDetail> games)
        {
            var currentValues = block.GetAll();

            var gameMeters = new List<string>();
            var denomMeters = new Dictionary<long, List<string>>();
            foreach (var game in games)
            {
                var betLevelMeters = new List<string>();

                // For each bet level within a game, create an atomic meter, add it to collection in base class,
                // and add its name to a collection of meter names that will make up a composite meter
                foreach (var denomination in game.SupportedDenominations.OrderBy(d => d))
                {
                    if (!denomMeters.ContainsKey(denomination))
                    {
                        denomMeters[denomination] = new List<string>();
                    }

                    var meterName = _meterManager.GetMeterName(game.Id, denomination, meter.Name);

                    denomMeters[denomination].Add(meterName);
                    betLevelMeters.Add(meterName);

                    if (Contains(meterName))
                    {
                        continue;
                    }

                    var index = _meterManager.GetBlockIndex(game.Id, denomination);

                    var lifetime = 0L;
                    var period = 0L;

                    if (currentValues.TryGetValue(index, out var current))
                    {
                        lifetime = (long)current["Lifetime"];
                        period = (long)current["Period"];
                    }

                    var atomicMeter = new AtomicMeter(meterName, block, index, meter.Classification, this, lifetime, period);

                    SetupMeterRolloverTest(atomicMeter);
                    AddMeter(atomicMeter);
                }

                var compositeMeterName = _meterManager.GetMeterName(game.Id, meter.Name);

                CreateSummaryMeter(compositeMeterName, meter.Classification, betLevelMeters);
                gameMeters.Add(compositeMeterName);
            }

            foreach (var denomMeter in denomMeters)
            {
                var compositeMeterName = _meterManager.GetMeterName(denomMeter.Key, meter.Name);
                CreateSummaryMeter(compositeMeterName, meter.Classification, denomMeter.Value);
            }

            CreateSummaryMeter(meter.Name, meter.Classification, gameMeters);
        }

        private void AddAtomicWagerCategoryMeter(
            GameAtomicMeterNode meter,
            IPersistentStorageAccessor block,
            IEnumerable<IGameDetail> games)
        {
            var currentValues = block.GetAll();

            foreach (var game in games)
            {
                var wagerMeters = new Dictionary<string, List<string>>();
                foreach (var denomination in game.SupportedDenominations.OrderBy(d => d))
                {
                    foreach (var wagerCategory in game.WagerCategories.OrderBy(d => d.Id))
                    {
                        if (!wagerMeters.ContainsKey(wagerCategory.Id))
                        {
                            wagerMeters[wagerCategory.Id] = new List<string>();
                        }

                        var meterName = _meterManager.GetMeterName(game.Id, denomination, wagerCategory.Id, meter.Name);
                        if (Contains(meterName))
                        {
                            continue;
                        }

                        var lifetime = 0L;
                        var period = 0L;

                        wagerMeters[wagerCategory.Id].Add(meterName);
                        var index = _meterManager.GetBlockIndex(game.Id, denomination, wagerCategory.Id);

                        if (currentValues.TryGetValue(index, out var current))
                        {
                            lifetime = (long)current["Lifetime"];
                            period = (long)current["Period"];
                        }

                        var atomicMeter = new AtomicMeter(
                            meterName,
                            block,
                            index,
                            meter.Classification,
                            this,
                            lifetime,
                            period);

                        SetupMeterRolloverTest(atomicMeter);
                        AddMeter(atomicMeter);
                    }
                }

                foreach (var wagerMeter in wagerMeters)
                {
                    var compositeMeterName = _meterManager.GetMeterName(game.Id, wagerMeter.Key, meter.Name);
                    CreateSummaryMeter(compositeMeterName, meter.Classification, wagerMeter.Value);
                }
            }
        }

        private void AddCompositeMeters(IReadOnlyCollection<GameCompositeMeterNode> compositeMeterNodes)
        {
            var games = _properties.GetValues<IGameDetail>(GamingConstants.AllGames).OrderBy(g => g.Id).ToList();
            AddTheoreticalPaybackMeter(games);

            // This will temporarily allow us to avoid using the Flee library for calculating simple sums of meters (the majority)
            //  This needs to be re-factored out
            string[] operators = { "-", "*", "/" };

            foreach (var meter in compositeMeterNodes.Where(m => !operators.Any(o => m.Expression.Contains(o))))
            {
                if (meter.Group.Equals("denomination", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddCompositeDenominationMeter(meter, games);
                }
                else if (meter.Group.Equals("performance", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddCompositePerformanceMeter(meter, games);
                }
            }

            foreach (var meter in compositeMeterNodes.Where(m => operators.Any(o => m.Expression.Contains(o))))
            {
                if (meter.Group.Equals("denomination", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddComplexCompositeDenominationMeter(meter, games);
                }
                else if (meter.Group.Equals("performance", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddComplexCompositePerformanceMeter(meter, games);
                }
            }
        }

        private void AddCompositePerformanceMeter(GameCompositeMeterNode meter, IReadOnlyCollection<IGame> games)
        {
            var meters = GameMeterList(meter.Expression.Split(_operators));

            foreach (var game in games)
            {
                CreateSummaryMeter(
                    _meterManager.GetMeterName(game.Id, meter.Name),
                    meter.Classification,
                    meters.Select(symbol => _meterManager.GetMeterName(game.Id, symbol)).ToList());
            }

            // Creates the summary composite meter
            CreateSummaryMeter(
                meter.Name,
                meter.Classification,
                meters.Select(symbol => symbol).ToList());
        }

        private void AddCompositeDenominationMeter(GameCompositeMeterNode meter, IReadOnlyCollection<IGameDetail> games)
        {
            var meters = GameMeterList(meter.Expression.Split(_operators));

            var denomMeters = new Dictionary<long, List<IMeter>>();
            foreach (var game in games)
            {
                foreach (var denomination in game.SupportedDenominations)
                {
                    if (!denomMeters.ContainsKey(denomination))
                    {
                        denomMeters[denomination] = new List<IMeter>();
                    }

                    var gameDenomMeterName = _meterManager.GetMeterName(game.Id, denomination, meter.Name);

                    var denomMeter = CreateSummaryMeter(
                        gameDenomMeterName,
                        meter.Classification,
                        meters.Select(symbol => _meterManager.GetMeterName(game.Id, denomination, symbol)).ToList());

                    denomMeters[denomination].Add(denomMeter);
                }

                // Create a composite game summary meter
                CreateSummaryMeter(
                    _meterManager.GetMeterName(game.Id, meter.Name),
                    meter.Classification,
                    meters.Select(symbol => _meterManager.GetMeterName(game.Id, symbol)).ToList());
            }

            // Create a composite denomination summary meter
            foreach (var denomMeter in denomMeters)
            {
                CreateSummaryMeter(
                    _meterManager.GetMeterName(denomMeter.Key, meter.Name),
                    meter.Classification,
                    meters.Select(symbol => _meterManager.GetMeterName(denomMeter.Key, symbol)).ToList());
            }

            CreateSummaryMeter(
                meter.Name,
                meter.Classification,
                meters.Select(symbol => symbol).ToList());
        }

        private IMeter CreateSummaryMeter(string name, string classification, IReadOnlyCollection<string> meters)
        {
            return AddOrReplaceMeter(
                classification == "Percentage"
                    ? new CompositeMeter(name, BuildPercentMeterExpression(meters), meters, new PercentageMeterClassification())
                    : new CompositeMeter(name, BuildGameMeterExpression(meters), meters, classification));
        }

        private void SetupMeterRolloverTest(IMeter meter)
        {
            if (!_rolloverTest)
            {
                return;
            }

            if (meter == null || meter.Lifetime != 0)
            {
                return;
            }

            var preRollover = meter.Classification.UpperBounds;
            if (meter.Classification.GetType() == typeof(CurrencyMeterClassification))
            {
                var currencyMultiplier = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);
                var oneCent = currencyMultiplier / (int)CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit;
                preRollover -= (long)oneCent;
            }
            else
            {
                preRollover -= 1;
            }

            Logger.Debug($"Incrementing meter: {meter.Name} to {preRollover}");
            meter.Increment(preRollover);
        }

        private static IEnumerable<string> FormatGameMeterList(IEnumerable<string> symbols)
        {
            return symbols.Select(symbol => symbol.Trim())
                .Where(trimmed => !string.IsNullOrEmpty(trimmed) && !IsNumeric(trimmed)).ToList();
        }

        private void AddComplexCompositePerformanceMeter(GameCompositeMeterNode meter, IEnumerable<IGame> games)
        {
            foreach (var game in games)
            {
                var meterName = _meterManager.GetMeterName(game.Id, meter.Name);

                var expression = FormatConfiguredExpression(
                    new StringBuilder(meter.Expression),
                    meter.Expression.Split(_operators),
                    game.Id);

                AddCompositeMeter(meterName, expression, meter.Classification);
            }

            // Creates the summary composite meter
            AddCompositeMeter(meter.Name, meter.Expression, meter.Classification);
        }

        private void AddComplexCompositeDenominationMeter(GameCompositeMeterNode meter, IEnumerable<IGameDetail> games)
        {
            var denomMeters = new Dictionary<long, List<string>>();
            foreach (var game in games)
            {
                foreach (var denomination in game.SupportedDenominations)
                {
                    if (!denomMeters.ContainsKey(denomination))
                    {
                        denomMeters[denomination] = new List<string>();
                    }

                    var meterName = _meterManager.GetMeterName(game.Id, denomination, meter.Name);
                    denomMeters[denomination].Add(meterName);

                    var expression = FormatConfiguredExpression(
                        new StringBuilder(meter.Expression),
                        FormatGameMeterList(meter.Expression.Split(_operators)),
                        game.Id,
                        denomination);

                    AddCompositeMeter(meterName, expression, meter.Classification);
                }

                // Create a composite game summary meter
                var compositeGameMeterName = _meterManager.GetMeterName(game.Id, meter.Name);
                var compositeGameExpression = FormatConfiguredExpression(
                    new StringBuilder(meter.Expression),
                    meter.Expression.Split(_operators),
                    game.Id);

                AddCompositeMeter(compositeGameMeterName, compositeGameExpression, meter.Classification);
            }

            // Create a composite denomination summary meter
            foreach (var denomMeter in denomMeters)
            {
                var meterName = _meterManager.GetMeterName(denomMeter.Key, meter.Name);
                var compositeDenomExpression = FormatConfiguredExpression(
                    new StringBuilder(meter.Expression),
                    meter.Expression.Split(_operators),
                    denomMeter.Key);
                AddCompositeMeter(meterName, compositeDenomExpression, meter.Classification);
            }

            // Creates the summary composite meter
            AddCompositeMeter(meter.Name, meter.Expression, meter.Classification);
        }

        private void AddTheoreticalPaybackMeter(IEnumerable<IGameDetail> games)
        {
            var denomMeters = new Dictionary<long, List<(int, long, string, decimal)>>();
            foreach (var game in games)
            {
                foreach (var denomination in game.SupportedDenominations.OrderBy(d => d))
                {
                    if (!denomMeters.ContainsKey(denomination))
                    {
                        denomMeters.Add(
                            denomination,
                            new List<(int gameId, long denom, string wagerCategory, decimal theoPaybackPercent)>());
                    }

                    var meterName = _meterManager.GetMeterName(game.Id, denomination, GamingMeters.TheoPayback);

                    var gameInfo = game.WagerCategories.Select(x => (game.Id, denomination, x.Id, x.TheoPaybackPercent))
                        .ToList();
                    CreateTheoreticalPaybackMeter(meterName, gameInfo);
                    denomMeters[denomination].AddRange(gameInfo);
                }

                var compositeMeterName = _meterManager.GetMeterName(game.Id, GamingMeters.TheoPayback);
                CreateTheoreticalPaybackMeter(
                    compositeMeterName,
                    game.SupportedDenominations.SelectMany(
                        d => game.WagerCategories.Select(w => (game.Id, d, w.Id, w.TheoPaybackPercent))).ToList());
            }

            foreach (var denomMeter in denomMeters)
            {
                var compositeMeterName = _meterManager.GetMeterName(denomMeter.Key, GamingMeters.TheoPayback);
                CreateTheoreticalPaybackMeter(compositeMeterName, denomMeter.Value);
            }

            CreateTheoreticalPaybackMeter(
                GamingMeters.TheoPayback,
                denomMeters.Values.SelectMany((v, k) => v).ToList());
        }

        private void CreateTheoreticalPaybackMeter(
            string meterName,
            IReadOnlyList<(int gameId, long denom, string wagerCategory, decimal theoPaybackPercent)> gameInfo)
        {
            AddOrReplaceMeter(
                new CompositeMeter(
                    meterName,
                    (timeFrame) => (long)Math.Round(
                        gameInfo.Sum(
                            x =>
                                _meterManager.GetMeter(
                                    x.gameId,
                                    x.denom,
                                    x.wagerCategory,
                                    GamingMeters.WagerCategoryWageredAmount).GetValue(timeFrame) *
                                x.theoPaybackPercent.ToDecimal())),
                    gameInfo.Select(
                        x => _meterManager.GetMeterName(
                            x.gameId,
                            x.denom,
                            x.wagerCategory,
                            GamingMeters.WagerCategoryWageredAmount)).ToList(),
                    new CurrencyMeterClassification()));
        }

        private void AddCompositeMeter(string name, string expression, string classification)
        {
            var includedMeters = FormatGameMeterList(expression.Split(_operators)).ToList();

            if (includedMeters.Any(m => !Contains(m)))
            {
                Logger.Debug($"Composite Meter for meter {name} with expression {expression} not created, since not all meters in expression are provided.");
                return;
            }

            var compositeMeter = new CompositeMeter(
                name,
                expression,
                includedMeters,
                classification);
            AddOrReplaceMeter(compositeMeter);
        }

        private string FormatConfiguredExpression(
                    StringBuilder expressionBuilder,
                    IEnumerable<string> symbols,
                    int gameId)
        {
            var expression = expressionBuilder.ToString();

            foreach (var symbol in symbols)
            {
                var originalSymbol = symbol.Trim();

                if (string.IsNullOrEmpty(originalSymbol) || IsNumeric(originalSymbol))
                {
                    continue;
                }

                var regex = new Regex(@"\b" + originalSymbol + @"\b");
                expression = regex.Replace(expression, _meterManager.GetMeterName(gameId, originalSymbol), 1);
            }

            return expression;
        }

        private string FormatConfiguredExpression(
            StringBuilder expressionBuilder,
            IEnumerable<string> symbols,
            long denomination)
        {
            var expression = expressionBuilder.ToString();

            foreach (var symbol in symbols)
            {
                var originalSymbol = symbol.Trim();

                if (string.IsNullOrEmpty(originalSymbol) || IsNumeric(originalSymbol))
                {
                    continue;
                }

                var regex = new Regex(@"\b" + originalSymbol + @"\b");
                expression = regex.Replace(expression, _meterManager.GetMeterName(denomination, originalSymbol), 1);
            }

            return expression;
        }

        private string FormatConfiguredExpression(
            StringBuilder expressionBuilder,
            IEnumerable<string> symbols,
            int gameId,
            long denomination)
        {
            var expression = expressionBuilder.ToString();

            foreach (var symbol in symbols)
            {
                var originalSymbol = symbol.Trim();

                if (string.IsNullOrEmpty(originalSymbol) || IsNumeric(originalSymbol))
                {
                    continue;
                }

                var regex = new Regex(@"\b" + originalSymbol + @"\b");
                expression = regex.Replace(
                    expression,
                    _meterManager.GetMeterName(gameId, denomination, originalSymbol),
                    1);
            }

            return expression;
        }
    }
}