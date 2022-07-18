namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Contracts.Client;
    using Contracts.Events;
    using Gaming.Contracts;
    using Kernel;
    using log4net;

    /// <summary>Definition of the AftLockHandler class.</summary>
    public sealed class AftLockHandler : IAftLockHandler, IService, IDisposable, ITransactionRequestor
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _disposed;
        private Timer _aftLockTimer;
        private readonly SasExceptionTimer _lockExceptionTimer;
        private const double LockExceptionInterval = 5000.0; // 5 seconds
        private Guid _transactionRequestId;

        private readonly object _lock = new object();
        private readonly ISasHost _sasHost;
        private readonly IEventBus _bus;
        private readonly IAftHostCashOutProvider _aftHostCashOutProvider;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IMessageDisplay _messageDisplay;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IAutoPlayStatusProvider _autoPlayStatusProvider;
        private readonly IRuntimeFlagHandler _runtimeFlagHandler;
        private readonly IGamePlayState _gamePlay;
        private bool _autoPlayActive;
        private bool _unlockPending;

        /// <summary>Constructs the AftLockHandler object</summary>
        public AftLockHandler(
            ISasHost sasHost,
            IEventBus bus,
            ISasExceptionHandler exceptionHandler,
            IAftHostCashOutProvider aftHostCashOutProvider,
            ITransactionCoordinator transactionCoordinator,
            IMessageDisplay messageDisplay,
            ISystemDisableManager systemDisableManager,
            IAutoPlayStatusProvider autoPlayStatusProvider,
            IRuntimeFlagHandler runtimeFlagHandler,
            IGamePlayState gamePlay)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _aftHostCashOutProvider = aftHostCashOutProvider ?? throw new ArgumentNullException(nameof(aftHostCashOutProvider));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _autoPlayStatusProvider =
                autoPlayStatusProvider ?? throw new ArgumentNullException(nameof(autoPlayStatusProvider));
            _runtimeFlagHandler = runtimeFlagHandler ?? throw new ArgumentNullException(nameof(runtimeFlagHandler));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));

            _lockExceptionTimer = new SasExceptionTimer(
                exceptionHandler,
                GetGameLockedException,
                TimerInactive,
                LockExceptionInterval);

            LockStatus = AftGameLockStatus.GameNotLocked;
            _bus.Subscribe<SystemDisableRemovedEvent>(
                this,
                _ => ProcessAftLock(),
                _ => CurrentTransactionId != Guid.Empty && CanLock);
            _bus.Subscribe<GameIdleEvent>(
                this,
                _ => ProcessAftLock(),
                _ => CurrentTransactionId != Guid.Empty && CanLock);
            _bus.Subscribe<SystemDisableAddedEvent>(
                this,
                async (_, _) => await HandleAftUnlock(),
                _ => CurrentTransactionId != Guid.Empty && !_aftHostCashOutProvider.CanCashOut);
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> OnLocked;

        /// <inheritdoc />
        public AftGameLockStatus LockStatus { get; private set; }

        /// <inheritdoc />
        public Guid RequestorGuid => SasConstants.AftLockHandlerGuid;

        /// <inheritdoc />
        public string Name => typeof(AftLockHandler).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAftLockHandler) };

        private bool CanLock =>
            (!_systemDisableManager.DisableImmediately &&
             (_gamePlay.UncommittedState == PlayState.Idle ||
              (AftLockTransferConditions & AftTransferConditions.BonusAwardToGamingMachineOk) != 0)) ||
            _aftHostCashOutProvider.CanCashOut;

        /// <inheritdoc />
        public void Initialize()
        {
            Recover();
            Logger.Info("The service has been initialized!");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                CancelTimer();
                _bus.UnsubscribeAll(this);
                _lockExceptionTimer.Dispose();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public void AftLock(bool requestLock, uint timeout)
        {
            if (requestLock)
            {
                Logger.Debug($"Lock requested -- timeout: {timeout}");
                HandleAftLock(timeout);
            }
            else
            {
                Logger.Debug($"Unlock requested -- timeout: {timeout}");
                HandleAftUnlock();
            }
        }

        /// <inheritdoc />
        public Guid RetrieveTransactionId()
        {
            var transactionId = CurrentTransactionId;
            CurrentTransactionId = Guid.Empty;
            return transactionId;
        }

        /// <inheritdoc />
        public AftTransferConditions AftLockTransferConditions { get; set; }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
            // Queue the transaction handling to avoid blocking the transaction coordinator.
            Logger.Debug("Transaction is ready to retrieve for handling.");
            Task.Run(() => OnTransactionReady(requestId));
        }

        private Guid CurrentTransactionId { get; set; }

        private uint Timeout { get; set; }

        private void OnTransactionReady(Guid threadContext)
        {
            Logger.Debug("Starting to process the transaction...");

            lock (_lock)
            {
                _transactionRequestId = threadContext;
                CurrentTransactionId = _transactionCoordinator.RetrieveTransaction(_transactionRequestId);
                StartAftLock(Timeout);
            }

            // To avoid a deadlock, do not put it in Lock(...) block
            OnLocked?.Invoke(this, null);

            Logger.Debug("The transaction has been processed!");
        }

        private async void AftLockTimeout(object source, ElapsedEventArgs elapsedEventArgs)
        {
            Logger.Debug("Aft Lock timed out");
            await HandleAftUnlock();
        }

        private void HandleAftLock(uint timeout)
        {
            Logger.Info("Start of HandleAftLock().");
            if (timeout <= 0)
            {
                Logger.Error($"HandleAftLock(): {timeout} is an invalid timeout value; Request Unlock");
                HandleAftUnlock();
                return;
            }

            if (!CanLock && _systemDisableManager.CurrentDisableKeys.Except(AftConstants.AllowedAftOffDisables).Any())
            {
                Logger.Warn("HandleAftLock(): System is disabled: cannot lock");
                return;
            }

            Logger.Debug($"HandleAftLock(): System is enabled: can lock LockStatus={LockStatus}");
            Timeout = timeout;
            switch (LockStatus)
            {
                case AftGameLockStatus.GameLockPending:
                    Logger.Warn("Attempting to handle a lock request while another lock request is pending");
                    return;
                case AftGameLockStatus.GameNotLocked:
                    LockStatus = AftGameLockStatus.GameLockPending;
                    break;
            }

            _autoPlayActive = _autoPlayStatusProvider.EndAutoPlayIfActive();
            Task.Run(HandleLockRequest);
            Logger.Info("End of HandleAftLock().");
        }

        private void HandleLockRequest()
        {
            lock (_lock)
            {
                switch (LockStatus)
                {
                    case AftGameLockStatus.GameLocked:
                        StartAftLock(Timeout);
                        break;
                    case AftGameLockStatus.GameLockPending:
                        var lockAllowed = true;
                        // if any transfer conditions used to request lock are false, set lockAllowed = false
                        if (!_aftHostCashOutProvider.HostCashOutPending && CurrentTransactionId == Guid.Empty)
                        {
                            Logger.Debug("HandleAftLockRequest(): Requesting transaction...");

                            // Make sure the lock is released after timeout is complete.
                            CurrentTransactionId = _transactionCoordinator.RequestTransaction(this, TransactionType.Write);
                            lockAllowed = CurrentTransactionId != Guid.Empty && !_autoPlayActive && CanLock;
                            Logger.Debug($"HandleAftLock(): lockAllowed={lockAllowed}");
                        }

                        if (lockAllowed)
                        {
                            StartAftLock(Timeout);
                        }

                        break;
                }
            }
        }

        private void StartAftLock(uint timeout)
        {
            LockStatus = AftGameLockStatus.GameLocked;

            // Cancel any pending timers. Subsequent locks should use the new timeout value.
            CancelTimer();

            // Enable lock timer
            _aftLockTimer = new Timer(TimeoutMilliseconds(timeout)) { AutoReset = false };
            _aftLockTimer.Elapsed += AftLockTimeout;
            _aftLockTimer.Start();
            _lockExceptionTimer.StartTimer();

            // Display lock message
            _messageDisplay.DisplayMessage(AftConstants.LockMessage);
            _bus.Publish(new TransferLockEvent(true, AftLockTransferConditions));
            _unlockPending = true;

            // Notify runtime
            _runtimeFlagHandler.SetFundsTransferring(true);

            // Notify the lock is completed
            _sasHost.AftLockCompleted();
        }

        private static uint TimeoutMilliseconds(uint timeout)
        {
            return timeout * SasConstants.MillisecondsInHundredthSecond;
        }

        private static GeneralExceptionCode? GetGameLockedException()
        {
            return GeneralExceptionCode.GameLocked;
        }

        private Task HandleAftUnlock()
        {
            // Lock was canceled unlock system.
            LockStatus = AftGameLockStatus.GameNotLocked;
            // Cancel any pending timers. Subsequent locks should use the new timeout value.
            CancelTimer();
            // allow autoplay
            _autoPlayStatusProvider.UnpausePlayerAutoPlay();

            return Task.Run(ReleaseTransaction);
        }

        private void ReleaseTransaction()
        {
            lock (_lock)
            {
                if (CurrentTransactionId != Guid.Empty)
                {
                    _transactionCoordinator.ReleaseTransaction(CurrentTransactionId);
                }
                else
                {
                    _transactionCoordinator.AbandonTransactions(RequestorGuid);
                }

                CurrentTransactionId = Guid.Empty;

                // Remove please wait display.
                _messageDisplay.RemoveMessage(AftConstants.LockMessage);
                if (_unlockPending)
                {
                    _bus.Publish(new TransferLockEvent(false, AftLockTransferConditions));
                    _unlockPending = false;
                }

                AftLockTransferConditions = AftTransferConditions.None;

                // Notify runtime
                _runtimeFlagHandler.SetFundsTransferring(false);
            }
        }

        private bool TimerInactive() => _aftLockTimer != null;

        private void CancelTimer()
        {
            if (_aftLockTimer == null)
            {
                return;
            }

            _aftLockTimer.Stop();
            _aftLockTimer.Elapsed -= AftLockTimeout;
            _aftLockTimer.Close();
            _aftLockTimer = null;
            _lockExceptionTimer?.StopTimer();
        }

        private void Recover()
        {
            // If we are recovering just unlock the device.
            HandleAftUnlock();
        }

        private void ProcessAftLock()
        {
            lock (_lock)
            {
                StartAftLock(Timeout);
            }
        }
    }
}
