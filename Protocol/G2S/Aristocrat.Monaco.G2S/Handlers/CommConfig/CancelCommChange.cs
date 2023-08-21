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
    ///     Implementation of 'cancelCommChange' command of 'CommConfig' G2S class.
    /// </summary>
    public class CancelCommChange : ICommandHandler<commConfig, cancelCommChange>
    {
        private readonly ICommandBuilder<ICommConfigDevice, commChangeStatus> _commandBuilder;
        private readonly IConfigurationService _configuration;
        private readonly IG2SEgm _egm;
        private readonly ICommChangeLogValidationService _validationService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CancelCommChange" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="changeLogValidation">The change log validation service.</param>
        /// <param name="configuration">An <see cref="IConfigurationService" /> instance.</param>
        /// <param name="commandBuilder">The command builder.</param>
        public CancelCommChange(
            IG2SEgm egm,
            ICommChangeLogValidationService changeLogValidation,
            IConfigurationService configuration,
            ICommandBuilder<ICommConfigDevice, commChangeStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _validationService = changeLogValidation ?? throw new ArgumentNullException(nameof(changeLogValidation));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, cancelCommChange> command)
        {
            var error = await Sanction.OnlyOwner<ICommConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var errorCode = _validationService.Validate(command.Command.transactionId);

            return !string.IsNullOrEmpty(errorCode) ? new Error(errorCode) : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, cancelCommChange> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

                _configuration.Cancel(command.Command.transactionId);

                var response = command.GenerateResponse<commChangeStatus>();
                response.Command.configurationId = command.Command.configurationId;
                response.Command.transactionId = command.Command.transactionId;
                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}