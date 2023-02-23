namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;
    using Gaming.Contracts;
    using Gaming.Contracts.Models;
    using PackageManifest.Models;

    /// <summary>
    ///     Internal class for testing
    /// </summary>
    internal class TestGameProfile : IGameDetail
    {
        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime InstallDate { get; set; }

        public bool Upgraded { get; set; }

        public int Id { get; set; }

        public string ThemeId { get; set; }

        public string PaytableId { get; set; }

        public bool Active { get; set; } = true;

        public string PositionPriorityKey { get; }

        public int MaximumWagerCredits { get; set; }

        public int MinimumWagerCredits { get; set; }

        public long MaximumWinAmount { get; set; }

        public bool ProgressiveAllowed { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool CentralAllowed { get; set; }

        public IEnumerable<ICdsGameInfo> CdsGameInfos { get; set; }

        public decimal MaximumPaybackPercent { get; set; }

        public decimal MinimumPaybackPercent { get; set; }

        public string ThemeName { get; set; }

        public string PaytableName { get; set; }

        public string CdsThemeId { get; set; }

        public string CdsTitleId { get; set; }

        public long? ProductCode { get; set; }

        public IEnumerable<long> ActiveDenominations { get; set; }

        public IEnumerable<long> SupportedDenominations => Denominations.Select(d => d.Value);

        public IEnumerable<IDenomination> Denominations { get; set; }

        public IEnumerable<IWagerCategory> WagerCategories { get; set; }

        public IEnumerable<IWinLevel> WinLevels { get; set; }

        public string VariationId { get; set; }

        public string ReferenceId { get; set; }

        public bool Enabled { get; set; }

        public bool EgmEnabled { get; set; }

        public GameStatus Status { get; set; }

        public IEnumerable<string> GameTags { get; set; }

        public BetOptionList BetOptionList { get; set; } = new BetOptionList(new List<c_betOption>());

        public BetOption ActiveBetOption { get; }

        public LineOptionList LineOptionList { get; } = new LineOptionList(new List<c_lineOption>());

        public LineOption ActiveLineOption { get; }

        public BetLinePresetList BetLinePresetList { get; }

        public long WinThreshold { get; }

        public int? MaximumProgressivePerDenom { get; }

        public bool AutoPlaySupported { get; }

        public GameType GameType { get; set; }

        public string GameSubtype { get; set; }

        public string BetOption { get; set; }

        public string BonusBet { get; set; }

        public string Folder { get; }

        public string GameDll { get; }

        public Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; }

        public string DisplayMeterName { get; }

        public IEnumerable<string> AssociatedSapDisplayMeterName { get; set; }

        public GameIconType GameIconType { get; set; }

        public long InitialValue { get; }

        public string TargetRuntime { get; }

        public GameCategory Category { get; }

        public GameSubCategory SubCategory { get; }

        public IEnumerable<Feature> Features { get; set; }

        public int MechanicalReels { get; set; }

        public int[] MechanicalReelHomeSteps { get; set; }

        public bool HasExtendedRtpInformation
        {
            get
            {
                return WagerCategories.Any(
                    w =>
                        w.MinBaseRtpPercent != default ||
                        w.MaxBaseRtpPercent != default ||
                        w.MinSapStartupRtpPercent != default ||
                        w.MaxSapStartupRtpPercent != default ||
                        w.SapIncrementRtpPercent != default ||
                        w.MinLinkStartupRtpPercent != default ||
                        w.MaxLinkStartupRtpPercent != default ||
                        w.LinkIncrementRtpPercent != default);
            }
        }
    }
}