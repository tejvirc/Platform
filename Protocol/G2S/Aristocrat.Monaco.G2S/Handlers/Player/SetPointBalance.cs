namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    [ProhibitWhenDisabled]
    public class SetPointBalance : ICommandHandler<player, setPointBalance>
    {
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPlayerService _players;

        public SetPointBalance(
            IG2SEgm egm,
            IPlayerService players,
            ICommandBuilder<IPlayerDevice, playerStatus> command,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public async Task<Error> Verify(ClassCommand<player, setPointBalance> command)
        {
            var error = await Sanction.OnlyOwner<IPlayerDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            return _players.ActiveSession?.Log.TransactionId != command.Command.transactionId
                ? new Error(ErrorCode.G2S_PRX002)
                : null;
        }

        public async Task Handle(ClassCommand<player, setPointBalance> command)
        {
            var device = _egm.GetDevice<IPlayerDevice>(command.IClass.deviceId);

            _players.SetSessionParameters(
                command.Command.transactionId,
                command.Command.pointBalance,
                command.Command.overrideId);

            var response = command.GenerateResponse<playerStatus>();
            await _command.Build(device, response.Command);

            var status = new playerStatus();
            await _command.Build(device, status);

            _eventLift.Report(device, EventCode.G2S_PRE112, device.DeviceList(status));
        }
    }
}
