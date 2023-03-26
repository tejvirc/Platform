namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Kernel;
using Store;

public class SystemDisabledConsumer : Consumes<SystemDisabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public SystemDisabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override Task ConsumeAsync(SystemDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new SystemDisabledAction(theEvent.Priority));

        return Task.CompletedTask;
    }
}
