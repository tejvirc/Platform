namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Common.Exceptions;
    using Kernel;
    using Kernel.Contracts;

    public class RegistrationCommandHandler : ICommandHandler<RegistrationCommand>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IPropertiesManager _properties;

        public RegistrationCommandHandler(IRegistrationService registrationService, IPropertiesManager properties)
        {
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public async Task Handle(RegistrationCommand command, CancellationToken token = default)
        {
            try
            {
                var serialNumber = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                var machineId = _properties.GetValue(ApplicationConstants.MachineId, (uint)0).ToString();
                var version = _properties.GetValue(KernelConstants.SystemVersion, string.Empty);
                var message = new RegistrationMessage(serialNumber, machineId, version);
                var results = await _registrationService.RegisterClient(message, token);
                switch (results.ResponseCode)
                {
                    case ResponseCode.Ok:
                        _properties.SetProperty(BingoConstants.BingoServerVersion, results.ServerVersion);
                        break;
                    case ResponseCode.Rejected:
                        throw new RegistrationException(
                            "Registration failed to communicate to the server",
                            RegistrationFailureReason.Rejected);
                    default:
                        throw new RegistrationException(
                            "Registration failed to communicate to the server",
                            RegistrationFailureReason.NoResponse);
                }
            }
            catch (Exception e) when (!(e is RegistrationException))
            {
                throw new RegistrationException(
                    "Registration failed to communicate to the server",
                    e,
                    RegistrationFailureReason.NoResponse);
            }
        }
    }
}