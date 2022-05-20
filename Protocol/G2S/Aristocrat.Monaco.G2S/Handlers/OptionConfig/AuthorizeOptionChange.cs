namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Implementation of 'authorizeOptionChange' command of 'OptionConfig' G2S class.
    /// </summary>
    public class AuthorizeOptionChange : ICommandHandler<optionConfig, authorizeOptionChange>
    {
        private readonly IOptionChangeLogValidationService _changeLogValidationService;
        private readonly ICommandBuilder<IOptionConfigDevice, optionChangeStatus> _commandBuilder;
        private readonly IConfigurationService _configuration;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizeOptionChange" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="changeLogValidationService">The change log validation service.</param>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="configuration">The configuration.</param>
        public AuthorizeOptionChange(
            IG2SEgm egm,
            ICommandBuilder<IOptionConfigDevice, optionChangeStatus> commandBuilder,
            IOptionChangeLogValidationService changeLogValidationService,
            IConfigurationService configuration)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _changeLogValidationService = changeLogValidationService ??
                                          throw new ArgumentNullException(nameof(changeLogValidationService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, authorizeOptionChange> command)
        {
            var error = await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var errorCode = _changeLogValidationService.Validate(command.Command.transactionId);

            return !string.IsNullOrEmpty(errorCode) ? new Error(errorCode) : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, authorizeOptionChange> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

                AuthorizeAsync(command.Command.transactionId, command.HostId);

                var response = command.GenerateResponse<optionChangeStatus>();
                response.Command.configurationId = command.Command.configurationId;
                response.Command.transactionId = command.Command.transactionId;
                await _commandBuilder.Build(device, response.Command);
            }
        }

        private void AuthorizeAsync(long transactionId, int hostId)
        {
            Task.Run(() => Authorize(transactionId, hostId));
        }

        private void Authorize(long transactionId, int hostId)
        {
            _configuration.Authorize(transactionId, hostId);
        }
    }
}