namespace Aristocrat.Mgam.Client.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Stores a list of <see cref="IDisposable"/> objects to dispose.
    /// </summary>
    public class SubscriptionList : IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _disposed;

        /// <summary>
        ///     Adds a subscription.
        /// </summary>
        /// <param name="subscriptions">Adds a list of <see cref="IDisposable"/> to unsubscribe.</param>
        public void Add(params IDisposable[] subscriptions)
        {
            _subscriptions.AddRange(subscriptions);
        }

        /// <inheritdoc />
        ~SubscriptionList()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///  <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                UnsubscribeAll();
            }

            _disposed = true;
        }

        private void UnsubscribeAll()
        {
            foreach (var subscription in _subscriptions.ToArray())
            {
                subscription.Dispose();
                _subscriptions.Remove(subscription);
            }
        }
    }
}
