namespace Aristocrat.Monaco.Gaming.Lobby.Store.Reserve;

using Fluxor;

[FeatureState]
public record ReserveState
{
    public bool IsActive { get; set; }
}
