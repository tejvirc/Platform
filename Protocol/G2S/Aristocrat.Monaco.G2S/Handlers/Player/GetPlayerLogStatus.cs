namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    public class GetPlayerLogStatus : ICommandHandler<player, getPlayerLogStatus>
    {
        private readonly IG2SEgm _egm;

        private readonly IPlayerSessionHistory _playerSessionHistory;

        public GetPlayerLogStatus(IG2SEgm egm, IPlayerSessionHistory playerSessionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _playerSessionHistory = playerSessionHistory ?? throw new ArgumentNullException(nameof(playerSessionHistory));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerLogStatus> command)
        {
            var status = command.GenerateResponse<playerLogStatus>();
 
            status.Command.lastSequence = _playerSessionHistory.LogSequence;
            status.Command.totalEntries = _playerSessionHistory.TotalEntries;

            await Task.CompletedTask;
        }
    }
}
