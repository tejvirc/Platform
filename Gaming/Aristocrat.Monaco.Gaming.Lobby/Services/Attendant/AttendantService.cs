namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attendant;

public class AttendantService : IAttendantService
{
    private readonly Contracts.IAttendantService _attendantService;

    public AttendantService(Contracts.IAttendantService attendantService)
    {
        _attendantService = attendantService;
    }

    public void RequestService()
    {
        _attendantService.OnServiceButtonPressed();
    }
}
