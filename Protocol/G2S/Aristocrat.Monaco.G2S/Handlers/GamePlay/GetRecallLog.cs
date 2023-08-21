namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    /// <summary>
    ///     An implementation of <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetRecallLog : ICommandHandler<gamePlay, getRecallLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetRecallLog" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        public GetRecallLog(IG2SEgm egm, IGameHistory gameHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gamePlay, getRecallLog> command)
        {
            return await Sanction.OwnerAndGuests<IGamePlayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gamePlay, getRecallLog> command)
        {
            var recallLog = command.GenerateResponse<recallLogList>();

            // TODO: WinLevel items for progressives
            recallLog.Command.recallLog = _gameHistory
                .GetGameHistory()
                .ToList()
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(h => h.ToRecallLog())
                .ToArray();

            await Task.CompletedTask;
        }
    }
}