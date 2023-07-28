namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class InitializationCompletedCommandHandler : ICommandHandler<InitializationCompleted>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public InitializationCompletedCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(InitializationCompleted command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new SystemInitializedAction());
    }
}
