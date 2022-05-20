namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Reflection;
    using log4net;
    using Messages;

    /// <summary>
    ///     Implementation for the IProgressiveBroadcastService interface
    /// </summary>
    public class ProgressiveBroadcastService : IProgressiveBroadcastService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Subject<GameProgressiveUpdate> _incomingProgressiveUpdates =
            new Subject<GameProgressiveUpdate>();

        private readonly List<IDisposable> _subscribersList = new List<IDisposable>();

        private readonly IMessageFactory _messageFactory;
        private bool _disposed;

        /// <summary>
        ///     Constructor that takes all the various bits and pieces that we use.
        /// </summary>
        public ProgressiveBroadcastService(
            IUdpConnection udpConnection,
            IMessageFactory messageFactory)
        {
            var localUdpConnection = udpConnection ??
                                     throw new ArgumentNullException(nameof(udpConnection));

            _subscribersList.Add(localUdpConnection.IncomingBytes.Subscribe(
                OnProgressiveUpdateReceived,
                error => Logger.Error($"Error occurred while trying to receive message - {error}.")));

            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IObservable<GameProgressiveUpdate> ProgressiveUpdates => _incomingProgressiveUpdates;

        /// <summary>
        ///     For cleaning up resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var disposable in _subscribersList)
                {
                    disposable.Dispose();
                }

                _incomingProgressiveUpdates.Dispose();
            }

            _disposed = true;
        }

        private void OnProgressiveUpdateReceived(Packet packet)
        {
            try
            {
                _incomingProgressiveUpdates.OnNext((GameProgressiveUpdate)_messageFactory.Deserialize(packet.Data));
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to DeSerialize data.", ex);
            }
        }
    }
}