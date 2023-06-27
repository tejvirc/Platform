namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
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

    public override Task ConsumeAsync(InitializationCompletedEvent theEvent, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new SystemInitializedAction());

        return Task.CompletedTask;
    }
}
