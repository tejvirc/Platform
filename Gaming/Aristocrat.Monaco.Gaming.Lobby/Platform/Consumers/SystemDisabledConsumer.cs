namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Kernel;
using Store;

public class SystemDisabledConsumer : Consumes<SystemDisabledEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly ISystemDisableManager _disableManager;

    public SystemDisabledConsumer(IDispatcher dispatcher, ISystemDisableManager disableManager)
    {
        _dispatcher = dispatcher;
        _disableManager = disableManager;
    }

    public override Task ConsumeAsync(SystemDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(
            new SystemDisabledAction
            {
                Priority = theEvent.Priority,
                IsDisabled = _disableManager.IsDisabled,
                IsDisableImmediately = _disableManager.DisableImmediately,
                DisableKeys = ImmutableList.CreateRange(_disableManager.CurrentDisableKeys),
                ImmediateDisableKeys = ImmutableList.CreateRange(_disableManager.CurrentImmediateDisableKeys)
            });

        return Task.CompletedTask;
    }
}
