namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System;
using System.Collections.Immutable;
using Contracts.Lobby;
using Contracts.Models;
using global::Fluxor;
using Models;

[FeatureState]
public partial record LobbyState
{
    public IImmutableList<GameInfo> Games { get; set; } = ImmutableList<GameInfo>.Empty;

    public int UniqueThemesCount { get; set; }

    public bool IsSingleGame { get; set; }

    public bool IsMultiLanguage { get; set; }

    public bool IsGamesLoaded { get; set; }

    public bool IsAgeWarningNeeded { get; set; }

    public string? BackgroundImagePath { get; set; }

    public ImmutableList<InfoOverlayText>? InfoOverlayTextItems { get; set; }

    public IntPtr GameMainHandle { get; set; }

    public IntPtr GameTopHandle { get; set; }

    public IntPtr GameTopperHandle { get; set; }

    public IntPtr GameButtonDeckHandle { get; set; }

    public bool IsSystemDisabled { get; set; }

    public bool IsSystemDisableImmediately { get; set; }

    public bool AllowGameInCharge { get; set; }

    public bool UseGen8IdleModeEdgeLightingOverride { get; set; }

    public DateTime DenomCheckTime { get; set; }

    public DateTime LastUserInteractionTime { get; set; }

    public int DenomFilter { get; set; }

    public GameType GameFilter { get; set; }

    public bool IsAlternateTopImageActive { get; set; }

    public bool IsAlternateTopperImageActive { get; set; }

    public string? IdleText { get; set; }

    public bool IsScrollingIdleTextEnabled { get; set; }

    public bool IsIdleTextPaused { get; set; }

    public double Credits { get; set; }

    public bool HideIdleTextOnCashIn { get; set; }

    public bool IsTabView { get; set; }

    public bool IsIdleTextScrolling { get; set; }

    public BannerDisplayMode BannerDisplayMode { get; set; }

    public bool HasAttractIntroVideo { get; set; }

    public int CurrentAttractIndex { get; set; }

    public int ConsecutiveAttractVideos { get; set; }

    public int ConsecutiveAttractCount { get; set; }

    public IImmutableList<string> RotateTopImageAfterAttractVideo { get; set; } = ImmutableList<string>.Empty;

    public int RotateTopImageAfterAttractVideoCount { get; set; }

    public bool IsRotateTopImageAfterAttractVideo { get; set; }

    public IImmutableList<string> RotateTopperImageAfterAttractVideo { get; set; } = ImmutableList<string>.Empty;

    public int RotateTopperImageAfterAttractVideoCount { get; set; }

    public bool IsRotateTopperImageAfterAttractVideo { get; set; }

    public int AttractModeTopperImageIndex { get; set; } = -1;

    public int AttractModeTopImageIndex { get; set; } = -1;

    public string? TopImageResourceKey { get; set; }

    public string? TopperImageResourceKey { get; set; }
}
