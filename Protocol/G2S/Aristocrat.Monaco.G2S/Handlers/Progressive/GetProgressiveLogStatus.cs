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
    using Gaming.Contracts.Progressives;

    public class GetProgressiveLogStatus : ICommandHandler<progressive, getProgressiveLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetProgressiveLogStatus" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="transactions">An <see cref="ITransactionHistory" /> instance.</param>
        public GetProgressiveLogStatus(IG2SEgm egm, ITransactionHistory transactions)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, getProgressiveLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IProgressiveDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<progressive, getProgressiveLogStatus> command)
        {
            var transactions = _transactions.RecallTransactions<JackpotTransaction>();

            var response = command.GenerateResponse<progressiveLogStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count > 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }
    }
}