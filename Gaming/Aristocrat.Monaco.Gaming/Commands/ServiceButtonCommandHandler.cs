namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    public class ServiceButtonCommandHandler : ICommandHandler<ServiceButton>
    {
        private readonly IAttendantService _attendantService;

        public ServiceButtonCommandHandler(IAttendantService attendantService)
        {
            _attendantService = attendantService ?? throw new ArgumentNullException(nameof(attendantService));
        }

        public void Handle(ServiceButton command)
        {
            _attendantService.OnServiceButtonPressed();
        }
    }
}
