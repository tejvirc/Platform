namespace Aristocrat.Monaco.Gaming.Presentation.Store.Upi;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.Upi;

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
        await dispatcher.DispatchAsync(new AttendantUpdateServiceAvailableAction(_upiService.GetIsServiceAvailable()));
        await dispatcher.DispatchAsync(new AudioUpdateVolumeControlEnabledAction(_upiService.GetIsVolumeControlEnabled()));
    }
}
