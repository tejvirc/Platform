namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using Fluxor;
using Store;

public class SystemDisabledCommandHandler : ICommandHandler<SystemDisabled>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public SystemDisabledCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(SystemDisabled command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new SystemDisabledAction(command.Priority, command.IsSystemDisabled, command.IsSystemDisableImmediately,
            command.DisableKeys, command.ImmediateDisableKeys));
    }
}
