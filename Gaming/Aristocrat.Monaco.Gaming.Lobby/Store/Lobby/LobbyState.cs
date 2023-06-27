namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System;
using System.Collections.Immutable;
using Aristocrat.Monaco.Application.Contracts.EdgeLight;
using Contracts.Lobby;
using Contracts.Models;
using Fluxor;
using Models;

[FeatureState]
public partial record LobbyState
{
    public IImmutableList<GameInfo> Games { get; set; } = ImmutableList<GameInfo>.Empty;

    public IImmutableList<AttractVideoInfo> AttractList { get; set; } = ImmutableList<AttractVideoInfo>.Empty;

    public int UniqueThemesCount { get; set; }

    public bool IsSingleGame { get; set; }

    public bool IsGamesLoaded { get; set; }

    public string? BackgroundImagePath { get; set; }

    public ImmutableList<InfoOverlayText>? InfoOverlayTextItems { get; set; }

    public bool AllowGameInCharge { get; set; }

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

    public bool IsTabView { get; set; }

    public bool IsIdleTextScrolling { get; set; }

    public BannerDisplayMode BannerDisplayMode { get; set; }

    public string? TopImageResourceKey { get; set; }

    public string? TopperImageResourceKey { get; set; }

    public bool IsCashingOut { get; set; }

    public LobbyCashOutState CurrentCashOutState { get; set; }

    public EdgeLightState CurrentEdgeLightState { get; set; }

    public bool IsVoucherNotificationActive { get; set; }

    public bool IsProgressiveGameDisabledNotificationActive { get; set; }

    public bool IsPlayerInfoRequestActive { get; set; }
}
