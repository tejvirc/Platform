namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using Kernel;
    using System;
    using System.Collections.Generic;

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

        /// <summary> Disposes the object </summary>
        /// <param name="disposing">Indicates whether to dispose</param>
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
}
