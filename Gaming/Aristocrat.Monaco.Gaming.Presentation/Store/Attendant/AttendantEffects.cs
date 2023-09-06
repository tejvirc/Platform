namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

using System.Threading.Tasks;
using Fluxor;
using Services.Attendant;

public class AttendantEffects
{
    private readonly IAttendantService _attendantService;

    public AttendantEffects(IAttendantService attendantService)
    {
        _attendantService = attendantService;
    }

    [EffectMethod(typeof(AttendantRequestOrCancelServiceAction))]
    public async Task RequestOrCancel(IDispatcher _)
    {
        await _attendantService.RequestOrCancelServiceAsync();
    }
}
