namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Application.Contracts.OperatorMenu;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class OperatorMenuExitingConsumer : Consumes<OperatorMenuExitingEvent>
{
    private readonly IDispatcher _dispatcher;

    public OperatorMenuExitingConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(OperatorMenuExitingEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new ReplayExitAction());
    }
}
