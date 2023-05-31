namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Attract;
using Chooser;
using Fluxor;
using Lobby;

[FeatureState]
public record MainState
{
    public MainState(
        IState<LobbyState> lobbyState,
        IState<ChooserState> chooserState,
        IState<AttractState> attractState)
    {
        LobbyState = lobbyState.Value;
        Chooser = chooserState.Value;
        Attract = attractState.Value;
    }

    public ChooserState Chooser { get; }

    public AttractState Attract { get; }

    public LobbyState LobbyState { get; }
}
