namespace Aristocrat.Sas.Client
{
    using System.Collections.Concurrent;
    using log4net;

    /// <inheritdoc />
    public class SasPriorityMessageQueue : ISasMessageQueue
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConcurrentQueue<ISasMessage> _messages = new ConcurrentQueue<ISasMessage>();
        private readonly ISasMessage _emptyMessage = new SasEmptyMessage();

        private bool _pendingReadMessage;

        /// <inheritdoc />
        public bool IsEmpty => _messages.IsEmpty;

        /// <inheritdoc />
        public void QueueMessage(ISasMessage message)
        {
            _messages.Enqueue(message);
            Logger.Debug($"SAS message queue now contains {_messages.Count} messages");
        }

        /// <inheritdoc />
        public ISasMessage GetNextMessage()
        {
            if (!_messages.TryPeek(out var message))
            {
                return _emptyMessage;
            }

            _pendingReadMessage = true;
            return message;
        }

        /// <inheritdoc />
        public void MessageAcknowledged()
        {
            if (!_pendingReadMessage)
            {
                return;
            }

            _messages.TryDequeue(out _);
            _pendingReadMessage = false;
        }

        /// <inheritdoc />
        public void ClearPendingMessage()
        {
            _pendingReadMessage = false;
        }
    }
}