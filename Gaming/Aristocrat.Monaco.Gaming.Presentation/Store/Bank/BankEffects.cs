namespace Aristocrat.Monaco.Gaming.Presentation.Store.Bank;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.Bank;

public class BankEffects
{
    private readonly IBankService _bankService;

    public BankEffects(IBankService bankService)
    {
        _bankService = bankService;
    }

    [EffectMethod(typeof(StartupAction))]
    public async Task Startup(IDispatcher dispatcher)
    {
        var credits = _bankService.GetBalance();

        await dispatcher.DispatchAsync(new BankUpdateCreditsAction(credits));
    }
}
