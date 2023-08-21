namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
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
    ///     Implementation of 'authorizeCommChange' command of 'CommConfig' G2S class.
    /// </summary>
    public class AuthorizeCommChange : ICommandHandler<commConfig, authorizeCommChange>
    {
        private readonly ICommChangeLogValidationService _changeLogValidationService;
        private readonly ICommandBuilder<ICommConfigDevice, commChangeStatus> _commandBuilder;
        private readonly IConfigurationService _configuration;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizeCommChange" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="changeLogValidationService">Service for checking validation logic for commConfig change log.</param>
        /// <param name="configuration">An <see cref="IConfigurationService" /> instance.</param>
        public AuthorizeCommChange(
            IG2SEgm egm,
            ICommandBuilder<ICommConfigDevice, commChangeStatus> commandBuilder,
            ICommChangeLogValidationService changeLogValidationService,
            IConfigurationService configuration)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _changeLogValidationService = changeLogValidationService ??
                                          throw new ArgumentNullException(nameof(changeLogValidationService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, authorizeCommChange> command)
        {
            var error = await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var errorCode = _changeLogValidationService.Validate(command.Command.transactionId);

            return !string.IsNullOrEmpty(errorCode) ? new Error(errorCode) : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, authorizeCommChange> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

                _configuration.Authorize(command.Command.transactionId, command.HostId);

                var response = command.GenerateResponse<commChangeStatus>();
                response.Command.configurationId = command.Command.configurationId;
                response.Command.transactionId = command.Command.transactionId;
                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}