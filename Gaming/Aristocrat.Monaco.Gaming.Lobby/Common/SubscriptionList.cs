namespace Aristocrat.Monaco.Common;

using System;
using System.Collections.Generic;
using System.Linq;

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

    public void UnsubscribeAll()
    {
        foreach (var subscription in _subscriptions.ToArray())
        {
            _subscriptions.Remove(subscription);
            subscription.Dispose();
        }
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

    public static SubscriptionList operator +(SubscriptionList list, IDisposable disposable)
    {
        list._subscriptions.Add(disposable);
        return list;
    }

    public static SubscriptionList operator -(SubscriptionList list, IDisposable disposable)
    {
        list._subscriptions.Remove(disposable);
        return list;
    }
}
