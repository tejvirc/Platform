namespace Aristocrat.Monaco.RobotController
{
    using Accounting.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Gaming.Contracts.Models;
    using Kernel;
    using System.Threading;
    using Kernel.Contracts;
    using Test.Automation;
    using System.Linq;

    public sealed partial class RobotController
    {
        private bool _voucherRedeemed;

        private void HandlerStarting()
        {
            LoadConfiguration();

            _automator.SetOverlayText(_config.ActiveType.ToString(), false, _overlayTextGuid, InfoLocation.TopLeft);
            _lobbyState = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>();

            if (_config.Active.MaxWinLimitOverrideMilliCents > 0)
            {
                _automator.SetMaxWinLimit(_config.Active.MaxWinLimitOverrideMilliCents);
            }

            var desiredTransition = RobotControllerState.Disabled;

            switch (PlatformState)
            {
                case RobotPlatformState.InRecovery:
                {
                    desiredTransition = RobotControllerState.DriveRecovery;
                    break;
                }
                case RobotPlatformState.GamePlaying:
                case RobotPlatformState.GameIdle:
                case RobotPlatformState.GameLoaded:
                {
                    desiredTransition = RobotControllerState.Running;
                    break;
                }
                case RobotPlatformState.Lobby:
                {
                    if (_gameLoaded)
                    {
                        PlatformState = RobotPlatformState.GameLoaded;
                        desiredTransition = RobotControllerState.Running;
                    }
                    else
                    {
                        desiredTransition = RobotControllerState.RequestGameLoad;
                        _config.SelectNextGame();
                    }

                    break;
                }
                case RobotPlatformState.InAudit:
                {
                    if (!ExpectingLockup)
                    {
                        LogInfo("Entered audit menu unexpectedly. Disabling.");
                        Enabled = false;
                    }

                    break;
                }
                default:
                {
                    LogFatal("Disabling because it could not determine the state of the platform. Are comms online for the protocol configured?");
                    break;
                }

            }

            ControllerState = desiredTransition;

            // Must come last to override other states
            BalanceCheck();
        }

        private void HandlerRequestGameLoad()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            bool timeLimitDialogVisible = _pm.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);

            bool timeLimitDialogPending = _pm.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);

            if (PlatformState != RobotPlatformState.InCashOut &&
                !timeLimitDialogPending && 
                !timeLimitDialogVisible)
            {
                if (RequestGameLoad())
                {
                    ControllerState = (RobotControllerState.WaitGameLoad);
                }
                else
                {
                    // Leads to a disabled state
                    LogInfo("Game load request failed. Disabling.");
                    Enabled = false;
                }
            }
            else
            {
                LogInfo("Waiting for responsible gaming dialog to be dismissed.");
            }
        }

        private void HandlerForceGameExit()
        {
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
            ControllerState = (RobotControllerState.WaitForRecoveryStart);
        }

        private void HandlerRequestGameExit()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (PlatformState != RobotPlatformState.InRecovery)
            {
                _automator.EnableExitToLobby(true);

                if (PlatformState != RobotPlatformState.Lobby)
                {
                    _automator.RequestGameExit();
                    ControllerState = (RobotControllerState.WaitLobbyLoad);
                }
                else
                {
                    ControllerState = (RobotControllerState.Running);
                }
            }
        }

        private void HandlerWaitGameLoad()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (PlatformState == RobotPlatformState.GameLoaded ||
                PlatformState == RobotPlatformState.GamePlaying)
            {
                ControllerState = (RobotControllerState.Running);
            }
        }

        private void HandlerRunning()
        {
            IdleCheck();

            if (!Enabled)
            {
                ControllerState = (RobotControllerState.Disabled);
            }

            if (TimeTo(_counter, _config.Active.IntervalTouch))
            {
                ActionTouch();
            }

            if (TimeTo(_counter, 5000))
            {
                _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            }

            if(TimeTo(_counter, _config.Active.IntervalServiceRequest))
            {
                _automator.ServiceButton(_serviceRequested);

                _serviceRequested = !_serviceRequested;
            }

            //any actions involving state transitions should come after

            if (TimeTo(_counter, _config.Active.IntervalBalanceCheck))
            {
                BalanceCheck();
            }

            if  (PlatformState == RobotPlatformState.GameLoaded ||
                PlatformState == RobotPlatformState.GamePlaying ||
                PlatformState == RobotPlatformState.GameIdle)
            {
                if (TimeTo(_counter, _config.Active.IntervalAction))
                {
                    ActionPlayer();
                }

            }

            if (!_lobbyState.AllowSingleGameAutoLaunch && TimeTo(_counter, _config.Active.IntervalLobby))
            {
                //decides to enter a game or go to lobby
                ActionLobby(_counter);
            }

            

            if (_config.ActiveType == ModeType.Uber)
            {
                if (TimeTo(_counter, _config.Active.IntervalSoftReboot))
                {
                    LogInfo("Requesting soft reboot");
                    ControllerState = (RobotControllerState.RequestSoftReboot);
                }

                if(TimeTo(_counter, _config.Active.IntervalRebootMachine))
                {
                    LogInfo("Rebooting the machine");
                    OSManager.ResetComputer();
                }
            }

            if (TimeTo(_counter, _config.Active.IntervalSetOperatingHours))
            {
                LogInfo("Setting Operating Hours");
                ControllerState = (RobotControllerState.SettingOperatingHours);
                return;
            }

            if (TimeTo(_counter, _config.Active.IntervalLoadAuditMenu))
            {
                HandlerLoadAuditMenu();
            }

            if (TimeTo(_counter, _config.Active.IntervalTriggerLockup))
            {
                HandlerEnterLockup();
            }

            if (_bank.QueryBalance() > 0 && TimeTo(_counter, _config.Active.IntervalCashOut))
            {
                if ((bool)_pm.GetProperty(GamingConstants.AwaitingPlayerSelection, false))
                {
                    LogInfo("AwaitingPlayerSelection - Not Requesting Cashout");
                }
                else
                {
                    if (IsPlatformIdle())
                    {
                        LogInfo("Requesting Cashout");
                        _automator.EnableCashOut(true);
                        _automator.EnableExitToLobby(true);
                        _eventBus.Publish(new CashOutButtonPressedEvent());
                        ControllerState = (RobotControllerState.WaitCashOut);
                    }
                }
            }
        }

        private void HandlerWaitLobbyLoad()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (PlatformState == RobotPlatformState.Lobby)
            {
                ControllerState = (RobotControllerState.Running);
            }
        }

        private void HandlerWaitForRecoveryStart()
        {
            _waitDuration += _config.Active.IntervalResolution;

            if (_waitDuration >= 2000)
            {
                _waitDuration = 0;
                ControllerState = (RobotControllerState.DriveRecovery);
            }
        }

        private void HandlerDriveRecovery()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (PlatformState == RobotPlatformState.InRecovery ||
                // Poker can require a play request after recovery is complete but during in game round
                PlatformState == RobotPlatformState.GamePlaying)
            {
                // Some recovery scenarios require touch
                DriveToIdle();
            }
            // If we cause recovery, let it play out
            else if (PlatformState != RobotPlatformState.GamePlaying)
            {
                _expectingRecovery = false;
                ControllerState = (RobotControllerState.RecoveryComplete);
            }
        }

        private void HandlerRecoveryComplete()
        {
            _waitDuration += _config.Active.IntervalResolution;

            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (_waitDuration >= 4000 && PlatformState == RobotPlatformState.GamePlaying)
            {
                _waitDuration = 0;

                // If a game is being played after 4 seconds, go back to driving recovery. A free game feature may have started.
                ControllerState = (RobotControllerState.DriveRecovery);
            }
            else if (_waitDuration >= 8000 && PlatformState != RobotPlatformState.GamePlaying)
            {
                _waitDuration = 0;
                if (_config.ActiveType == ModeType.Super || _config.ActiveType == ModeType.Uber)
                {
                    if (PlatformState != RobotPlatformState.Lobby && _gameLoaded)
                    {
                        ControllerState = (RobotControllerState.RequestGameExit);
                    }
                    else
                    {
                        ControllerState = (RobotControllerState.RequestGameLoad);
                    }
                }
                else
                {
                    LogInfo("Recovery complete but no game playing. Disabling.");
                    Enabled = false;
                }
            }
        }

        private void HandlerLoadAuditMenu()
        {
            if (IsPlatformIdle() || ExpectingLockup)
            {
                LogInfo("Requesting Audit Menu");
                _expectingAuditMenu = true;
                PreviousPlatformState = PlatformState;
                _automator.LoadAuditMenu();

                _exitAuditMenuTimer = new Timer(
                    (sender) =>
                    {
                        _automator.ExitAuditMenu();

                        _expectingAuditMenu = false;

                        _exitAuditMenuTimer.Dispose();
                    },
                    null,
                    Constants.AuditMenuDuration,
                    System.Threading.Timeout.Infinite);
            }
            else
            {
                LogInfo("Not Requesting Audit Menu - Not Idle");
                DriveToIdle();
            }
        }

        private void HandlerExitAuditMenu()
        {
            _waitDuration += _config.Active.IntervalResolution;

            if (_waitDuration > Constants.AuditMenuDuration)
            {
                _waitDuration = 0;
                _automator.ExitAuditMenu();
                ControllerState = (RobotControllerState.Running);
            }
        }

        private void HandlerEnterLockup()
        {
            if(!ExpectingLockup){
                return;
            }
            _automator.EnterLockup();
            _lockupTimer = new Timer(
            (sender) =>
            {
                _automator.ExitLockup();
                _lockupTimer.Dispose();
            }, null, Constants.LockupDuration, System.Threading.Timeout.Infinite);
        }

        private void HandlerExitLockup()
        {
            _waitDuration += _config.Active.IntervalResolution;

            if (_waitDuration > Constants.LockupDuration)
            {
                _waitDuration = 0;
                _automator.ExitLockup();
                ControllerState = (RobotControllerState.Running);
            }
        }

        private void HandlerWaitCashOut()
        {
            _waitDuration += _config.Active.IntervalResolution;

            if (TimeTo(_counter, 500))
            {
                _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            }

            if (_waitDuration > Constants.CashOutTimeout)
            {
                _waitDuration = 0;
                _automator.EnableCashOut(false);
                _automator.EnableExitToLobby(false);
                ControllerState = (RobotControllerState.Running);
            }
        }

        private void HandlerRequestSoftReboot()
        {
            if (PlatformState == RobotPlatformState.GameIdle)
            {
                _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));

                ControllerState = (RobotControllerState.WaitRebootStart);
            }
            else
            {
                DriveToIdle();
            }
        }

        private void HandlerSettingOperatingHours()
        {
            SetOperatingHours();

            ControllerState = (RobotControllerState.OutOfOperatingHours);
        }

        private void HandlerOutOfOperatingHours()
        {

            if (PlatformState == RobotPlatformState.GameLoaded ||
                PlatformState == RobotPlatformState.GamePlaying ||
                PlatformState == RobotPlatformState.GameIdle)
            {
                if (TimeTo(_counter, _config.Active.IntervalAction))
                {
                    ActionPlayer();
                }

                if (TimeTo(_counter, _config.Active.IntervalTouch))
                {
                    ActionTouch();
                }
            }

            if (TimeTo(_counter, _config.Active.IntervalLoadAuditMenu))
            {
                HandlerLoadAuditMenu();
            }

            if (TimeTo(_counter, _config.Active.IntervalTriggerLockup))
            {
                HandlerEnterLockup();
            }
        }

        private void HandlerInsertCredits()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            if (IsPlatformIdle())
            {
                _waitDuration += _config.Active.IntervalResolution;

                if (_waitDuration >= Constants.InsertCreditsDelay)
                {
                    _eventBus.Subscribe<BankBalanceChangedEvent>(this, _ =>
                    {
                        _bankBalanceChangedEventReceived = true;
                    });

                    _eventBus.Subscribe<CurrencyInCompletedEvent>(this, evt =>
                    {
                        _currencyInCompletedEventReceived = true;

                        if (evt.Amount.Equals(0L))
                        {
                            if (_transactionHistory == null)
                            {
                                _transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();
                            }

                            var transaction = _transactionHistory.RecallTransactions<BillTransaction>()
                                .OrderByDescending(b => b.LogSequence).FirstOrDefault();

                            if (transaction != null)
                            {
                                if (transaction.Exception.Equals((int)CurrencyInExceptionCode.CreditInLimitExceeded) ||
                                    transaction.Exception.Equals((int)CurrencyInExceptionCode.LaundryLimitExceeded))
                                {
                                    _eventBus.Publish(new CashOutButtonPressedEvent());
                                }
                            }
                        }
                    });

                    _waitDuration = 0;

                    _automator.InsertDollars(_config.GetDollarsInserted());

                    ControllerState = (RobotControllerState.InsertCreditsComplete);
                }
            }
            else
            {
                _waitDuration = 0;

                DriveToIdle();
            }
        }

        private void HandlerInsertCreditsComplete()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            //wait out credits inserted transaction until an event is received to exit the state
            if (_bankBalanceChangedEventReceived && _currencyInCompletedEventReceived)
            {
                _waitDuration += _config.Active.IntervalResolution;

                if (_waitDuration >= 10 * _config.Active.IntervalResolution)
                {
                    _timeoutDuration = 0;

                    _waitDuration = 0;

                    _bankBalanceChangedEventReceived = false;
                    _currencyInCompletedEventReceived = false;

                    _eventBus.Unsubscribe<BankBalanceChangedEvent>(this);

                    ControllerState = (PreviousControllerState);
                }
            }
            else
            {
                _timeoutDuration += _config.Active.IntervalResolution;

                // If we post a DebugNoteEvent during a state where it will be discarded, we need to time out and repost
                if(_timeoutDuration > Constants.InsertCreditsTimeout)
                {
                    _timeoutDuration = 0;

                    _bankBalanceChangedEventReceived = false;
                    _currencyInCompletedEventReceived = false;

                    _eventBus.Unsubscribe<BankBalanceChangedEvent>(this);

                    ControllerState = (RobotControllerState.InsertCredits);
                }
            }
        }

        private void HandlerInsertVoucher()
        {
            if (IsPlatformIdle())
            {
                _waitDuration += _config.Active.IntervalResolution;

                if (_lastVoucherIssued != null)
                {
                    _automator.InsertVoucher(_lastVoucherIssued.Barcode);

                    _eventBus.Subscribe<VoucherRedeemedEvent>(this, _ =>
                    {
                        _voucherRedeemed = true;
                    });

                    ControllerState = (RobotControllerState.InsertVoucherComplete);
                }
                else
                {
                    ControllerState = (PreviousControllerState);
                }
            }
            else
            {
                _waitDuration = 0;

                DriveToIdle();
            }
        }

        private void HandlerInsertVoucherComplete()
        {
            if (_voucherRedeemed)
            {
                _timeoutDuration = 0;

                _eventBus.Unsubscribe<VoucherRedeemedEvent>(this);

                _lastVoucherIssued = null;

                ControllerState = (PreviousControllerState);
            }
            else
            {
                _timeoutDuration += _config.Active.IntervalResolution;
            }
        }

        private void DriveToIdle()
        {
            IdleCheck();

            if (TimeTo(_counter, _config.Active.IntervalTouch))
            {
                ActionTouch();
            }

            if (TimeTo(_counter, 2000))
            {
                LogInfo("Spin Request");
                _automator.SpinRequest();
            }
        }

        private void IdleCheck()
        {
            if (_idleDuration > Constants.IdleTimeout)
            {
				_idleDuration = 0;
                LogInfo("Idle for too long. Disabling.");
                Enabled = false;
            }
        }

        private bool IsPlatformIdle()
        {
            return (PlatformState == RobotPlatformState.GameIdle ||
                    PlatformState == RobotPlatformState.Lobby ||
                    PlatformState == RobotPlatformState.GameLoaded)
                && PlatformState != RobotPlatformState.GamePlaying;
        }

        private bool TimeTo(long counter, long interval)
        {
            if (interval == 0) return false;
            long prevCounter = counter - _config.Active.IntervalResolution;
            if (prevCounter < 0) return false;

            var eventCount = counter / interval;
            var prevCount = prevCounter / interval;

            // We should now have fired eventCount times, which is greater than the
            // number of times we should have fired on the previous check. That means
            // it's time to fire.
            return eventCount > prevCount;
        }
    }
}
