namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Timers;

    /// <summary>
    ///     SystemTimerWrapper is a general purpose timer. It will need to be disposed of properly.
    /// </summary>
    public class SystemTimerWrapper : ISystemTimerWrapper
    {
        private readonly Timer _timer;
        private bool _disposed;

        /// <summary>
        ///     Creates and returns a new instance of GeneralTimer.
        /// </summary>
        public SystemTimerWrapper()
        {
            _timer = new Timer();
        }

        /// <inheritdoc />
        public void Start()
        {
            _timer.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _timer.Stop();
        }

        /// <inheritdoc />
        public bool AutoReset
        {
            get => _timer.AutoReset;
            set => _timer.AutoReset = value;
        }

        /// <inheritdoc />
        public bool Enabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        /// <inheritdoc />
        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        /// <inheritdoc />
        public event ElapsedEventHandler Elapsed
        {
            add => _timer.Elapsed += value;
            remove => _timer.Elapsed -= value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes the GeneralTimer and its internal System.Timer
        /// </summary>
        /// <param name="disposing">Whether or not the dispose the resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer.Dispose();
            }

            _disposed = true;
        }
    }
}