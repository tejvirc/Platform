namespace Aristocrat.Monaco.Application.Plugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Stores a list of <see cref="IDisposable"/> objects to dispose.
    /// </summary>
    public sealed class Disposables : IDisposable, IReadOnlyCollection<IDisposable>
    {
        private readonly List<IDisposable> _disposables = new();

        private bool _disposed;

        /// <summary>
        ///     Adds a subscription.
        /// </summary>
        /// <param name="subscriptions">Adds a list of <see cref="IDisposable"/> to unsubscribe.</param>
        public Disposables Add(params IDisposable[] subscriptions)
        {
            _disposables.AddRange(subscriptions);
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            UnsubscribeAll();

            _disposed = true;
        }

        public static Disposables operator +(Disposables disposables, IDisposable disposable) =>
            disposables.Add(disposable);

        public int Count => _disposables.Count;

        public IEnumerator<IDisposable> GetEnumerator() => _disposables.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void UnsubscribeAll()
        {
            foreach (var subscription in _disposables.ToArray())
            {
                subscription.Dispose();
                _disposables.Remove(subscription);
            }
        }
    }
}
