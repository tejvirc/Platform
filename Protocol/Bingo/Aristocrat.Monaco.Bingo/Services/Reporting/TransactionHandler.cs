namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common.Events;
    using Kernel;
    using Monaco.Common;
    using TransactionType = Common.TransactionType;

    /// <summary>
    ///     This class handles getting notifications of events from consumers
    ///     that require transaction reporting to the bingo server.
    ///     It handles queuing the transactions and sending transactions that
    ///     are queued.
    ///     It will resend failed transactions after a short delay.
    /// </summary>
    public sealed class TransactionHandler : IReportTransactionQueueService, IDisposable
    {
        private const int RetryDelay = 500;
        private readonly IAcknowledgedQueue<ReportTransactionMessage, long> _queue;
        private readonly IPropertiesManager _properties;
        private readonly IBingoClientConnectionState _connectionState;
        private readonly IReportTransactionService _reportTransactionService;
        private readonly IIdProvider _idProvider;
        private readonly IEventBus _eventBus;
        private bool _disposed;
        private CancellationTokenSource _tokenSource;

        public TransactionHandler(
            IPropertiesManager properties,
            IBingoClientConnectionState connectionState,
            IAcknowledgedQueue<ReportTransactionMessage, long> queue,
            IReportTransactionService reportTransactionService,
            IIdProvider idProvider,
            IEventBus eventBus)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _reportTransactionService = reportTransactionService ?? throw new ArgumentNullException(nameof(reportTransactionService));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _connectionState.ClientConnected += HandleConnected;
            _connectionState.ClientDisconnected += HandleDisconnected;
        }

        public bool IsFull => _queue.IsQueueFull;

        public void AddNewTransactionToQueue(
            TransactionType transactionType,
            long amount,
            uint gameTitleId,
            int denominationId,
            long gameSerial,
            int paytableId,
            string barcode)
        {
            var message = new ReportTransactionMessage(
                _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty),
                DateTime.UtcNow,
                amount,
                gameSerial,
                gameTitleId,
                _idProvider.GetNextLogSequence<TransactionHandler>(),
                paytableId,
                denominationId,
                (int)transactionType,
                barcode);

            _queue.Enqueue(message);

            if (IsFull)
            {
                _eventBus.Publish(new QueueFullEvent());
            }
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
            if (_disposed)
            {
                return;
            }

            _connectionState.ClientConnected -= HandleConnected;
            _connectionState.ClientDisconnected -= HandleDisconnected;
            if (_tokenSource is not null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            _disposed = true;
        }

        private async Task ConsumeQueuedTransactions(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this will block until an item is available or the token is cancelled
                var item = await _queue.GetNextItem(token).ConfigureAwait(false);
                if (item is null)
                {
                    continue;
                }

                ReportTransactionResponse ack = null;
                try
                {
                    ack = await _reportTransactionService.ReportTransaction(item, token).ConfigureAwait(false);
                }
                catch (Exception e) when(e is not OperationCanceledException)
                {
                }

                if (ack is { ResponseCode: ResponseCode.Ok })
                {
                    _queue.Acknowledge(ack.TransactionId);
                }
                else
                {
                    // delay 1/2 second before retrying failed reports
                    await Task.Delay(TimeSpan.FromMilliseconds(RetryDelay), token).ConfigureAwait(false);
                }
            }
        }
    }
}