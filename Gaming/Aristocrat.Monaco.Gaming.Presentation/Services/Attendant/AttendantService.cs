namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attendant;

using System.Threading.Tasks;
using Gaming.Contracts;
using Microsoft.Extensions.Logging;

public class AttendantService : IAttendantService
{
    private readonly ILogger<AttendantService> _logger;
    private readonly Gaming.Contracts.IAttendantService _attendantService;

    public AttendantService(ILogger<AttendantService> logger, Gaming.Contracts.IAttendantService attendantService)
    {
        _logger = logger;
        _attendantService = attendantService;
    }

    public Task RequestOrCancelServiceAsync()
    {
        _attendantService.OnServiceButtonPressed();

        return Task.CompletedTask;
    }
}
