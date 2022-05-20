namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Timers;

    /// <inheritdoc cref="IProgressiveBroadcastTimer" />
    public sealed class ProgressiveBroadcastTimer : IProgressiveBroadcastTimer, IDisposable
    {
        private const int DefaultIntervalInMs = 250;
        private readonly Timer _timer;
        private bool _disposed;

        public ProgressiveBroadcastTimer()
        {
            _timer = new Timer(DefaultIntervalInMs) { AutoReset = true };
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _timer.Dispose();
            _disposed = true;
        }

        public Timer Timer => _timer;
    }
}