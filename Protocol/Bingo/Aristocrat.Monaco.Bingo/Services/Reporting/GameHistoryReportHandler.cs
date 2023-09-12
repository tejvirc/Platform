namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Gaming.Contracts.Central;
    using log4net;
    using Monaco.Common;
    using ServerApiGateway;

    /// <summary>
    ///     Reports game history to the bingo server
    /// </summary>
    public class GameHistoryReportHandler : IGameHistoryReportHandler, IDisposable
    {
        private const int RetryDelayMs = 100;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICentralProvider _centralProvider;
        private readonly IBingoClientConnectionState _clientConnectionState;
        private readonly IAcknowledgedQueue<ReportMultiGameOutcomeMessage, long> _queue;
        private readonly IGameOutcomeService _gameOutcomeService;

        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        public GameHistoryReportHandler(
            ICentralProvider centralProvider,
            IBingoClientConnectionState clientConnectionState,
            IAcknowledgedQueue<ReportMultiGameOutcomeMessage, long> queue,
            IGameOutcomeService gameOutcomeService)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _clientConnectionState =
                clientConnectionState ?? throw new ArgumentNullException(nameof(clientConnectionState));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _gameOutcomeService = gameOutcomeService ?? throw new ArgumentNullException(nameof(gameOutcomeService));
            _clientConnectionState.ClientConnected += OnClientConnected;
            _clientConnectionState.ClientDisconnected += OnClientDisconnected;
        }

        /// <inheritdoc/>
        public void AddReportToQueue(ReportMultiGameOutcomeMessage message)
        {
            Logger.Debug($"adding ReportMultiGameOutcomeMessage to queue. queue has {_queue.Count} items");
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _queue.Enqueue(message);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _clientConnectionState.ClientConnected -= OnClientConnected;
                _clientConnectionState.ClientDisconnected -= OnClientDisconnected;
                _tokenSource?.Cancel();
                if (_tokenSource is not null)
                {
                    _tokenSource.Dispose();
                }

                _tokenSource = null;
            }

            _disposed = true;
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            _tokenSource?.Cancel();
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            ConsumeQueuedGameHistoryRecords(_tokenSource.Token)
                .FireAndForget(exception => Logger.Warn("Consuming Game History Records Failed", exception));
        }

        private async Task ConsumeQueuedGameHistoryRecords(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this will block until an item is available or the token is cancelled
                var item = await _queue.GetNextItem(token);
                if (item is null)
                {
                    continue;
                }

                Logger.Debug("************ got a ReportGameHistory item");
                GameOutcomeAck ack = null;
                try
                {
                    ack = await _gameOutcomeService.ReportMultiGameOutcome(item, token);
                    var ackValue = ack==null ? "null" : ack.Succeeded.ToString();
                    Logger.Debug($"after ReportGameOutcome call ack is {ackValue}");
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    Logger.Error($"************ ReportGameHistory The game history message failed: {item}", e);
                }

                var ackValue1 = ack == null ? "null" : ack.Succeeded.ToString();
                Logger.Debug($"************ ReportGameHistory after ReportGameOutcome call ack is {ackValue1}");
                if (ack is not null && ack.Succeeded)
                {
                    Logger.Debug("************ ReportGameHistory Took an item off the game history reporting queue");
                    _centralProvider.AcknowledgeOutcome(item.TransactionId);
                    _queue.Acknowledge(item.TransactionId);
                }
                else
                {
                    Logger.Debug("************ ReportGameHistory ack was null or didn't succeed");
                    // delay before retrying failed reports
                    await Task.Delay(RetryDelayMs, token);
                }
            }
        }
    }
}