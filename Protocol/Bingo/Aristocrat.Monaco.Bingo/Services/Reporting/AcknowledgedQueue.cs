// ReSharper disable InconsistentlySynchronizedField
namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using Monaco.Common;

    public class AcknowledgedQueue<TQueueItem, TQueueId> : IDisposable, IAcknowledgedQueue<TQueueItem, TQueueId>
        where TQueueItem : class
        where TQueueId : struct
    {
        private const int DefaultQueueSize = 30;
        private const int AlmostFullThreshold = 24; // 80% full
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ConcurrentQueue<TQueueItem> _queue;
        private readonly IAcknowledgedQueueHelper<TQueueItem, TQueueId> _helper;
        private readonly int _queueSize;
        private readonly AutoResetEvent _autoResetEvent = new (false);
        private readonly object _locker = new ();
        private bool _disposed;
        private bool _tilted;

        /// <summary>
        ///     Construct a new instance of the AcknowledgedQueue
        /// </summary>
        /// <param name="helper">
        /// A helper class that provides methods to get the Id information for
        /// the transaction or event and provides methods to load/save persisted data</param>
        public AcknowledgedQueue(IAcknowledgedQueueHelper<TQueueItem, TQueueId> helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _queueSize = DefaultQueueSize;
            LoadPersistedQueue();
            DisableIfQueueAlmostFull();
        }

        public async Task<TQueueItem> GetNextItem(CancellationToken token)
        {
            if (!IsEmpty())
            {
                _autoResetEvent.Reset();
                return _queue.TryPeek(out var result) ? result : default;
            }

            // block until we get something added to the queue
            await _autoResetEvent.AsTask(token);
            return _queue.TryPeek(out var result1) ? result1 : default;
        }

        public void Enqueue(TQueueItem item)
        {
            if (_queue.Count < _queueSize)
            {
                _queue.Enqueue(item);
                PersistQueue();
                _autoResetEvent.Set();
            }
            else
            {
                _logger.Debug("queue is full, dropping an item");
            }

            DisableIfQueueAlmostFull();
        }

        public void Acknowledge(TQueueId id)
        {
            var item = _queue.TryPeek(out var result) ? result : default;
            if (item is null || !Equals(_helper.GetId(item), id))
            {
                return;
            }

            _logger.Debug($"Dequeuing item with id '{id}'");
            _queue.TryDequeue(out _);
            PersistQueue();

            EnableIfQueueBelowThreshold();
        }

        public bool IsEmpty()
        {
            return _queue.IsEmpty;
        }

        public int Count()
        {
            return _queue.Count;
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
                _autoResetEvent.Dispose();
            }

            _disposed = true;
        }

        private void LoadPersistedQueue()
        {
            _queue = new ConcurrentQueue<TQueueItem>(_helper.ReadPersistence());
        }

        private void PersistQueue()
        {
            _helper.WritePersistence(_queue.ToList());
        }

        private void DisableIfQueueAlmostFull()
        {
            lock (_locker)
            {
                if (_queue.Count < AlmostFullThreshold || _tilted)
                {
                    return;
                }

                _helper.AlmostFullDisable();
                _tilted = true;
            }
        }

        private void EnableIfQueueBelowThreshold()
        {
            lock (_locker)
            {
                if (_queue.Count >= AlmostFullThreshold || !_tilted)
                {
                    return;
                }

                _helper.AlmostFullClear();
                _tilted = false;
            }
        }
    }
}