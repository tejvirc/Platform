namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using System;
using System.Collections.Immutable;
using Fluxor;
using Store;

public class SystemEnabledCommandHandler : ICommandHandler<SystemEnabled>
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;

    public SystemEnabledCommandHandler(IStore store, IDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public void Handle(SystemEnabled command)
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }

        _dispatcher.Dispatch(new SystemEnabledAction(command.IsSystemDisabled, command.IsSystemDisableImmediately,
            command.DisableKeys, command.ImmediateDisableKeys));
    }
}
