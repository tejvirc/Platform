namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Kernel;
using Store;

public class SystemEnabledConsumer : Consumes<SystemEnabledEvent>
{
    private readonly IDispatcher _dispatcher;
    private readonly ISystemDisableManager _disableManager;

    public SystemEnabledConsumer(IDispatcher dispatcher, ISystemDisableManager disableManager)
    {
        _dispatcher = dispatcher;
        _disableManager = disableManager;
    }

    public override async Task ConsumeAsync(SystemEnabledEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new PlatformEnabledAction(_disableManager.IsDisabled, _disableManager.DisableImmediately));
    }
}
