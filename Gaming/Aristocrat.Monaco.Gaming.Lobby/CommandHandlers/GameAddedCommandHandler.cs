namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameAddedCommandHandler : ICommandHandler<GameAdded>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameAddedCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameAdded command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameAddedAction());
    }
}
