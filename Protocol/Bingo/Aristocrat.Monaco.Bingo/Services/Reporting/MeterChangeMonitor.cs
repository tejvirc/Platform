namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Meters;
    using TransactionType = Common.TransactionType;

    /// <summary>
    ///     This class monitors several meters and when they change it
    ///     sends a Transaction Report to the bingo server
    /// </summary>
    public class MeterChangeMonitor : IDisposable
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly ICentralProvider _centralProvider;
        private bool _disposed;

        private readonly IMeter _cashPlayed;
        private readonly IMeter _cashWon;
        private readonly IMeter _gamesPlayed;
        private readonly IMeter _gamesWon;

        public MeterChangeMonitor(
            IGameMeterManager gamingMeterManager,
            IReportTransactionQueueService bingoTransactionReportHandler,
            ICentralProvider centralProvider)
        {
            if (gamingMeterManager is null)
            {
                throw new ArgumentNullException(nameof(gamingMeterManager));
            }

            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));

            _cashWon = gamingMeterManager.GetMeter(GamingMeters.TotalEgmPaidGameWonAmount);
            _cashPlayed = gamingMeterManager.GetMeter(GamingMeters.WageredAmount);
            _gamesPlayed = gamingMeterManager.GetMeter(GamingMeters.PlayedCount);
            _gamesWon = gamingMeterManager.GetMeter(GamingMeters.WonCount);

            _cashPlayed.MeterChangedEvent += OnCashPlayedChanged;
            _cashWon.MeterChangedEvent += OnCashWonChanged;
            _gamesPlayed.MeterChangedEvent += OnGamesPlayedChanged;
            _gamesWon.MeterChangedEvent += OnGamesWonChanged;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cashPlayed.MeterChangedEvent -= OnCashPlayedChanged;
                _cashWon.MeterChangedEvent -= OnCashWonChanged;
                _gamesPlayed.MeterChangedEvent -= OnGamesPlayedChanged;
                _gamesWon.MeterChangedEvent -= OnGamesWonChanged;
            }

            _disposed = true;
        }

        private void SendTransactionReport(long amount, TransactionType transaction)
        {
            if (amount == 0 ||
             _centralProvider.Transactions.OrderBy(t => t.TransactionId).LastOrDefault()?.Descriptions.FirstOrDefault() is not BingoGameDescription lastBingoDescription)
            {
                return;
            }

            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                transaction,
                amount,
                lastBingoDescription.GameTitleId,
                lastBingoDescription.DenominationId,
                lastBingoDescription.GameSerial,
                int.TryParse(lastBingoDescription.Paytable, out var paytable) ? paytable : 0);
        }

        private void OnGamesWonChanged(object sender, MeterChangedEventArgs e)
        {
            SendTransactionReport(_gamesWon.Lifetime, TransactionType.GamesWon);
        }

        private void OnGamesPlayedChanged(object sender, MeterChangedEventArgs e)
        {
            SendTransactionReport(_gamesPlayed.Lifetime, TransactionType.GamesPlayed);
        }

        private void OnCashWonChanged(object sender, MeterChangedEventArgs e)
        {
            SendTransactionReport(e.Amount.MillicentsToCents(), TransactionType.CashWon);
        }

        private void OnCashPlayedChanged(object sender, MeterChangedEventArgs e)
        {
            SendTransactionReport(e.Amount.MillicentsToCents(), TransactionType.CashPlayed);
        }
    }
}