namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record StartupAction
{
    public StartupAction(LobbyConfiguration configuration)
    {
        Configuration = configuration;
    }

    public LobbyConfiguration Configuration { get; init; }
}
