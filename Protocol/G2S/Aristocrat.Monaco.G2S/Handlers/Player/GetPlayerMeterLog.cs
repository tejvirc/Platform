namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Session;

    public class GetPlayerMeterLog : ICommandHandler<player, getPlayerMeterLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameMeterManager _meters;
        private readonly IPlayerSessionHistory _playerSessionHistory;

        public GetPlayerMeterLog(IG2SEgm egm, IPlayerSessionHistory playerSessionHistory, IGameMeterManager meters)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _playerSessionHistory =
                playerSessionHistory ?? throw new ArgumentNullException(nameof(playerSessionHistory));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerMeterLog> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerMeterLog> command)
        {
            var response = command.GenerateResponse<playerMeterLogList>();

            response.Command.playerMeterLog = new playerMeterLog[0];

            var device = _egm.GetDevice<IPlayerDevice>();

            var subscription = device.SubscribedMeters.Expand(_egm.Devices);

            response.Command.playerMeterLog = _playerSessionHistory.GetHistory()
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(
                    h => h.ToPlayerMeterLog(
                        device,
                        subscription,
                        (id, meterName) => _meters.GetMeterName(id, meterName))).ToArray();

            await Task.CompletedTask;
        }
    }
}
