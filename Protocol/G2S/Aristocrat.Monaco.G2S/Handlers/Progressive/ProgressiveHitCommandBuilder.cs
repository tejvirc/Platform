﻿namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;

    /// <summary>
    ///     A progressive hit command builder.
    /// </summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.G2S.Handlers.ICommandBuilder{Aristocrat.G2S.Client.Devices.IProgressiveDevice, Aristocrat.G2S.Protocol.v21.progressiveHit}" />
    public class ProgressiveHitCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveHit>
    {
        private readonly IGameProvider _gameProvider;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveDeviceManager _progressiveDeviceManager;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.G2S.Handlers.Progressive.ProgressiveHitCommandBuilder class.
        /// </summary>
        public ProgressiveHitCommandBuilder(
            IGameProvider gameProvider,
            ITransactionHistory transactionHistory,
            IProgressiveLevelProvider progressiveProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IProgressiveDeviceManager progressiveDeviceManager)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveDeviceManager = progressiveDeviceManager ?? throw new ArgumentNullException(nameof(progressiveDeviceManager));
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

            if (transaction.ValueSequence == 1L) // If unset, set this to be used for the corresponding progressiveCommit and Vertex recovery
            {
                var level = _progressiveProvider.GetProgressiveLevels().First(l => l.ProgressiveId == device.ProgressiveId && l.LevelId == transaction.LevelId && (_progressiveDeviceManager.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id);
                if (level != null)
                {
                    transaction.ValueSequence = level.ProgressiveValueSequence;
                    _transactionHistory.UpdateTransaction(transaction);
                }
            }

            IViewableLinkedProgressiveLevel linkedLevel = null;

            if (!string.IsNullOrEmpty(transaction.AssignedProgressiveKey))
            {
                _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(transaction.AssignedProgressiveKey, out linkedLevel);
            }

            var levelId = linkedLevel?.ProtocolLevelId ?? transaction.LevelId;

            command.denomId = transaction.DenomId;
            command.gamePlayId = transaction.GameId;
            command.hitDateTime = transaction.TransactionDateTime;
            command.paytableId = _gameProvider.GetGame(transaction.GameId)?.PaytableId;
            command.progId = transaction.ProgressiveId;
            command.progValueAmt = transaction.ValueAmount;
            command.progValueSeq = transaction.ValueSequence;
            command.themeId = _gameProvider.GetGame(transaction.GameId)?.ThemeId;
            command.levelId = levelId;
            command.progValueText = transaction.ValueText;
            command.winLevelCombo = string.Empty; //ToDo depends on WinLevel
            command.winLevelIndex = transaction.WinLevelIndex;

            return Task.CompletedTask;
        }
    }
}