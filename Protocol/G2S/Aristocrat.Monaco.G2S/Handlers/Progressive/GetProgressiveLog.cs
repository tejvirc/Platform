namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    public class GetProgressiveLog : ICommandHandler<progressive, getProgressiveLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _gameProvider;
        private readonly ITransactionHistory _transactions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetProgressiveLog" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="transactions">An <see cref="ITransactionHistory" /> instance.</param>
        /// <param name="gameProvider">The game provider.</param>
        public GetProgressiveLog(IG2SEgm egm, ITransactionHistory transactions, IGameProvider gameProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(progressiveLog));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, getProgressiveLog> command)
        {
            return await Sanction.OwnerAndGuests<IProgressiveDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<progressive, getProgressiveLog> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);

            if (device == null)
            {
                return;
            }

            var response = command.GenerateResponse<progressiveLogList>();

            var transactions = _transactions.RecallTransactions<JackpotTransaction>();

            response.Command.progressiveLog = transactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToProgressiveLog(_gameProvider))
                .ToArray();

            await Task.CompletedTask;
        }
    }
}