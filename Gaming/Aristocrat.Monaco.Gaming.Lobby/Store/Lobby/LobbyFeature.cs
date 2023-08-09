namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Fluxor;

internal class LobbyFeature : Feature<LobbyState>
{
    public override string GetName() => "Lobby";

    protected override LobbyState GetInitialState()
    {
        return new LobbyState();
    }
}
