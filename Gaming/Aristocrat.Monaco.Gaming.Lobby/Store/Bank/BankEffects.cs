namespace Aristocrat.Monaco.Gaming.Lobby.Store.Bank;

using System.Threading.Tasks;
using Fluxor;
using Services.Bank;

public class BankEffects
{
    private readonly IBankService _bankService;

    public BankEffects(IBankService bankService)
    {
        _bankService = bankService;
    }

    [EffectMethod]
    public async Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        var credits = _bankService.GetBalance();

        await dispatcher.DispatchAsync(new UpdateCreditsAction(credits));
    }
}
