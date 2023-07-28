namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class GameUpgradedCommandHandler : ICommandHandler<GameUpgraded>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public GameUpgradedCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(GameUpgraded command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new GameUpgradedAction());
    }
}
