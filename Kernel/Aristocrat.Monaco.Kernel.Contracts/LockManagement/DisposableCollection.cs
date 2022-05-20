namespace Aristocrat.Monaco.Kernel.Contracts.LockManagement
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A collection that calls Dispose on each member
    /// </summary>
    /// <typeparam name="T">Should implement IDisposable</typeparam>
    public class DisposableCollection<T> : List<T>, IDisposable where T : IDisposable
    {
        private bool _disposed;

        /// <summary>
        ///     Dispose the collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Disposing collection if not already disposed.
        /// </summary>
        /// <param name="disposing">If disposing the collection</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }

                Clear();
            }

            _disposed = true;
        }
    }
}