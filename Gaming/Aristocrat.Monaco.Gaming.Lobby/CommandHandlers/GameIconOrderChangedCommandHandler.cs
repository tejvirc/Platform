namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameIconOrderChangedCommandHandler : ICommandHandler<GameIconOrderChanged>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameIconOrderChangedCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameIconOrderChanged command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameIconOrderChangedAction());
    }
}
