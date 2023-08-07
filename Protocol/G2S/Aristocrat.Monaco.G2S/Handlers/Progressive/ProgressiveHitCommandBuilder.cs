namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
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
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.G2S.Handlers.Progressive.ProgressiveHitCommandBuilder class.
        /// </summary>
        public ProgressiveHitCommandBuilder(
            IGameProvider gameProvider,
            ITransactionHistory transactionHistory,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveHit command)
        {
            var transaction =
                _transactionHistory.RecallTransaction<JackpotTransaction>(command.transactionId);
            if (transaction == null || string.IsNullOrEmpty(transaction.AssignedProgressiveKey))
            {
                return Task.CompletedTask;
            }

            _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(transaction.AssignedProgressiveKey, out var linkedLevel);
            
            if (transaction.ValueSequence == 1L) // If unset, set this to be used for the corresponding progressiveCommit and Vertex recovery
            {
                transaction.ValueSequence = linkedLevel.ProgressiveValueSequence;
                _transactionHistory.UpdateTransaction(transaction);
            }

            command.denomId = transaction.DenomId;
            command.gamePlayId = transaction.GameId;
            command.hitDateTime = transaction.TransactionDateTime;
            command.paytableId = _gameProvider.GetGame(transaction.GameId)?.PaytableId;
            command.progValueAmt = transaction.ValueAmount;
            command.progValueText = transaction.ValueText;
            command.progValueSeq = transaction.ValueSequence;
            command.themeId = _gameProvider.GetGame(transaction.GameId)?.ThemeId;
            command.progId = linkedLevel.ProgressiveGroupId;
            command.levelId = linkedLevel.LevelId;
            command.winLevelCombo = string.Empty; //ToDo depends on WinLevel
            command.winLevelIndex = transaction.WinLevelIndex;

            return Task.CompletedTask;
        }
    }
}