namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attendant;

using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.Logging;

public class AttendantService : IAttendantService
{
    private readonly ILogger<AttendantService> _logger;
    private readonly Contracts.IAttendantService _attendantService;

    public AttendantService(ILogger<AttendantService> logger, Contracts.IAttendantService attendantService)
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
