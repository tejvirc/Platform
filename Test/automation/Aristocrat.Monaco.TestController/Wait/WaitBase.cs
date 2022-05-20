namespace Aristocrat.Monaco.TestController.Wait
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataModel;

    public class WaitBase : IWaitStrategy, IDisposable
    {
        protected readonly ManualResetEvent _waitReset = new ManualResetEvent(false);

        /// <summary>
        ///     timeout in seconds
        /// </summary>
        protected readonly int _timeout;

        protected readonly ConcurrentDictionary<WaitEventEnum, WaitStatus> _waitStatuses =
            new ConcurrentDictionary<WaitEventEnum, WaitStatus>();

        protected bool _disposed;

        public WaitBase(IEnumerable<WaitEventEnum> waits, int timeout)
        {
            foreach (var wait in waits)
            {
                _waitStatuses.TryAdd(wait, WaitStatus.WaitPending);
            }

            _timeout = timeout;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            _waitReset.Reset();

            Task.Run(() => _waitReset.WaitOne(_timeout * 1000)).ContinueWith(
                timedOut => { WaitTImeOut(timedOut.Result); });
        }

        public void EventPublished(WaitEventEnum wait)
        {
            if (_waitStatuses.ContainsKey(wait))
            {
                if (_waitStatuses[wait] != WaitStatus.WaitTimedOut)
                {
                    _waitStatuses[wait] = WaitStatus.WaitMet;
                }
            }
        }

        public virtual string CheckStatus(Dictionary<WaitEventEnum, WaitStatus> status, out string msg)
        {
            throw new NotImplementedException();
        }

        public void ClearWaits()
        {
            _waitStatuses.Clear();
        }

        public void CancelWait(WaitEventEnum wait)
        {
            _waitStatuses.TryRemove(wait, out _);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _waitReset.Close();
            }

            _disposed = true;
        }

        protected void WaitTImeOut(bool timeOut)
        {
            foreach (var wait in _waitStatuses.Keys)
            {
                if (_waitStatuses[wait] != WaitStatus.WaitMet)
                {
                    _waitStatuses[wait] = WaitStatus.WaitTimedOut;
                }
            }
        }
    }
}