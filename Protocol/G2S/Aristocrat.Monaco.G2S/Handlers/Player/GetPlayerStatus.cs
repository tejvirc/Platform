namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetPlayerStatus : ICommandHandler<player, getPlayerStatus>
    {
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _command;
        private readonly IG2SEgm _egm;

        public GetPlayerStatus(IG2SEgm egm, ICommandBuilder<IPlayerDevice, playerStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerStatus> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerStatus> command)
        {
            var device = _egm.GetDevice<IPlayerDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<playerStatus>();
            await _command.Build(device, response.Command);
        }
    }
}
