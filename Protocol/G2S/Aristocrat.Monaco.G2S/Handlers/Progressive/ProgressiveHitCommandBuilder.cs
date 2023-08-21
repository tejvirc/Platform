namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     A progressive hit command builder.
    /// </summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.G2S.Handlers.ICommandBuilder{Aristocrat.G2S.Client.Devices.IProgressiveDevice, Aristocrat.G2S.Protocol.v21.progressiveHit}" />
    public class ProgressiveHitCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveHit>
    {
        private readonly IGameProvider _gameProvider;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.G2S.Handlers.Progressive.ProgressiveHitCommandBuilder class.
        /// </summary>
        public ProgressiveHitCommandBuilder(
            IGameProvider gameProvider,
            ITransactionHistory transactionHistory)
        {
            _gameProvider = gameProvider;
            _transactionHistory = transactionHistory;
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveHit command)
        {
            var transaction =
                _transactionHistory.RecallTransaction<JackpotTransaction>(command.transactionId);
            if (transaction == null)
            {
                return Task.CompletedTask;
            }

            command.denomId = transaction.DenomId;
            command.gamePlayId = transaction.GameId;
            command.hitDateTime = transaction.TransactionDateTime;
            command.paytableId = _gameProvider.GetGame(transaction.GameId)?.PaytableId;
            command.progId = transaction.ProgressiveId;
            command.progValueAmt = transaction.ValueAmount;
            command.progValueSeq = transaction.ValueSequence;
            command.themeId = _gameProvider.GetGame(transaction.GameId)?.ThemeId;
            command.levelId = transaction.LevelId;
            command.progValueText = transaction.ValueText;
            command.winLevelCombo = string.Empty; //ToDo depends on WinLevel
            command.winLevelIndex = transaction.WinLevelIndex;

            return Task.CompletedTask;
        }
    }
}