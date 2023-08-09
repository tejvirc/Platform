namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attendant;

using System.Threading.Tasks;
using Fluxor;
using Services.Attendant;

public class AttendantEffects
{
    private readonly IAttendant _attendant;

    public AttendantEffects(IAttendant attendant)
    {
        _attendant = attendant;
    }

    [EffectMethod]
    public Task Effect(ToggleServiceRequestAction _, IDispatcher dispatcher)
    {
        _attendant.RequestOrCancelService();

        return Task.CompletedTask;
    }
}
