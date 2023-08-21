namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;

    /// <summary>
    ///     Implementation of 'getOptionList' command of 'OptionConfig' G2S class.
    /// </summary>
    public class GetOptionList : ICommandHandler<optionConfig, getOptionList>
    {
        private readonly IOptionListCommandBuilder _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionList" /> class using an egm and a optionConfig status command
        ///     builder.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="commandBuilder">Command builder.</param>
        public GetOptionList(IG2SEgm egm, IOptionListCommandBuilder commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionList> command)
        {
            if (!command.Command.IncludeAllDevices() &&
                !_egm.Devices.Where(x => x.DeviceClass == command.Command.deviceClass.TrimmedDeviceClass())
                    .Any(x => x.Active))
            {
                return new Error(ErrorCode.G2S_OCX001);
            }

            if (command.Command.deviceId != DeviceId.All)
            {
                var specifiedDevice = _egm.Devices.FirstOrDefault(x => x.Id == command.Command.deviceId);

                if (specifiedDevice == null)
                {
                    return new Error(ErrorCode.G2S_OCX001);
                }

                if (!specifiedDevice.Active)
                {
                    return new Error(ErrorCode.G2S_OCX001);
                }
            }

            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionList> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<optionList>();

            var commandBuilderParameters = new OptionListCommandBuilderParameters
            {
                IncludeAllDeviceClasses = command.Command.IncludeAllDevices(),
                IncludeAllDevices = command.Command.deviceId == DeviceId.All,
                IncludeAllGroups = command.Command.optionGroupId.Equals(DeviceClass.G2S_all),
                IncludeAllOptions = command.Command.optionId.Equals(DeviceClass.G2S_all),
                IncludeDetails = command.Command.optionDetail,
                DeviceClass = command.Command.deviceClass,
                DeviceId = command.Command.deviceId,
                OptionGroupId = command.Command.optionGroupId,
                OptionId = command.Command.optionId
            };

            await _commandBuilder.Build(device, response.Command, commandBuilderParameters);
        }
    }
}