namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Contracts.EdgeLighting;
    using Gaming.Contracts;
    using Gaming.Contracts.Models;
    using Kernel;
    using PackageManifest.Models;

    public class MockGameInfo : IGameDetail
    {
        public static string[] MockLocalGraphics => new[] { "en-us", "fr-ca", };

        private static readonly List<Tuple<
            int, /*game ID*/
            GameType, /*game type*/
            string, /*theme ID*/
            string, /*theme name*/
            bool, /*game enabled*/
            Dictionary<string, ILocaleGameGraphics> /*game graphics*/
        >> GameDetails =
            new List<Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>>
            {
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    1,
                    GameType.Slot,
                    "ThemeID-1",
                    "ThemeOne",
                    true,
                    CreateLocaleGraphics("ThemeOne")),
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    2,
                    GameType.Blackjack,
                    "ThemeID-2",
                    "ThemeTwo",
                    true,
                    CreateLocaleGraphics("ThemeTwo")),
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    3,
                    GameType.Slot,
                    "ThemeID-3",
                    "ThemeThree",
                    false,
                    CreateLocaleGraphics("ThemeThree")),
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    4,
                    GameType.Poker,
                    "ThemeID-4",
                    "ThemeFour",
                    true,
                    CreateLocaleGraphics("ThemeFour")),
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    5,
                    GameType.Keno,
                    "ThemeID-5",
                    "ThemeFive",
                    true,
                    CreateLocaleGraphics("ThemeFive")),
                new Tuple<int, GameType, string, string, bool, Dictionary<string, ILocaleGameGraphics>>(
                    6,
                    GameType.Roulette,
                    "ThemeID-6",
                    "ThemeSix",
                    true,
                    CreateLocaleGraphics("ThemeSix")),
            };

        private GameStatus _status;

        public bool New { get; set; }

        public IEnumerable<UpgradeAction> UpgradeActions { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool Active { get; set; }

        public int Id { get; set; }

        public string ThemeName { get; set; }

        public DateTime InstallDate { get; set; }

        public string GameDll { get; set; }

        public Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; set; }

        public string DisplayMeterName { get; set; }

        public IEnumerable<string> AssociatedSapDisplayMeterName { get; set; }

        public GameIconType GameIconType { get; set; }

        public long InitialValue { get; set; }

        public string TargetRuntime { get; set; }

        public string ThemeId { get; set; }

        public string PaytableId { get; set; }

        public int MaximumWagerCredits => WagerCategories.Max(c => c.MaxWagerCredits ?? 1);

        public int MinimumWagerCredits => WagerCategories.Min(c => c.MinWagerCredits ?? 1);

        public long MaximumWinAmount => WagerCategories.Max(c => c.MaxWinAmount);

        public bool CentralAllowed { get; set; }

        public decimal MaximumPaybackPercent { get; set; }

        public decimal MinimumPaybackPercent { get; set; }

        public string PaytableName { get; set; }

        public string CdsThemeId { get; set; }

        public long? ProductCode { get; set; }

        public string Folder { get; set; }

        public IEnumerable<long> ActiveDenominations => Denominations.Where(d => d.Active).Select(d => d.Value);

        public IEnumerable<long> SupportedDenominations => Denominations.Select(d => d.Value);

        public IEnumerable<IDenomination> Denominations { get; set; }

        public IEnumerable<IWagerCategory> WagerCategories { get; set; }

        public IEnumerable<IWinLevel> WinLevels { get; set; }

        public string VariationId { get; set; }

        public string ReferenceId { get; set; }

        public bool Enabled => Active;

        public bool EgmEnabled => Active && Denominations.Any(d => d.Active) &&
                                  (Status & GameStatus.DisabledBySystem) != GameStatus.DisabledBySystem;

        public int? MaximumProgressivePerDenom { get; }

        public GameStatus Status
        {
            get => _status;
            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new GameStatusChangedEvent(Id));
            }
        }

        public IEnumerable<string> GameTags { get; set; }

        public GameType GameType { get; set; }

        public string GameSubtype { get; set; }

        public BetOptionList BetOptionList { get; set; }

        public BetOption ActiveBetOption { get; set; }

        public LineOptionList LineOptionList { get; set; }

        public LineOption ActiveLineOption { get; set; }

        public BetLinePresetList BetLinePresetList { get; set; }

        public long WinThreshold { get; set; }

        public bool AutoPlaySupported { get; }

        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public bool Upgraded { get; set; }

        public GameCategory Category { get; set; } = GameCategory.Table;

        public GameSubCategory SubCategory => GameSubCategory.FiveHand;

        public bool NextToMaxBetTopAwardMultiplier { get; set; }

        public static IEnumerable<IGameDetail> GetMockGameDetailInfo()
        {
            var gameDetail = new List<IGameDetail>();


            foreach (var (item1, item2, item3, item4, item5, _) in GameDetails)
            {
                var localeGraphics = new Dictionary<string, ILocaleGameGraphics>();
                var gameGraphics = new MockLocalGameGraphics
                {
                    LocaleCode = "en-us",
                    TopperAttractVideo = "en_Topper_Attract_Video",
                    TopAttractVideo = "en_Top_Attract_Video",
                    BottomAttractVideo = "en_Bottom_Attract_Video",
                };
                localeGraphics.Add(gameGraphics.LocaleCode, gameGraphics);

                gameDetail.Add(
                    new MockGameInfo
                    {
                        Id = item1,
                        GameType = item2,
                        ThemeId = item3,
                        ThemeName = item4,
                        Active = item5,
                        LocaleGraphics = localeGraphics,
                    });
            }

            return gameDetail;
        }

        public static IEnumerable<IAttractInfo> GetMockAttractInfo()
        {
            var attractSeq = new List<IAttractInfo>();
            foreach (var (item1, item2, item3, _, item5, _) in GameDetails)
            {
                attractSeq.Add(
                    new AttractInfo { ThemeId = item3, GameType = item2, IsSelected = item5, SequenceNumber = item1 });
            }

            return attractSeq;
        }

        private static Dictionary<string, ILocaleGameGraphics> CreateLocaleGraphics(string gameTheme = "")
        {
            var locales = MockLocalGraphics;
            var gameGraphics = new Dictionary<string, ILocaleGameGraphics>();

            foreach (var locale in locales)
            {
                var graphics = new MockLocalGameGraphics
                {
                    LocaleCode = locale,
                    TopperAttractVideo = locale + "_Topper_Attract_Video_" + gameTheme,
                    TopAttractVideo = locale + "_Top_Attract_Video_" + gameTheme,
                    BottomAttractVideo = locale + "_Bottom_Attract_Video_" + gameTheme,
                };
                gameGraphics.Add(locale, graphics);
            }

            return gameGraphics;
        }

        public IEnumerable<Feature> Features { get; set; }

        public int MechanicalReels { get; set; }

        public int[] MechanicalReelHomeStops { get; set; }

        public PlatformTarget PlatformTarget { get; set; }

        public int MaximumWagerInsideCredits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int MaximumWagerOutsideCredits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class MockLocalGameGraphics : ILocaleGameGraphics
    {
        public string LocaleCode { get; set; }

        public string LargeIcon { get; set; }

        public string SmallIcon { get; set; }

        public string LargeTopPickIcon { get; set; }

        public string SmallTopPickIcon { get; set; }

        public string DenomButtonIcon { get; set; }

        public string DenomPanel { get; set; }

        public string TopAttractVideo { get; set; }

        public string BottomAttractVideo { get; set; }

        public string LoadingScreen { get; set; }

        public string TopperAttractVideo { get; set; }

        public IReadOnlyCollection<(string Color, string BackgroundFilePath)> BackgroundPreviewImages { get; set; }

        public IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> PlayerInfoDisplayResources { get; set; }
    }
}