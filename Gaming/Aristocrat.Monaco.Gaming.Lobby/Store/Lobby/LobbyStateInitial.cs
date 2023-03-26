namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Fluxor;
using Kernel;

public class LobbyStateInitial : Feature<LobbyState>
{
    private readonly ISystemDisableManager _disableManager;

    public LobbyStateInitial(ISystemDisableManager disableManager)
    {
        _disableManager = disableManager;
    }

    public override string GetName() => nameof(LobbyState);

    protected override LobbyState GetInitialState()
    {
        return new LobbyState
        {
            IsSystemEnabled = !_disableManager.IsDisabled
        };
    }
}
