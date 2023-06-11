namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System;
using System.Collections.Immutable;
using Fluxor;
using Models;

[FeatureState]
public record LobbyState
{
    public IImmutableList<GameInfo> Games { get; set; } = ImmutableList<GameInfo>.Empty;

    public bool AllowGameInCharge { get; set; }

    public bool AllowGameAutoLaunch { get; set; } = true;

    public bool IsMultiLanguage { get; set; }

    public bool IsGamesLoaded { get; set; }

    public string? BackgroundImagePath { get; set; }

    public ImmutableList<InfoOverlayText>? InfoOverlayTextItems { get; set; }

    public IntPtr GameMainHandle { get; set; }

    public IntPtr GameTopHandle { get; set; }

    public IntPtr GameTopperHandle { get; set; }

    public IntPtr GameButtonDeckHandle { get; set; }
}
