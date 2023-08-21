namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    /// <summary>
    ///     An implementation of <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetRecallLogStatus : ICommandHandler<gamePlay, getRecallLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetRecallLogStatus" /> class.
        /// </summary>
        /// <param name="egm">An instance of an <see cref="IG2SEgm" /></param>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance</param>
        public GetRecallLogStatus(IG2SEgm egm, IGameHistory gameHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gamePlay, getRecallLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IGamePlayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gamePlay, getRecallLogStatus> command)
        {
            var status = command.GenerateResponse<recallLogStatus>();

            status.Command.lastSequence = _gameHistory.LogSequence;
            status.Command.totalEntries = _gameHistory.TotalEntries;

            await Task.CompletedTask;
        }
    }
}