namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attendant;

public class AttendantAgent : IAttendantAgent
{
    private readonly Contracts.IAttendantService _attendantService;

    public AttendantAgent(Contracts.IAttendantService attendantService)
    {
        _attendantService = attendantService;
    }

    public void RequestOrCancelService()
    {
        _attendantService.OnServiceButtonPressed();
    }
}
