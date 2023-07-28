namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using Aristocrat.Monaco.Application;
using Aristocrat.Monaco.Application.Contracts;
using Aristocrat.PackageManifest.Extension.v100;
using Commands;
using Contracts.Lobby;
using Fluxor;
using Microsoft.Extensions.Logging;
using Services;
using Store;
using Vgt.Client12.Application.OperatorMenu;

internal class LobbyEngine : ILobby
{
    private readonly ILogger<LobbyEngine> _logger;
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;
    private readonly IOperatorMenuLauncher _operatorMenuLauncher;
    private readonly ILayoutManager _layoutManager;
    private readonly IApplicationCommands _commands;

    public LobbyEngine(
        ILogger<LobbyEngine> logger,
        IStore store,
        IDispatcher dispatcher,
        LobbyConfiguration configuration,
        IOperatorMenuLauncher operatorMenuLauncher,
        ILayoutManager layoutManager,
        IApplicationCommands commands)
    {
        _logger = logger;
        _store = store;
        _dispatcher = dispatcher;
        _configuration = configuration;
        _operatorMenuLauncher = operatorMenuLauncher;
        _layoutManager = layoutManager;
        _commands = commands;
    }

    public void CreateWindow()
    {
        _store.InitializeAsync().Wait();

        _dispatcher.Dispatch(new StartupAction(_configuration));

        _operatorMenuLauncher.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);

        _layoutManager.InitializeAsync().Wait();
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        _dispatcher.Dispatch(new ShutdownAction());

        _commands.ShutdownCommand.Execute(null);

        _layoutManager.DestroyWindows();
    }
}
