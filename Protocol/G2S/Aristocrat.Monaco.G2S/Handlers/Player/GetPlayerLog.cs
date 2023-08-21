namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    public class GetPlayerLog : ICommandHandler<player, getPlayerLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IPlayerSessionHistory _playerSessionHistory;

        public GetPlayerLog(IG2SEgm egm, IPlayerSessionHistory playerSessionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _playerSessionHistory = playerSessionHistory ?? throw new ArgumentNullException(nameof(playerSessionHistory));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerLog> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerLog> command)
        {
            var response = command.GenerateResponse<playerLogList>();

            var playerDevice = _egm.GetDevice<IPlayerDevice>();

            response.Command.playerLog = _playerSessionHistory.GetHistory()
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(h => h.ToPlayerLog(playerDevice)).ToArray();

            await Task.CompletedTask;
        }
    }
}
