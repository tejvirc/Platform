namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;
    using Monaco.Common.Storage;

    public class GetOptionChangeStatus : ICommandHandler<optionConfig, getOptionChangeStatus>
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly ICommandBuilder<IOptionConfigDevice, optionChangeStatus> _commandBuilder;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;

        public GetOptionChangeStatus(
            IG2SEgm egm,
            ICommandBuilder<IOptionConfigDevice, optionChangeStatus> commandBuilder,
            IMonacoContextFactory contextFactory,
            IOptionChangeLogRepository changeLogRepository)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
        }

        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionChangeStatus> command)
        {
            // If the transactionId included in the getOptionChangeStatus command references a change request that is
            // not associated with the device identified in the class-level element of the command,
            // the EGM SHOULD include error code G2S_OCX005 Invalid Transaction Identifier in its response
            using (var context = _contextFactory.CreateDbContext())
            {
                var optionChangeLog = _changeLogRepository.GetByTransactionId(context, command.Command.transactionId);

                if (optionChangeLog == null)
                {
                    return new Error(ErrorCode.G2S_OCX005);
                }

                if (optionChangeLog.DeviceId != command.Class.deviceId)
                {
                    return new Error(ErrorCode.G2S_OCX005);
                }
            }

            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<optionConfig, getOptionChangeStatus> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<optionChangeStatus>();
            response.Command.transactionId = command.Command.transactionId;
            response.Command.configurationId = command.Command.configurationId;

            await _commandBuilder.Build(device, response.Command);
        }
    }
}