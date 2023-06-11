namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public class StartupAction
{
    public StartupAction(LobbyConfiguration configuration)
    {
        Configuration = configuration;
    }

    public LobbyConfiguration Configuration { get; }
}
