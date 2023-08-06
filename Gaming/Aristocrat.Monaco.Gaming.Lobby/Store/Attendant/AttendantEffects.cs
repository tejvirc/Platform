namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attendant;

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

    [EffectMethod]
    public Task Effect(RequestServiceAction _, IDispatcher dispatcher)
    {
        _attendantService.RequestService();

        return Task.CompletedTask;
    }
}
