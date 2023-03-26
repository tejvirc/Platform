namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using Fluxor;
using Katalogo;
using Microsoft.Extensions.DependencyInjection;
using Views;

public class GamingLobbyCatalog : Catalog
{
    public GamingLobbyCatalog(IServiceCollection services)
        : base(services)
    {
        services.AddView<DefaultLobbyView>();
        services.AddView<ChooserView>();

        services.AddHostedService<Lobby>();

        services.AddFluxor(options => options.ScanAssemblies(typeof(Lobby).Assembly));
    }
}
