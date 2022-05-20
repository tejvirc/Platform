namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Handle the <see cref="setIpState" /> command
    /// </summary>
    public class SetIpState : ICommandHandler<informedPlayer, setIpState>
    {
        private readonly ICommandBuilder<IInformedPlayerDevice, ipStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IInformedPlayerService _ipService;

        /// <summary>
        ///     Constructor for <see cref="SetIpState" />
        /// </summary>
        public SetIpState(
            IG2SEgm egm,
            IInformedPlayerService ipService,
            ICommandBuilder<IInformedPlayerDevice, ipStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _ipService = ipService ?? throw new ArgumentNullException(nameof(ipService));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<informedPlayer, setIpState> command)
        {
            return await Sanction.OnlyOwner<IInformedPlayerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<informedPlayer, setIpState> command)
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>(command.IClass.deviceId);

            device.SessionLimit = command.Command.sessionLimit;

            if (device.HostEnabled != command.Command.enable)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = command.Command.enable;
            }

            _ipService.SetMoneyInState(device, command.Command.enableMoneyIn);
            _ipService.SetGamePlayState(device, command.Command.enableGamePlay, device.DisableText);

            var response = command.GenerateResponse<ipStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
