namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Kernel.Contracts.Events;
using Store;

public class InitializationCompletedConsumer : Consumes<InitializationCompletedEvent>
{
    private readonly IDispatcher _dispatcher;

    public InitializationCompletedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(InitializationCompletedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new PlatformInitializedAction());
    }
}
