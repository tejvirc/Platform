namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Kernel;
    using Monaco.Common;
    using ServerApiGateway;
    using TransactionType = Common.TransactionType;

    /// <summary>
    ///     This class handles getting notifications of events from consumers
    ///     that require transaction reporting to the bingo server.
    ///     It handles queuing the transactions and sending transactions that
    ///     are queued.
    ///     It will resend failed transactions after a short delay.
    /// </summary>
    public class TransactionHandler : IReportTransactionQueueService, IDisposable
    {
        private const int RetryDelay = 500;
        private readonly IAcknowledgedQueue<ReportTransactionMessage, int> _queue;
        private readonly IPropertiesManager _properties;
        private readonly IBingoClientConnectionState _connectionState;
        private readonly IReportTransactionService _reportTransactionService;
        private readonly IIdProvider _idProvider;
        private bool _disposed;
        private CancellationTokenSource _tokenSource;

        public TransactionHandler(
            IPropertiesManager properties,
            IBingoClientConnectionState connectionState,
            IAcknowledgedQueue<ReportTransactionMessage, int> queue,
            IReportTransactionService reportTransactionService,
            IIdProvider idProvider)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _reportTransactionService = reportTransactionService ?? throw new ArgumentNullException(nameof(reportTransactionService));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            _connectionState.ClientConnected += HandleConnected;
            _connectionState.ClientDisconnected += HandleDisconnected;
        }

        public void AddNewTransactionToQueue(
            TransactionType transactionType,
            long amount,
            long gameSerial = 0,
            uint gameTitleId = 0,
            int paytableId = 0,
            int denominationId = 0)
        {
            var message = new ReportTransactionMessage
            (
                _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                DateTime.UtcNow,
                amount,
                gameSerial,
                gameTitleId,
                (int)_idProvider.GetNextLogSequence<TransactionHandler>(),
                paytableId,
                denominationId,
                (int)transactionType);

            _queue.Enqueue(message);
        }

        private void HandleDisconnected(object sender, EventArgs args)
        {
            _tokenSource?.Cancel();
        }

        private void HandleConnected(object sender, EventArgs args)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new();
            ConsumeQueuedTransactions(_tokenSource.Token).FireAndForget();
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
                _connectionState.ClientConnected -= HandleConnected;
                _connectionState.ClientDisconnected -= HandleDisconnected;
                if (_tokenSource is not null)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                    _tokenSource = null;
                }
            }

            _disposed = true;
        }

        private async Task ConsumeQueuedTransactions(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this will block until an item is available or the token is cancelled
                var item = await _queue.GetNextItem(token);
                if (item is null)
                {
                    continue;
                }

                ReportTransactionAck ack = null;
                try
                {
                    ack = await _reportTransactionService.ReportTransaction(item, token);
                }
                catch (Exception e) when(e is not OperationCanceledException)
                {
                }

                if (ack is not null && ack.Succeeded)
                {
                    _queue.Acknowledge(ack.TransactionId);
                }
                else
                {
                    // delay 1/2 second before retrying failed reports
                    await Task.Delay(TimeSpan.FromMilliseconds(RetryDelay), token);
                }
            }
        }
    }
}