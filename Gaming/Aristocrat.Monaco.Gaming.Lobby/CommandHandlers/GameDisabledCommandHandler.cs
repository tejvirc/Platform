namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameDisabledCommandHandler : ICommandHandler<GameDisabled>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameDisabledCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameDisabled command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameDisabledAction());
    }
}
