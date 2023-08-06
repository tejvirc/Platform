namespace Aristocrat.Monaco.Gaming.Lobby.Store.Upi;

using Services.Upi;
using Fluxor;
using System.Threading.Tasks;

public class UpiEffects
{
    private readonly IUpiService _upiService;

    public UpiEffects(IUpiService upiService)
    {
        _upiService = upiService;
    }

    [EffectMethod]
    public async Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateServiceAvailable(_upiService.GetIsServiceAvailable()));
        await dispatcher.DispatchAsync(new UpdateVolumeControlEnabled(_upiService.GetIsVolumeControlEnabled()));
    }
}
