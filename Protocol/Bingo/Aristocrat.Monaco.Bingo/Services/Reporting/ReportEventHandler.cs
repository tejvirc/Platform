﻿namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Kernel;
    using Monaco.Common;

    /// <summary>
    ///     This class handles getting notifications of events from consumers
    ///     that require event reporting to the bingo server.
    ///     It handles queuing the events and sending events that
    ///     are queued.
    ///     It will resend failed events after a short delay.
    /// </summary>
    public sealed class ReportEventHandler : IReportEventQueueService, IDisposable
    {
        private const int RetryDelay = 500;
        private readonly IAcknowledgedQueue<ReportEventMessage, long> _queue;
        private readonly IPropertiesManager _properties;
        private readonly IBingoClientConnectionState _connectionState;
        private readonly IReportEventService _reportEventService;
        private readonly IIdProvider _idProvider;
        private bool _disposed;
        private CancellationTokenSource _tokenSource;

        public ReportEventHandler(
            IPropertiesManager properties,
            IBingoClientConnectionState connectionState,
            IAcknowledgedQueue<ReportEventMessage, long> queue,
            IReportEventService reportEventService,
            IIdProvider idProvider)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _reportEventService = reportEventService ?? throw new ArgumentNullException(nameof(reportEventService));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            _connectionState.ClientConnected += HandleConnected;
            _connectionState.ClientDisconnected += HandleDisconnected;
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

        public void AddNewEventToQueue(ReportableEvent eventType)
        {
            var message = new ReportEventMessage(
                string.Empty,
                DateTime.UtcNow,
                _idProvider.GetNextLogSequence<ReportEventHandler>(),
                (int)eventType);

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
            _tokenSource = new CancellationTokenSource();
            ConsumeQueuedEvents(_tokenSource.Token).FireAndForget();
        }

        private async Task ConsumeQueuedEvents(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this will block until an item is available or the token is cancelled
                var item = await _queue.GetNextItem(token).ConfigureAwait(false);
                if (item is null)
                {
                    continue;
                }

                // add the current machine serial to the report
                item.MachineSerial = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);

                ReportEventResponse ack = null;
                try
                {
                    ack = await _reportEventService.ReportEvent(item, token).ConfigureAwait(false);
                }
                catch (Exception e) when(e is not OperationCanceledException)
                {
                }

                if (ack is { ResponseCode: ResponseCode.Ok })
                {
                    _queue.Acknowledge(ack.EventId);
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