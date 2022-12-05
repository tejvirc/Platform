namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using Application.Contracts;
    using Common;
    using Common.Storage;
    using Extensions;
    using Services.Reporting;

    /// <summary>
    ///     A base meter monitor used for reporting transaction information to the server that occur during a game round
    /// </summary>
    public abstract class BaseMeterMonitor : IMeterMonitor, IDisposable
    {
        private readonly IBingoGameProvider _bingoGameProvider;
        private readonly TransactionType _transactionType;
        private readonly IReportTransactionQueueService _transactionQueue;
        private bool _disposed;

        /// <summary>
        ///     Creates an instance of <see cref="BaseMeterMonitor"/>
        /// </summary>
        /// <param name="meterId">The meterId to register for changes that occur</param>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionType">The transaction type that will be reported for this meter monitor</param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref cref="meterManager"/>, <paramref cref="bingoGameProvider"/>, or <paramref cref="transactionQueue"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="meterId"/> is an empty string or null</exception>
        protected BaseMeterMonitor(
            string meterId,
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            TransactionType transactionType,
            IReportTransactionQueueService transactionQueue)
        {
            if (meterManager == null)
            {
                throw new ArgumentNullException(nameof(meterManager));
            }

            if (string.IsNullOrEmpty(meterId))
            {
                throw new ArgumentException(@"Value cannot be null or empty.", nameof(meterId));
            }

            _bingoGameProvider = bingoGameProvider ?? throw new ArgumentNullException(nameof(bingoGameProvider));
            _transactionType = transactionType;
            _transactionQueue = transactionQueue ?? throw new ArgumentNullException(nameof(transactionQueue));
            Meter = meterManager.GetMeter(meterId);
            Meter.MeterChangedEvent += MeterOnMeterChangedEvent;
        }

        /// <summary>
        ///     Finalize used to clean up resources when dispose isn't called
        /// </summary>
        ~BaseMeterMonitor() => Dispose(false);

        /// <summary>
        ///     Gets the <see cref="IMeter"/> that was registered for changes
        /// </summary>
        protected IMeter Meter { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Used for cleaning up resources
        /// </summary>
        /// <param name="disposing">Whether or not this was called from the <seealso cref="Dispose"/> method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Meter.MeterChangedEvent -= MeterOnMeterChangedEvent;
            }

            _disposed = true;
        }

        /// <summary>
        ///     This will report the transaction information to the server for the provided meter changed information
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        protected virtual void ReportTransaction(MeterChangedEventArgs changedEventArgs)
        {
            var bingoGameDescription = _bingoGameProvider.GetBingoGame();
            if (bingoGameDescription is null || changedEventArgs.Amount <= 0)
            {
                return;
            }

            var amount = GetAmount(changedEventArgs, bingoGameDescription);
            var gameTitleId = GetTitleId(changedEventArgs, bingoGameDescription);
            var denominationId = GetDenominationId(changedEventArgs, bingoGameDescription);
            var gameSerial = GetGameSerial(changedEventArgs, bingoGameDescription);
            var paytableId = GetPaytableId(changedEventArgs, bingoGameDescription);
            var barcode = GetBarcode(changedEventArgs, bingoGameDescription);
            _transactionQueue.AddNewTransactionToQueue(
                _transactionType,
                amount,
                gameTitleId,
                denominationId,
                gameSerial,
                paytableId,
                barcode);
        }

        /// <summary>
        ///     Gets the title id to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The title id for this transaction</returns>
        protected virtual uint GetTitleId(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => gameDescription.GameTitleId;

        /// <summary>
        ///     Gets the denomination id to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The denomination id for this transaction</returns>
        protected virtual int GetDenominationId(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => gameDescription.DenominationId;

        /// <summary>
        ///     Gets the game serial to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The game serial for this transaction</returns>
        protected virtual long GetGameSerial(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => gameDescription.GameSerial;

        /// <summary>
        ///     Gets the paytable id to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The paytable id for this transaction</returns>
        protected virtual int GetPaytableId(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => gameDescription.GetPaytableID();

        /// <summary>
        ///     Gets the barcode to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The barcode for this transaction</returns>
        protected virtual string GetBarcode(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => string.Empty;

        /// <summary>
        ///     Gets the amount to use for the transaction reporting
        /// </summary>
        /// <param name="changedEventArgs">The <see cref="MeterChangedEventArgs"/> that was raised</param>
        /// <param name="gameDescription">The <see cref="BingoGameDescription"/> that is for this event</param>
        /// <returns>The amount for this transaction</returns>
        protected abstract long GetAmount(MeterChangedEventArgs changedEventArgs, BingoGameDescription gameDescription);

        private void MeterOnMeterChangedEvent(object sender, MeterChangedEventArgs changedEventArgs) =>
            ReportTransaction(changedEventArgs);
    }
}