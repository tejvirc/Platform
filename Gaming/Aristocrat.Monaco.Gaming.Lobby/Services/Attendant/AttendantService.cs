namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attendant;

public class AttendantService : IAttendant
{
    private readonly IAttendantAgent _attendantAgent;

    public AttendantService(IAttendantAgent attendantAgent)
    {
        _attendantAgent = attendantAgent;
    }

    public void RequestOrCancelService()
    {
        _attendantAgent.RequestOrCancelService();
    }
}
