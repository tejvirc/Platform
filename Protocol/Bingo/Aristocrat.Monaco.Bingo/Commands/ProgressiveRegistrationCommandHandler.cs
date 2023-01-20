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
    using Protocol.Common.Storage.Entity;

    public class ProgressiveRegistrationCommandHandler : ICommandHandler<ProgressiveRegistrationCommand>
    {
        private readonly IProgressiveRegistrationService _registrationService;
        private readonly IPropertiesManager _properties;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public ProgressiveRegistrationCommandHandler(
            IProgressiveRegistrationService registrationService,
            IPropertiesManager properties,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task Handle(ProgressiveRegistrationCommand command, CancellationToken token = default)
        {
            try
            {
                var machineId = _properties.GetValue(ApplicationConstants.MachineId, (uint)0).ToString();
                var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_properties);
                var gameTitleId = (int)(gameConfiguration?.GameTitleId ?? 0);
                var message = new ProgressiveRegistrationMessage(machineId, gameTitleId);
                var results = await _registrationService.RegisterClient(message, token);
                switch (results.ResponseCode)
                {
                    case ResponseCode.Ok:
                        break;
                    case ResponseCode.Rejected:
                        throw new RegistrationException(
                            "Progressive registration failed to communicate to the server",
                            RegistrationFailureReason.Rejected);
                    default:
                        throw new RegistrationException(
                            "Progressive registration failed to communicate to the server",
                            RegistrationFailureReason.NoResponse);
                }
            }
            catch (Exception e) when (!(e is RegistrationException))
            {
                throw new RegistrationException(
                    "Progressive registration failed to communicate to the server",
                    e,
                    RegistrationFailureReason.NoResponse);
            }
        }
    }
}