namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of 'getOptionConfigModeStatus' command of 'OptionConfig' G2S class.
    /// </summary>
    public class GetOptionConfigModeStatus : ICommandHandler<optionConfig, getOptionConfigModeStatus>
    {
        private readonly ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionConfigModeStatus" /> class using an egm and a optionConfig
        ///     status command builder.
        /// </summary>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="egm">The egm.</param>
        public GetOptionConfigModeStatus(
            IG2SEgm egm,
            ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionConfigModeStatus> command)
        {
            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionConfigModeStatus> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<optionConfigModeStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}