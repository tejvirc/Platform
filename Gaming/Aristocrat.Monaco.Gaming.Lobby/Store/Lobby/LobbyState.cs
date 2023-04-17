namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Collections.Immutable;
using Fluxor;
using Models;

[FeatureState]
public record LobbyState
{
    public bool IsGamesLoaded { get; set; }

    public string BackgroundImagePath { get; set; }

    public bool IsSystemEnabled { get; set; }

    public ImmutableList<InfoOverlayText> InfoOverlayTextItems { get; set; }
}
