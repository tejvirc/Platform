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

    public sealed class RegistrationCommandHandler : ICommandHandler<RegistrationCommand>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IPropertiesManager _properties;
        private readonly INetworkInformationProvider _networkInformationProvider;

        public RegistrationCommandHandler(
            IRegistrationService registrationService,
            IPropertiesManager properties,
            INetworkInformationProvider networkInformationProvider)
        {
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _networkInformationProvider = networkInformationProvider ??
                                          throw new ArgumentNullException(nameof(networkInformationProvider));
        }

        public async Task Handle(RegistrationCommand command, CancellationToken token = default)
        {
            try
            {
                var message = GetRegistrationMessage();
                var results = await _registrationService.RegisterClient(message, token);
                HandleResponse(results);
            }
            catch (Exception e) when (e is not RegistrationException)
            {
                throw new RegistrationException(
                    "Registration failed to communicate to the server",
                    e,
                    RegistrationFailureReason.NoResponse);
            }
        }

        private void HandleResponse(RegistrationResults results)
        {
            switch (results.ResponseCode)
            {
                case ResponseCode.Ok:
                    _properties.SetProperty(BingoConstants.BingoServerVersion, results.ServerVersion);
                    break;
                case ResponseCode.Rejected:
                    throw new RegistrationException(
                        "Registration failed with a rejected response from the server",
                        RegistrationFailureReason.Rejected);
                default:
                    throw new RegistrationException(
                        "Registration failed to communicate to the server",
                        RegistrationFailureReason.NoResponse);
            }
        }

        private RegistrationMessage GetRegistrationMessage()
        {
            var serialNumber = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            var machineId = _properties.GetValue(ApplicationConstants.MachineId, (uint)0).ToString();
            var version = _properties.GetValue(KernelConstants.SystemVersion, string.Empty);
            var physicalAddress = _networkInformationProvider.GetPhysicalAddress().ToString();
            return new RegistrationMessage(serialNumber, machineId, version, physicalAddress);
        }
    }
}