namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetSpcState : ICommandHandler<spc, setSpcState>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<ISpcDevice, spcStatus> _commandBuilder;

        public SetSpcState(IG2SEgm egm, ICommandBuilder<ISpcDevice, spcStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<spc, setSpcState> command)
        {
            return await Sanction.OnlyOwner<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<spc, setSpcState> command)
        {
            var device = _egm.GetDevice<ISpcDevice>(command.IClass.deviceId);

            var enabled = command.Command.enable;

            if (device.HostEnabled != enabled)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = enabled;
            }

            var response = command.GenerateResponse<spcStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
