namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class ThreadSafeHashSet<T> : IDisposable, IEnumerable<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public IEnumerator<T> GetEnumerator()
        {
            HashSet<T> snapShot;

            _lock.EnterReadLock();
            try
            {
                snapShot = new HashSet<T>(_hashSet);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return snapShot.GetEnumerator();
        }

        public bool TryAdd(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryRemove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hashSet.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.FirstOrDefault(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lock is not null)
                {
                    _lock.Dispose();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
