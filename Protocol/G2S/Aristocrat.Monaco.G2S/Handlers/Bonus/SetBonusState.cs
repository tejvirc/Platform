namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetBonusState : ICommandHandler<bonus, setBonusState>
    {
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        public SetBonusState(IG2SEgm egm, ICommandBuilder<IBonusDevice, bonusStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<bonus, setBonusState> command)
        {
            return await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, setBonusState> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            device.NotifyActive();

            var enabled = command.Command.enable;

            if (device.HostEnabled != enabled)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = enabled;
            }

            var response = command.GenerateResponse<bonusStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}