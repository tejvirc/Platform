namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Kernel;

    public sealed class ClientConnectedCommandHandler : ICommandHandler<ClientConnectedCommand>
    {
        private readonly ICommandService _commandService;
        private readonly IPropertiesManager _properties;

        public ClientConnectedCommandHandler(
            ICommandService commandService,
            IPropertiesManager properties)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public async Task Handle(ClientConnectedCommand command, CancellationToken token = default)
        {
            var serialNumber = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            await _commandService.HandleCommands(serialNumber, token);
        }
    }
}