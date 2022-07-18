namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    using System;
    using System.Threading;
    using System.Timers;
    using Aristocrat.Sas.Client;
    using Common;
    using Contracts.Eft;
    using Gaming.Contracts;
    using Kernel;

    /// <inheritdoc cref="IEftHostCashOutProvider" />
    public class EftHostCashoutProvider : IEftHostCashOutProvider, IDisposable
    {
        private const double HostCashOutTimeOut = 800.0;

        private readonly AutoResetEvent _hostCashOutResetEvent;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly ISystemDisableManager _disableProvider;
        private readonly IGamePlayState _gamePlayState;

        private CashOutReason _cashOutReason;
        private ISystemTimerWrapper _timer;
        private bool _disposed;

        /// <summary>
        ///     Creates the HostCashOutProvider instance.
        /// </summary>
        /// <param name="exceptionHandler">Instance of <see cref="ISasExceptionHandler"</param>
        /// <param name="disableProvider">Instance of <see cref="ISystemDisableManager"</param>
        /// <param name="gamePlayState">Instance of <see cref="IGamePlayState"</param>
        public EftHostCashoutProvider(
            ISasExceptionHandler exceptionHandler,
            ISystemDisableManager disableProvider,
            IGamePlayState gamePlayState)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));

            _hostCashOutResetEvent = new AutoResetEvent(false);
            SetTimer();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public CashOutReason HandleHostCashOut()
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.CashOutButtonPressed));

            if (!CanCashoutNormally())
            {
                return CashOutReason.None;
            }

            _timer.Start();
            _hostCashOutResetEvent.WaitOne();
            return _cashOutReason;
        }

        /// <inheritdoc />
        public bool CashOutAccepted()
        {
            if (!_timer.Enabled)
            {
                return false;
            }

            _cashOutReason = CashOutReason.CashOutAccepted;
            _timer.Stop();
            _hostCashOutResetEvent.Set();
            return true;
        }

        /// <inheritdoc />
        public void RestartTimerIfPendingCallbackFromHost()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        /// <summary>
        ///     Sets the timerWrapper as well as includes a stub method for unit testing purposes.
        /// </summary>
        /// <param name="timerWrapper">Timer to be mocked in unit testing.</param>
        public void SetTimer(ISystemTimerWrapper timerWrapper = null)
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimedOut;
            }

            _timer = timerWrapper ?? new SystemTimerWrapper { Interval = HostCashOutTimeOut, AutoReset = false };
            _timer.Elapsed += TimedOut;
        }

        /// <summary>
        ///     Handles the disposing of the unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _hostCashOutResetEvent.Dispose();
                _timer.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        ///     As per section 8.8, if gaming machine has Non-Cashable present or is in game-play, tilt condition;
        ///     we de not need to wait 800ms for the host to initiate a force cashout and can cashout normally.
        /// </summary>
        /// <returns></returns>
        private bool CanCashoutNormally()
        {
            //the EFT NonCashable is not supported
            return !_disableProvider.IsDisabled &&
                   _gamePlayState.CurrentState == PlayState.Idle;
        }

        private void TimedOut(object sender, ElapsedEventArgs e)
        {
            _cashOutReason = CashOutReason.TimedOut;
            _hostCashOutResetEvent.Set();
        }
    }
}