namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record StartupAction
{
    public StartupAction(LobbyConfiguration configuration)
    {
        Configuration = configuration;
    }

    public LobbyConfiguration Configuration { get; init; }
}
