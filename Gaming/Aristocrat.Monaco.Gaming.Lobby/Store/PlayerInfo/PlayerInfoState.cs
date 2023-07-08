namespace Aristocrat.Monaco.Gaming.Lobby.Store.PlayerInfo;

using Fluxor;

[FeatureState]
public record PlayerInfoState
{
    public bool IsActive { get; set; }
}
