namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Accounting.Contracts;
using Fluxor;
using Store;
using UI.Utils;

public class BankBalanceChangedConsumer : Consumes<BankBalanceChangedEvent>
{
    private readonly IDispatcher _dispatcher;

    public BankBalanceChangedConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(BankBalanceChangedEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new UpdateCreditsAction(OverlayMessageUtils.ToCredits(theEvent.NewBalance)));
    }
}
