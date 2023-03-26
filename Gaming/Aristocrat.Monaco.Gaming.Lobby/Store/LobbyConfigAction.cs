namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public class LobbyConfigAction
{
    public LobbyConfigAction(LobbyConfiguration configuration)
    {
        Configuration = configuration;
    }

    public LobbyConfiguration Configuration { get; }
}
