namespace Aristocrat.Monaco.Sas.ChangeRequests
{
    using System;
    using System.Timers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>Definition of the MeterChangeExceptionHandler class.</summary>
    public sealed class MeterChangeExceptionHandler : ISasMeterChangeHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double DefaultCancellationInterval = 30000.0; // 30 seconds
        private const double ExceptionInterval = 5000.0; // 5 seconds
        private const uint MaxCancellationResets = 5;
        private bool _disposed;
        private readonly SasExceptionTimer _exceptionTimer;
        private Timer _cancellationTimer;
        private MeterCollectStatus _status;
        private uint _numberOfCancellationResets;

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>Constructs the MeterChangeExceptionHandler object</summary>
        public MeterChangeExceptionHandler(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _cancellationTimer = new Timer();
            _cancellationTimer.Elapsed += OnCancellation;
            _cancellationTimer.Interval = DefaultCancellationInterval;
            _numberOfCancellationResets = 0;

            _exceptionTimer = new SasExceptionTimer(
                _exceptionHandler,
                GetMeterChangeException,
                TimerActive,
                ExceptionInterval);

            MeterChangeStatus = MeterCollectStatus.NotInPendingChange;
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> OnChangeCommit;

        /// <inheritdoc />
        public event EventHandler<EventArgs> OnChangeCancel;

        /// <inheritdoc />
        public MeterCollectStatus MeterChangeStatus
        {
            get => _status;
            private set
            {
                _status = value;
                _propertiesManager.SetProperty(SasProperties.MeterCollectStatusKey, (byte)value);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                CancelTimer();

                _cancellationTimer.Elapsed -= OnCancellation;
                _cancellationTimer.Close();
                _cancellationTimer = null;
                _exceptionTimer.Dispose();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public void StartPendingChange(MeterCollectStatus status, double timeoutValue)
        {
            Logger.Debug($"MeterPendingChange -- status({status})");

            if (HandlePendingChange(status))
            {
                StartTimer(timeoutValue);
            }
        }

        /// <inheritdoc />
        public void AcknowledgePendingChange()
        {
            if (!ResetCancellationTimer())
            {
                CancelPendingChange();
            }
        }

        /// <inheritdoc />
        public void ReadyForPendingChange()
        {
            CancelTimer();

            switch (MeterChangeStatus)
            {
                case MeterCollectStatus.GameDenomPaytableChange:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EnabledGamesDenomsChanged));
                    break;
                case MeterCollectStatus.LifetimeMeterChange:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GamingMachineSoftMetersReset));
                    break;
            }

            MeterChangeStatus = MeterCollectStatus.NotInPendingChange;
            OnChangeCommit?.Invoke(this, null);
        }

        /// <inheritdoc />
        public void CancelPendingChange()
        {
            Logger.Debug($"CancelPendingChange, status = {MeterChangeStatus}");

            CancelTimer();
            if (MeterChangeStatus != MeterCollectStatus.NotInPendingChange)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.MeterChangeCanceled));
                MeterChangeStatus = MeterCollectStatus.NotInPendingChange;
                OnChangeCancel?.Invoke(this, null);
            }
        }

        /// <summary>Returns whether cancellation timer is running</summary>
        public bool TimerActive() => _cancellationTimer.Enabled;

        private bool HandlePendingChange(MeterCollectStatus status)
        {
            Logger.Info($"Start of HandlePendingChange({status}).");
            var handled = true;

            switch (status)
            {
                case MeterCollectStatus.LifetimeMeterChange:
                case MeterCollectStatus.GameDenomPaytableChange:
                    // Update the status if not already requesting a life-time meter change
                    if (MeterChangeStatus != MeterCollectStatus.LifetimeMeterChange)
                    {
                        MeterChangeStatus = status;
                    }
                    break;
                default:
                    handled = false;
                    break;
            }

            return handled;
        }

        private static GeneralExceptionCode? GetMeterChangeException()
        {
            return GeneralExceptionCode.MeterChangePending;
        }

        private void StartTimer(double timeoutValue)
        {
            if (timeoutValue < 0)
            {
                timeoutValue = DefaultCancellationInterval;
            }

            _cancellationTimer.Stop();
            _cancellationTimer.Interval = timeoutValue;
            _cancellationTimer.Start();
            _exceptionTimer.StartTimer();
        }

        private void CancelTimer()
        {
            _numberOfCancellationResets = 0;
            _cancellationTimer.Stop();
            _exceptionTimer.StopTimer();
        }

        private bool ResetCancellationTimer()
        {
            var ableToResetTimer = (++_numberOfCancellationResets < MaxCancellationResets);

            if (ableToResetTimer)
            {
                _cancellationTimer.Stop();
                _cancellationTimer.Start();
            }

            return ableToResetTimer;
        }

        private void OnCancellation(object source, ElapsedEventArgs elapsedEventArgs)
        {
            Logger.Debug("OnCancellation, status = {MeterChangeStatus} -- host did not respond");
            CancelPendingChange();
        }
    }
}
