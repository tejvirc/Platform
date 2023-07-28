namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using System.Timers;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     A Sas Exception Timer for reporting a given exception every few seconds
    /// </summary>
    public class SasExceptionTimer : IDisposable
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly Func<GeneralExceptionCode?> _exceptionSupplier;
        private readonly Func<bool> _timerResetHandler;
        private readonly double _timerTimeout;
        private readonly byte? _clientNumber;
        private readonly object _lockObject = new object();
        private readonly Timer _timer;

        private bool _disposed;

        /// <summary>
        ///     Creates a SasExceptionTimer
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="exceptionSupplier">The exception to report</param>
        /// <param name="timerResetHandler">The handler for whether or not to reset a running timer</param>
        /// <param name="timerTimeout">The interval to report the exception on</param>
        /// <param name="clientNumber">The client number for this exception</param>
        public SasExceptionTimer(
            ISasExceptionHandler exceptionHandler,
            Func<GeneralExceptionCode?> exceptionSupplier,
            Func<bool> timerResetHandler,
            double timerTimeout,
            byte? clientNumber = null)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _exceptionSupplier = exceptionSupplier ?? throw new ArgumentNullException(nameof(exceptionSupplier));
            _timerResetHandler = timerResetHandler ?? throw new ArgumentNullException(nameof(timerResetHandler));
            _timerTimeout = timerTimeout;
            _clientNumber = clientNumber;

            _timer = new Timer
            {
                AutoReset = false,
                Enabled = false,
                Interval = _timerTimeout
            };

            _timer.Elapsed += TimerOnElapsed;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Starts the timer and reports the exception
        /// </summary>
        /// <param name="restart">Whether or not the post the exception now and reset the timer</param>
        /// <returns>Whether or not the timer was started</returns>
        public bool StartTimer(bool restart = false)
        {
            Console.WriteLine($"[{DateTime.Now}] - [{nameof(StartTimer)}-0] - [{GetHashCode()}] - [{Environment.CurrentManagedThreadId}]");

            lock (_lockObject)
            {
                if (_timer.Enabled && !restart)
                {
                    return false;
                }

                _timer.Stop();
                _timer.Interval = _timerTimeout;
                var sasExceptionType = _exceptionSupplier.Invoke();
                if (!sasExceptionType.HasValue)
                {
                    return false;
                }

                Console.WriteLine($"[{DateTime.Now}] - [{nameof(StartTimer)}-1] - [{_clientNumber.HasValue}] - [{Environment.CurrentManagedThreadId}]");

                if (_clientNumber.HasValue)
                {
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(sasExceptionType.Value), _clientNumber.Value);
                }
                else
                {
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(sasExceptionType.Value));
                }

                Console.WriteLine($"[{DateTime.Now}] - [{nameof(StartTimer)}-2] - [{sasExceptionType.Value}] - [{Environment.CurrentManagedThreadId}]");

                _timer.Enabled = true;
                _timer.Start();

                return true;
            }
        }

        /// <summary>
        ///     Stops the timer
        /// </summary>
        public void StopTimer()
        {
            lock (_lockObject)
            {
                _timer.Stop();
                _timer.Enabled = false;
            }
        }

        /// <summary>
        ///     The dispose handing
        /// </summary>
        /// <param name="disposing">Whether or not to dispose it resources</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_lockObject)
            {
                if (!disposing || _disposed)
                {
                    return;
                }

                _timer.Elapsed -= TimerOnElapsed;
                _timer.Stop();
                _timer.Dispose();
                _disposed = true;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                if (_disposed)
                {
                    // We can get events after disposing so don't do anything
                    return;
                }

                if (_timerResetHandler.Invoke())
                {
                    StartTimer();
                }
                else
                {
                    StopTimer();
                }
            }
        }
    }
}