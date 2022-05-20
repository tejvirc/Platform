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

    public class GameHistoryReportHandler : IGameHistoryReportHandler, IDisposable
    {
        private const int RetryDelay = 100;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICentralProvider _centralProvider;
        private readonly IBingoClientConnectionState _clientConnectionState;
        private readonly IAcknowledgedQueue<ReportGameOutcomeMessage, long> _queue;
        private readonly IGameOutcomeService _gameOutcomeService;

        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        public GameHistoryReportHandler(
            ICentralProvider centralProvider,
            IBingoClientConnectionState clientConnectionState,
            IAcknowledgedQueue<ReportGameOutcomeMessage, long> queue,
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

        public void AddReportToQueue(ReportGameOutcomeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _queue.Enqueue(message);
        }

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

                ReportGameOutcomeResponse ack = null;
                try
                {
                    ack = await _gameOutcomeService.ReportGameOutcome(item, token);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    Logger.Error($"The game history message failed: {item}", e);
                }

                if (ack is { ResponseCode: ResponseCode.Ok })
                {
                    _centralProvider.AcknowledgeOutcome(item.TransactionId);
                    _queue.Acknowledge(item.TransactionId);
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