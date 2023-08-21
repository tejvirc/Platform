namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     A progressive commit command builder.
    /// </summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.G2S.Handlers.ICommandBuilder{Aristocrat.G2S.Client.Devices.IProgressiveDevice, Aristocrat.G2S.Protocol.v21.progressiveCommit}" />
    public class ProgressiveCommitCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveCommit>
    {
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.G2S.Handlers.Progressive.ProgressiveCommitCommandBuilder class.
        /// </summary>
        /// <param name="transactionHistory">The transaction history.</param>
        public ProgressiveCommitCommandBuilder(ITransactionHistory transactionHistory)
        {
            _transactionHistory = transactionHistory;
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveCommit command)
        {
            var transaction =
                _transactionHistory.RecallTransaction<JackpotTransaction>(command.transactionId);
            if (transaction == null)
            {
                return Task.CompletedTask;
            }

            command.progId = transaction.ProgressiveId;
            command.levelId = transaction.LevelId;
            command.progWinAmt = transaction.WinAmount;
            command.progWinText = transaction.WinText;
            command.progWinSeq = transaction.WinSequence;
            command.payMethod = (t_progPayMethods)transaction.PayMethod;
            command.progPaidAmt = transaction.PaidAmount;
            command.progException = transaction.Exception;
            command.paidDateTime = transaction.PaidDateTime;

            return Task.CompletedTask;
        }
    }
}