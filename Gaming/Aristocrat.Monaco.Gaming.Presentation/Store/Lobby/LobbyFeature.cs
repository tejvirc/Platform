namespace Aristocrat.Monaco.Gaming.Presentation.Store.Lobby;

using Fluxor;

public class LobbyFeature : Feature<LobbyState>
{
    public override string GetName() => "Lobby";

    protected override LobbyState GetInitialState()
    {
        return new LobbyState();
    }
}
