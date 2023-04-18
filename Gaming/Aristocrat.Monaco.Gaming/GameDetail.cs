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

        /// <summary>
        ///     Gets or sets whether this is a new game
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        ///     Gets or sets the upgrade actions
        /// </summary>
        public IEnumerable<UpgradeAction> UpgradeActions { get; set; }

        /// <summary>
        ///     Gets or sets whether secondary games are allowed
        /// </summary>
        public bool SecondaryAllowed { get; set; }

        /// <inheritdoc />>
        public bool Active { get; set; }

        /// <inheritdoc />>
        public int Id { get; set; }

        /// <inheritdoc />>
        public string ThemeName { get; set; }

        /// <inheritdoc />>
        public DateTime InstallDate { get; set; }

        /// <inheritdoc />>
        public string GameDll { get; set; }

        /// <inheritdoc />>
        public GameIconType GameIconType { get; set; }

        /// <inheritdoc />>
        public Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; set; }

        /// <inheritdoc />>
        public string DisplayMeterName { get; set; }

        /// <inheritdoc />>
        public IEnumerable<string> AssociatedSapDisplayMeterName { get; set; }

        /// <inheritdoc />>
        public long InitialValue { get; set; }

        /// <inheritdoc />>
        public string TargetRuntime { get; set; }

        /// <inheritdoc />>
        public string ThemeId { get; set; }

        /// <inheritdoc />>
        public string PaytableId { get; set; }

        /// <inheritdoc />>
        public int MaximumWagerCredits => WagerCategories.Max(c => c.MaxWagerCredits ?? 1);

        /// <inheritdoc />>
        public int MinimumWagerCredits => WagerCategories.Min(c => c.MinWagerCredits ?? 1);

        /// <inheritdoc />>
        public long MaximumWinAmount => WagerCategories.Max(c => c.MaxWinAmount);

        /// <inheritdoc />>
        public bool CentralAllowed { get; set; }

        /// <inheritdoc />>
        public IEnumerable<ICdsGameInfo> CdsGameInfos { get; set; }

        /// <inheritdoc />>
        public decimal MaximumPaybackPercent { get; set; }

        /// <inheritdoc />>
        public decimal MinimumPaybackPercent { get; set; }

        /// <inheritdoc />>
        public string PaytableName { get; set; }

        /// <inheritdoc />>
        public long? ProductCode { get; set; }

        /// <inheritdoc />>
        public string CdsThemeId { get; set; }

        /// <inheritdoc />>
        public string CdsTitleId { get; set; }

        /// <inheritdoc />>
        public string Folder { get; set; }

        /// <inheritdoc />>
        public IEnumerable<long> ActiveDenominations => Denominations.Where(d => d.Active).Select(d => d.Value);

        /// <inheritdoc />>
        public IEnumerable<long> SupportedDenominations => Denominations.Select(d => d.Value);

        /// <inheritdoc />>
        public IEnumerable<IDenomination> Denominations { get; set; }

        /// <inheritdoc />>
        public IEnumerable<IWagerCategory> WagerCategories { get; set; }

        /// <inheritdoc />>
        public IEnumerable<IWinLevel> WinLevels { get; set; }

        /// <inheritdoc />>
        public string VariationId { get; set; }

        /// <inheritdoc />>
        public bool Enabled => Active && Denominations.Any(d => d.Active) && Status == GameStatus.None;

        /// <inheritdoc />>
        public bool EgmEnabled => Active && Denominations.Any(d => d.Active) &&
                                  (Status & GameStatus.DisabledBySystem) != GameStatus.DisabledBySystem;

        /// <inheritdoc />>
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

        /// <inheritdoc />>
        public IEnumerable<string> GameTags { get; set; }

        /// <inheritdoc />>
        public GameType GameType { get; set; }

        /// <inheritdoc />>
        public string GameSubtype { get; set; }

        /// <inheritdoc />>
        public BetOptionList BetOptionList { get; set; }

        /// <inheritdoc />>
        public BetOption ActiveBetOption { get; set; }

        /// <inheritdoc />>
        public LineOptionList LineOptionList { get; set; }

        /// <inheritdoc />>
        public LineOption ActiveLineOption { get; set; }

        /// <inheritdoc />>
        public BetLinePresetList BetLinePresetList { get; set; }

        /// <inheritdoc />>
        public long WinThreshold { get; set; }

        /// <inheritdoc />>
        public string Version { get; set; }

        /// <inheritdoc />>
        public DateTime ReleaseDate { get; set; }

        /// <inheritdoc />>
        public bool Upgraded { get; set; }

        /// <inheritdoc />>
        public bool AutoPlaySupported => GameType == GameType.Slot || GameType == GameType.Keno;

        /// <inheritdoc />>
        public int? MaximumProgressivePerDenom { get; set; }

        /// <inheritdoc />>
        public string ReferenceId { get; set; }

        /// <inheritdoc />>
        public int MechanicalReels { get; set; }

        /// <inheritdoc />>
        public int[] MechanicalReelHomeSteps { get; set; }

        /// <inheritdoc />>
        public GameCategory Category { get; set; }

        /// <inheritdoc />>
        public GameSubCategory SubCategory { get; set; }

        /// <inheritdoc />>
        public IEnumerable<Feature> Features { get; set; }

        /// <inheritdoc />>
        public IEnumerable<ISubGameDetails> SupportedSubGames { get; set; }

        /// <inheritdoc />>
        public IEnumerable<ISubGameDetails> ActiveSubGames { get; set; }

        /// <summary>
        ///     Create a shallow copy of this class
        /// </summary>
        /// <returns>A shallow copy of the class</returns>
        public GameDetail ShallowClone()
        {
            return (GameDetail)MemberwiseClone();
        }
    }
}