namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System;
using System.Collections.Generic;
using Kernel;

/// <summary>
///     An implementation of <see cref="ISharedConsumer" />
/// </summary>
public class SharedConsumerContext : ISharedConsumer, IDisposable
{
    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public string Name => GetType().ToString();

    /// <inheritdoc />
    public ICollection<Type> ServiceTypes => new[] { typeof(ISharedConsumer) };

    /// <inheritdoc />
    public void Initialize()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }

        _disposed = true;
    }
}
