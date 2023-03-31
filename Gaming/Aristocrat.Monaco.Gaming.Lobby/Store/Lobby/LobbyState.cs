namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Fluxor;

[FeatureState]
public record LobbyState
{
    public bool IsGamesLoaded { get; set; }

    public string BackgroundImagePath { get; set; }

    public bool IsSystemEnabled { get; set; }
}
