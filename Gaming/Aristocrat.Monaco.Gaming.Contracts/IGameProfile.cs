namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Models;
    using System;
    using System.Collections.Generic;
    using PackageManifest.Models;

    /// <summary>
    ///     Indicates the states of the game
    /// </summary>
    [Flags]
    public enum GameStatus
    {
        /// <summary>
        ///     Indicates the game is enabled
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates the game DLL was not found when trying to load
        /// </summary>
        GameFilesNotFound = 1,

        /// <summary>
        ///     Indicates the game is disabled by the backend
        /// </summary>
        DisabledByBackend = 2,

        /// <summary>
        ///     Indicates the game is disabled by the System/EGM for any other reason
        /// </summary>
        DisabledBySystem = 4
    }

    /// <summary>
    ///     Provides details about a game
    /// </summary>
    public interface IGameProfile : IGame
    {
        /// <summary>
        ///     Gets the maximum wager
        /// </summary>
        int MaximumWagerCredits { get; }

        /// <summary>
        ///     Gets the minimum wager
        /// </summary>
        int MinimumWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum win amount
        /// </summary>
        long MaximumWinAmount { get; }

        /// <summary>
        ///     Gets a value indicating whether central determinant game outcomes are supported
        /// </summary>
        bool CentralAllowed { get; }

        /// <summary>
        ///     Gets the cds game infos for this game
        /// </summary>
        IEnumerable<ICdsGameInfo> CdsGameInfos { get; }

        /// <summary>
        ///     Gets the maximum theoretical payback percentage for the game; a value of 0 (zero) indicates that the attribute is
        ///     not supported; otherwise, MUST be set to the maximum payback percentage of the game, which MUST be greater than 0
        ///     (zero). For example, a value of 9637 represents a maximum payback percentage of 96.37%
        /// </summary>
        decimal MaximumPaybackPercent { get; }

        /// <summary>
        ///     Gets the minimum theoretical payback percentage for the game; a value of 0 (zero) indicates that the attribute is
        ///     not supported; otherwise, MUST be set to the minimum payback percentage for the game, which MUST be greater than 0
        ///     (zero). For example, a value of 8245 represent
        /// </summary>
        decimal MinimumPaybackPercent { get; }

        /// <summary>
        ///     Gets the theme description
        /// </summary>
        string ThemeName { get; }

        /// <summary>
        ///     Gets the paytable description
        /// </summary>
        string PaytableName { get; }

        /// <summary>
        ///     Gets the CdsThemeId
        /// </summary>
        string CdsThemeId { get; }

        /// <summary>
        ///     Gets the CdsTitleId
        /// </summary>
        string CdsTitleId { get; }

        /// <summary>
        ///     Gets a unique product code.  The product code is not guaranteed to be unique across game profiles
        /// </summary>
        long? ProductCode { get; }

        /// <summary>
        ///     Gets the list of supported wager categories
        /// </summary>
        IEnumerable<IWagerCategory> WagerCategories { get; }

        /// <summary>
        ///     Gets the list of associated win levels
        /// </summary>
        IEnumerable<IWinLevel> WinLevels { get; }

        /// <summary>
        ///     Gets the variation Id.  This is closely related to the paytable, but can be used by the game/GDK to identify the maths used when the game is loaded
        /// </summary>
        string VariationId { get; }

        /// <summary>
        ///     Gets a value indicating whether the theme/paytable is enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        ///     Gets a value indicating whether the theme/paytable is enabled by the system/EGM
        /// </summary>
        bool EgmEnabled { get; }

        /// <summary>
        ///     Gets the status of the game play instance
        /// </summary>
        GameStatus Status { get; }

        /// <summary>
        ///     Gets the list of tags associated with the game
        /// </summary>
        IEnumerable<string> GameTags { get; }

        /// <summary>
        ///     Gets the bet options
        /// </summary>
        BetOptionList BetOptionList { get; }

        /// <summary>
        ///     Gets the active bet option
        /// </summary>
        BetOption ActiveBetOption { get; }

        /// <summary>
        ///     Gets the line options
        /// </summary>
        LineOptionList LineOptionList { get; }

        /// <summary>
        ///     Gets the active line option
        /// </summary>
        LineOption ActiveLineOption { get; }

        /// <summary>
        ///     Gets the line options
        /// </summary>
        BetLinePresetList BetLinePresetList { get; }

        /// <summary>
        ///     Gets the game type
        /// </summary>
        GameType GameType { get; }

        /// <summary>
        ///     Gets the game sub type
        /// </summary>
        string GameSubtype { get; }

        /// <summary>
        ///     Gets the win threshold
        /// </summary>
        long WinThreshold { get; }

        /// <summary>
        ///    Gets whether autoplay is supported
        /// </summary>
        bool AutoPlaySupported { get; }

        /// <summary>
        ///    Maximum progressives per Denom
        /// </summary>
        int? MaximumProgressivePerDenom { get; }

        /// <summary>
        ///     Gets the reference Id.  This can be used by the platform to
        ///     identify the reference variation used when the game is loaded
        /// </summary>
        string ReferenceId { get; }

        /// <summary>
        ///     Gets the game category
        /// </summary>
        GameCategory Category { get; }

        /// <summary>
        ///     Gets the game subcategory
        /// </summary>
        GameSubCategory SubCategory { get; }

        /// <summary>
        ///     Get the list of supported features
        /// </summary>
        IEnumerable<Feature> Features { get; }

        /// <summary>
        ///     Gets or sets the number of mechanical reels
        /// </summary>
        int MechanicalReels { get; set; }

        /// <summary>
        ///     Gets or sets the mechanical reel home steps
        /// </summary>
        int[] MechanicalReelHomeSteps { get; set; }

        /// <summary>
        ///    Gets or sets the maximum wager for higher-odd bets, for example, betting on a specific number in roulette.
        /// </summary>
        int MaximumWagerInsideCredits { get; set; }

        /// <summary>
        ///     Gets or sets the maximum wager for low-odds bets, for example, betting on red/black or even/odd in roulette
        /// </summary>
        int MaximumWagerOutsideCredits { get; set; }

        /// <summary>
        ///     Specifies that a game uses the next-to-highest bet-multiplier when calculating its Top Award.
        /// </summary>
        public bool NextToMaxBetTopAwardMultiplier { get; set; }

        /// <summary>
        ///     Gets for sets the animation files to pre-load.
        /// </summary>
        public IEnumerable<AnimationFile> PreloadedAnimationFiles { get; set; }
    }
}
