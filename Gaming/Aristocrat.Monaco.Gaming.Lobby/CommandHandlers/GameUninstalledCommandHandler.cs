namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameUninstalledCommandHandler : ICommandHandler<GameUninstalled>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameUninstalledCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameUninstalled command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameUninstalledAction());
    }
}
