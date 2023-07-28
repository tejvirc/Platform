namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameEnabledCommandHandler : ICommandHandler<GameEnabled>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameEnabledCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameEnabled command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameEnabledAction());
    }
}
