namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

using System.Threading.Tasks;
using Fluxor;
using Services.Attendant;

public class AttendantEffects
{
    private readonly IAttendantService _attendantAgent;

    public AttendantEffects(IAttendantService attendantAgent)
    {
        _attendantAgent = attendantAgent;
    }

    [EffectMethod(typeof(AttendantRequestOrCancelServiceAction))]
    public async Task RequestOrCancel(IDispatcher _)
    {
        await _attendantAgent.RequestOrCancelServiceAsync();
    }
}
