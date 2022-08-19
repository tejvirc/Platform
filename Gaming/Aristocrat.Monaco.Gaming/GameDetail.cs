namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using PackageManifest.Models;

    internal class GameDetail : IGameDetail
    {
        private GameStatus _status;

        public bool New { get; set; }

        public IEnumerable<UpgradeAction> UpgradeActions { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool Active { get; set; }

        public int Id { get; set; }

        public string ThemeName { get; set; }

        public DateTime InstallDate { get; set; }

        public string GameDll { get; set; }

        public GameIconType GameIconType { get; set; }

        public Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; set; }

        public string DisplayMeterName { get; set; }

        public IEnumerable<string> AssociatedSapDisplayMeterName { get; set; }

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

        public long? ProductCode { get; set; }

        public string CdsThemeId { get; set; }

        public string Folder { get; set; }

        public IEnumerable<long> ActiveDenominations => Denominations.Where(d => d.Active).Select(d => d.Value);

        public IEnumerable<long> SupportedDenominations => Denominations.Select(d => d.Value);

        public IEnumerable<IDenomination> Denominations { get; set; }

        public IEnumerable<IWagerCategory> WagerCategories { get; set; }

        public IEnumerable<IWinLevel> WinLevels { get; set; }

        public string VariationId { get; set; }

        public bool Enabled => Active && Denominations.Any(d => d.Active) && Status == GameStatus.None;

        public bool EgmEnabled => Active && Denominations.Any(d => d.Active) &&
                                  (Status & GameStatus.DisabledBySystem) != GameStatus.DisabledBySystem;

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

        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public bool Upgraded { get; set; }

        public bool AutoPlaySupported => GameType == GameType.Slot || GameType == GameType.Keno;

        public int? MaximumProgressivePerDenom { get; set; }

        public string ReferenceId { get; set; }

        public int MechanicalReels { get; set; }

        public int[] MechanicalReelHomeSteps { get; set; }

        public GameCategory Category { get; set; }

        public GameSubCategory SubCategory { get; set; }

        public IEnumerable<Feature> Features { get; set; }
    }
}