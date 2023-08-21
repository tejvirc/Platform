namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

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
        _dispatcher.Dispatch(new PlatformDisabledAction(theEvent.Priority, _disableManager.IsDisabled, _disableManager.DisableImmediately));

        return Task.CompletedTask;
    }
}
