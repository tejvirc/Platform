namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Commands;
    using Common;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Queue for reporting multi-game outcomes to the bingo server
    /// </summary>
    public class GameHistoryReportAcknowledgeQueueHelper : IAcknowledgedQueueHelper<ReportMultiGameOutcomeMessage, long>
    {
        private readonly ICentralProvider _centralProvider;
        private readonly IGameHistory _gameHistory;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _disableManager;

        public GameHistoryReportAcknowledgeQueueHelper(
            ICentralProvider centralProvider,
            IGameHistory gameHistory,
            IPropertiesManager propertiesManager,
            ISystemDisableManager disableManager)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc/>
        public long GetId(ReportMultiGameOutcomeMessage item)
        {
            return item.TransactionId;
        }

        /// <inheritdoc/>
        public void WritePersistence(List<ReportMultiGameOutcomeMessage> list)
        {
            // Do nothing the persistence is the game history log and central transactions
        }

        /// <inheritdoc/>
        public List<ReportMultiGameOutcomeMessage> ReadPersistence()
        {
            var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            var gameHistoryLogs = _gameHistory.GetGameHistory().ToList();
            var currentGame = _gameHistory.CurrentLog;
            return _centralProvider.Transactions.Where(x => x is not { OutcomeState: OutcomeState.Acknowledged })
                .OrderBy(x => x.TransactionId)
                .Select(x => (Transaction: x, Log: GetGameHistoryLog(x)))
                .Where(x => x.Transaction is not null && x.Log is not null)
                .Select(x => x.Transaction.ToReportGameOutcomeMessage(serialNumber, x.Log))
                .Where(x => x is not null).ToList();

            IGameHistoryLog GetGameHistoryLog(ITransactionConnector transaction) =>
                !_gameHistory.IsRecoveryNeeded || currentGame is null ||
                !transaction.AssociatedTransactions.Contains(currentGame.TransactionId)
                    ? gameHistoryLogs.FirstOrDefault(g => transaction.AssociatedTransactions.Contains(g.TransactionId))
                    : null;
        }

        /// <inheritdoc/>
        public void AlmostFullDisable()
        {
            _disableManager.Disable(
                BingoConstants.GameHistoryQueueDisableKey,
                SystemDisablePriority.Normal,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryReportingAlmostFull),
                false,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryReportingAlmostFullHelp));
        }

        /// <inheritdoc/>
        public void AlmostFullClear()
        {
            _disableManager.Enable(BingoConstants.GameHistoryQueueDisableKey);
        }
    }
}