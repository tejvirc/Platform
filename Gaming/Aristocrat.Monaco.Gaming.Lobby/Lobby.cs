namespace Aristocrat.Monaco.Gaming.Lobby;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.UI.Common;
using Contracts;
using Contracts.Lobby;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Hosting;
using Store;

public class Lobby : ILobby
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IPropertiesManager _properties;

    public Lobby(IStore store, IDispatcher dispatcher, IPropertiesManager properties)
    {
        _store = store;
        _dispatcher = dispatcher;
        _properties = properties;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _store.InitializeAsync();

        var config = _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
        if (config != null)
        {
            _dispatcher.Dispatch(new LobbyConfigAction(config));
        }
    }

    public void CreateWindow()
    {
        _store.InitializeAsync().Wait();

        var config = _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
        if (config != null)
        {
            _dispatcher.Dispatch(new LobbyConfigAction(config));
        }

        ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
    }

    public void Show()
    {
    }

    public void Hide()
    {
    }

    public void Close()
    {
    }
}
