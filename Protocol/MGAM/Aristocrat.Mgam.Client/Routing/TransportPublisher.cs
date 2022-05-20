namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Logging;
    using Messaging;

    /// <summary>
    ///     Reports transport status events.
    /// </summary>
    internal sealed class TransportPublisher : ITransportPublisher
    {
        private readonly ILogger<TransportPublisher> _logger;

        private readonly IObservable<EventPattern<TransportStatusEventArgs>> _statuses;
        private readonly IObservable<EventPattern<RoutedMessageEventArgs>> _messages;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportPublisher" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}" />.</param>
        public TransportPublisher(ILogger<TransportPublisher> logger)
        {
            _logger = logger;

            _statuses = Observable.FromEventPattern<TransportStatusEventArgs>(
                h => TransportStatusChanged += h,
                h => TransportStatusChanged -= h);

            _messages = Observable.FromEventPattern<RoutedMessageEventArgs>(
                h => RoutedMessageTransmitted += h,
                h => RoutedMessageTransmitted -= h);
        }

        private event EventHandler<TransportStatusEventArgs> TransportStatusChanged;

        private event EventHandler<RoutedMessageEventArgs> RoutedMessageTransmitted;

        /// <inheritdoc />
        public void Publish(TransportStatus status)
        {
            _logger.LogInfo(
                $"Publishing transport status update [TransportState={status.TransportState},Broadcast={status.IsBroadcast},Address={status.EndPoint},ConnectionState={status.ConnectionState},Failure={status.Failure}]");

            RaiseTransportStatusChanged(status);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<TransportStatus> observer)
        {
            return _statuses.Select(e => e.EventArgs.Status).ObserveOn(Scheduler.Default).Subscribe(observer);
        }

        /// <inheritdoc />
        public void Publish(string source, string destination, IMessage message, string xml)
        {
            RaiseRoutedMessageTransmitted(
                new RoutedMessage
                {
                    Timestamp = DateTime.UtcNow,
                    Source = source,
                    Destination = destination,
                    Name = message.GetType().Name,
                    IsRequest = message is IRequest,
                    IsResponse = message is IResponse,
                    IsCommand = message is ICommand || message is CommandResponse,
                    IsNotification = message is INotification || message is NotificationResponse,
                    IsHeartbeat = message is KeepAlive || message is KeepAliveResponse,
                    ResponseCode = message is IResponse response ? (int?)response.ResponseCode : null,
                    RawData = xml
                });
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<RoutedMessage> observer)
        {
            return _messages.Select(e => e.EventArgs.Message).ObserveOn(Scheduler.Default).Subscribe(observer);
        }

        private void RaiseTransportStatusChanged(TransportStatus status)
        {
            TransportStatusChanged?.Invoke(this, new TransportStatusEventArgs(status));
        }

        private void RaiseRoutedMessageTransmitted(RoutedMessage message)
        {
            RoutedMessageTransmitted?.Invoke(this, new RoutedMessageEventArgs(message));
        }
    }
}
