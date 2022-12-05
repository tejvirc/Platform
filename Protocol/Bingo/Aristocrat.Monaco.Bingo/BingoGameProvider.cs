namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Linq;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

    /// <summary>
    ///     A provider for getting the current bingo game
    /// </summary>
    public sealed class BingoGameProvider : IBingoGameProvider
    {
        private readonly ICentralProvider _centralProvider;
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Creates an instance of <see cref="BingoGameProvider"/>
        /// </summary>
        /// <param name="centralProvider">An instance of <see cref="ICentralProvider"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="centralProvider"/> or <paramref name="gameHistory"/></exception>
        public BingoGameProvider(ICentralProvider centralProvider, IGameHistory gameHistory)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public BingoGameDescription GetBingoGame()
        {
            var currentLog = _gameHistory.CurrentLog;
            if (currentLog is null)
            {
                return null;
            }

            return _centralProvider.Transactions
                .FirstOrDefault(t => t.AssociatedTransactions.Contains(currentLog.TransactionId))?.Descriptions
                .FirstOrDefault() as BingoGameDescription;
        }
    }
}