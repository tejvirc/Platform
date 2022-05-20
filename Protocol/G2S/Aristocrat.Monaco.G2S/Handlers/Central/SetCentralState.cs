namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetCentralState : ICommandHandler<central, setCentralState>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<ICentralDevice, centralStatus> _commandBuilder;

        public SetCentralState(IG2SEgm egm, ICommandBuilder<ICentralDevice, centralStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<central, setCentralState> command)
        {
            return await Sanction.OnlyOwner<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<central, setCentralState> command)
        {
            var device = _egm.GetDevice<ICentralDevice>(command.IClass.deviceId);

            var enabled = command.Command.enable;

            if (device.HostEnabled != enabled)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = enabled;
            }

            var response = command.GenerateResponse<centralStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}