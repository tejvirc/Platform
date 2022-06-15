namespace Aristocrat.Monaco.RobotController
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Vouchers;
    using Application.Contracts;
    using Application.Contracts.Operations;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using G2S.Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Gaming.Contracts.Models;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hhr.Events;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Automation;

    public enum RobotControllerState
    {
        Disabled,
        Starting,
        Running,
        RequestGameLoad,
        RequestGameExit,
        ForceGameExit,
        WaitForRecoveryStart,
        DriveRecovery,
        RecoveryComplete,
        WaitGameLoad,
        WaitLobbyLoad,
        LoadAuditMenu,
        ExitAuditMenu,
        EnterLockup,
        ExitLockup,
        WaitCashOut,
        RecoveryConfused,
        RequestSoftReboot,
        WaitRebootStart,
        SettingOperatingHours,
        OutOfOperatingHours,
        InsertCredits,
        InsertCreditsComplete,
        InsertVoucher,
        InsertVoucherComplete
    }

    //RKS:TODO: search for an existing solution to the platform state 
    public enum RobotPlatformState
    {
        Unknown,
        Lobby,
        GameLoaded,
        GameIdle,
        GamePlaying,
        GameExiting,
        InRecovery,
        InAudit,
        InCashOut,
        CashOutComplete,
        WaitingKeyOff
    }

    public class TransferInformation
    {
        public long Amount { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }

    public sealed partial class RobotController : BaseRunnable, IRobotController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ReaderWriterLockSlim _controllerStateLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim _previousControllerStateLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim _previousPlatformStateLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim _platformStateLock = new ReaderWriterLockSlim();
        private readonly Guid _overlayTextGuid = new Guid("2774B299-E8FE-436C-B68C-F6CF8DCDB31B");
        private readonly TransferInformation _previousVoucherBalance = new TransferInformation();
        private readonly Dictionary<RobotControllerState, Func<bool>> _controlStateTransitionValidator = new Dictionary<RobotControllerState, Func<bool>>();

        private IEventBus _eventBus;
        private IBank _bank;
        private IPropertiesManager _pm;
        private ILobbyStateManager _lobbyState;
        private ITransactionHistory _transactionHistory;
        private Automation _automator;

        private Configuration _config = new Configuration();
        private IGameService _gameService;
        private RobotControllerState _controllerState = RobotControllerState.Disabled;
        private RobotControllerState _previousControllerState = RobotControllerState.Disabled;
        private RobotPlatformState _platformState = RobotPlatformState.Unknown;
        private RobotPlatformState _previousPlatformState = RobotPlatformState.Unknown;

        private bool _gameLoaded;
        private bool _isTimeLimitDialogVisible;
        private bool _bankBalanceChangedEventReceived;
        private bool _currencyInCompletedEventReceived;
        private bool _serviceRequested;
        private VoucherOutTransaction _lastVoucherIssued;
        private long _counter;
        private long _idleDuration;
        private long _waitDuration;
        private long _timeoutDuration;
        private bool _expectingAuditMenu;
        private bool _expectingRecovery;

        private Timer _lockupTimer;
        private Timer _exitAuditMenuTimer;

        private bool ExpectingLockup => _expectingAuditMenu ||
                                        (ControllerState == RobotControllerState.WaitCashOut ||
                                        ControllerState == RobotControllerState.EnterLockup ||
                                        ControllerState == RobotControllerState.ExitLockup ||
                                        ControllerState == RobotControllerState.LoadAuditMenu ||
                                        ControllerState == RobotControllerState.ExitAuditMenu);
        private bool ExpectingRecovery => ControllerState == RobotControllerState.ForceGameExit ||
                                          ControllerState == RobotControllerState.WaitForRecoveryStart ||
                                          ControllerState == RobotControllerState.OutOfOperatingHours ||
                                          _expectingRecovery;


        private RobotControllerState ControllerState
        {
            get
            {
                return ControllerStateReaderLockHandler();
            }

            set
            {
                if (value == _controllerState) { return; }
                if (_controllerState != value && !ValidateControlStateTransition(value))
                {
                    LogError($"ValidateStateTransition Failed for Transitioning to {value}");
                    return;
                }
                ControllerStateWriterLockHandler(value);
            }
        }

        private void ControllerStateWriterLockHandler(RobotControllerState value)
        {
            PreviousControllerState = ControllerState;
            _controllerStateLock.EnterWriteLock();
            try
            {
                if (_controllerState != value)
                {
                    Logger.Info($"Transition Controller to {value} state succeeded!");
                    _controllerState = value;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Transition Controller to {value} state failed: " + ex.ToString());
                Enabled = false;
            }
            finally
            {
                _controllerStateLock.ExitWriteLock();
            }
        }

        private RobotControllerState ControllerStateReaderLockHandler()
        {
            _controllerStateLock.EnterReadLock();
            try
            {
                return _controllerState;
            }
            catch (Exception ex) {
                Logger.Error("ControllerStateReaderLockHandler failed: " + ex.ToString());
                Enabled = false;
                return _controllerState;
            }
            finally
            {
                _controllerStateLock.ExitReadLock();
            }
        }

        private RobotControllerState PreviousControllerState
        {
            get
            {
                return PreviousControllerStateReaderLockHandler();
            }

            set
            {
                if (_previousControllerState == value) return;
                if (value == RobotControllerState.InsertCredits ||
                    value == RobotControllerState.InsertCreditsComplete ||
                    value == RobotControllerState.InsertVoucher ||
                    value == RobotControllerState.InsertVoucherComplete)
                {
                    return;
                }
                PreviousControllerStateWriterLockHandler(value);                
            }
        }

        private void PreviousControllerStateWriterLockHandler(RobotControllerState value)
        {
            _previousControllerStateLock.EnterWriteLock();
            try
            {
                if (_previousControllerState != value)
                {
                    _previousControllerState = value;
                    Logger.Info($"Transition PreviousController to {value} state succeeded!");
                }
            }
            catch (Exception ex) {
                Logger.Error($"Transition PreviousController to {value} state failed: " + ex.ToString());
                Enabled = false;
            }
            finally
            {
                _previousControllerStateLock.ExitWriteLock();
            }
        }

        private RobotControllerState PreviousControllerStateReaderLockHandler()
        {
            _previousControllerStateLock.EnterReadLock();
            try
            {
                return _previousControllerState;
            }
            catch (Exception ex)
            {
                Logger.Error("PreviousControllerStateReaderLockHandler Failed: " + ex.ToString());
                Enabled = false;
                return _previousControllerState;
            }
            finally
            {
                _previousControllerStateLock.ExitReadLock();
            }
        }

        private RobotPlatformState PlatformState
        {
            get
            {
                return PlatformStateReaderLockHandler();
            }
            set
            {
                if (value == _platformState) { return; }
                PlatformStateWriterLockHandler(value);
            }
        }

        private void PlatformStateWriterLockHandler(RobotPlatformState value)
        {
            PreviousPlatformState = PlatformState;
            _platformStateLock.EnterWriteLock();
            try
            {
                if (_platformState != value)
                {
                    _platformState = value;
                    Logger.Info($"Transition Platform to [{value}] state succeeded!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Transition Platform to [{value}] state failed! " + ex.ToString());
                Enabled = false;
            }
            finally
            {
                _platformStateLock.ExitWriteLock();
            }
        }

        private RobotPlatformState PlatformStateReaderLockHandler()
        {
            _platformStateLock.EnterReadLock();
            try
            {
                return _platformState;
            }
            catch (Exception ex)
            {
                Logger.Error("HandlePlatformStateReaderLock failed: " + ex.ToString());
                Enabled = false;
                return _platformState;
            }
            finally
            {
                _platformStateLock.ExitReadLock();
            }
        }

        private RobotPlatformState PreviousPlatformState
        {
            get
            {
                return PreviousPlatformStateReaderLockHandler();
            }

            set
            {
                if (_previousPlatformState == value) return;
                if (value == RobotPlatformState.InAudit || value == RobotPlatformState.InCashOut)
                {
                    return;
                }
                PreviousPlatformStateWriterLockHandler(value);                
            }
        }

        private void PreviousPlatformStateWriterLockHandler(RobotPlatformState value)
        {
            _previousPlatformStateLock.EnterWriteLock();
            try
            {
                if (_previousPlatformState != value)
                {
                    _previousPlatformState = value;
                    Logger.Info($"Transition PreviousPlatform to [{value}] state succeeded!");
                }
            }
            catch (Exception ex) {
                Logger.Error($"Transition PreviousPlatform to [{value}] state failed! " + ex.ToString());
                Enabled = false;
            }
            finally
            {
                _previousPlatformStateLock.ExitWriteLock();
            }
        }

        private RobotPlatformState PreviousPlatformStateReaderLockHandler()
        {
            _previousPlatformStateLock.EnterReadLock();
            try
            {
                return _previousPlatformState;
            }
            catch (Exception ex)
            {
                Logger.Error("PreviousPlatformStateReaderLockHandler failed: " + ex.ToString());
                return _previousPlatformState;
            }
            finally
            {
                _previousPlatformStateLock.ExitReadLock();
            }
        }

        private bool ValidateControlStateTransition(RobotControllerState newState)
        {
            if(_controlStateTransitionValidator.ContainsKey(newState)){
                var result = _controlStateTransitionValidator[newState]();
                _expectingRecovery = false;
                return result;
            }
            return true;
        }

        /// <inheritdoc />
        public bool Enabled
        {
            get => ControllerState != RobotControllerState.Disabled;

            set
            {
                if (Enabled != value)
                {
                    ControllerState = (value ? RobotControllerState.Starting : RobotControllerState.Disabled);
                    _automator.EnableExitToLobby(!value);
                    _automator.EnableCashOut(!value);
                    if (!value)
                    {
                        _automator.SetOverlayText("", true, _overlayTextGuid, InfoLocation.TopLeft);
                        _automator.ResetSpeed();
                    }
                    else
                    {
                        _automator.SetSpeed(_config.Speed);
                    }
                    LogInfo("RobotController is now " + (ControllerState != RobotControllerState.Disabled ? "enabled." : "disabled."));
                    _automator.IsRobotModeRunning = value;
                }
            }
        }
        
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            LogInfo("Initializing...");
            _pm = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _automator = new Automation(_pm, _eventBus);
            SubscribeToEvents();
            WaitForServices();
            InitializeControlStateTransitionValidator();
            LogInfo("Initialized");
        }

        private void InitializeControlStateTransitionValidator()
        {
            Func<bool> recoveryValidator = () =>  ExpectingRecovery || (_gameService != null && _gameService.Running);
            _controlStateTransitionValidator.Add(RobotControllerState.WaitForRecoveryStart, recoveryValidator);
            _controlStateTransitionValidator.Add(RobotControllerState.DriveRecovery, recoveryValidator);
            _controlStateTransitionValidator.Add(RobotControllerState.InsertCredits, () =>
                    ControllerState != RobotControllerState.InsertVoucherComplete &&
                    ControllerState != RobotControllerState.InsertCreditsComplete &&
                    ControllerState != RobotControllerState.OutOfOperatingHours &&
                    ControllerState != RobotControllerState.RequestGameLoad);
        }

        private void WaitForServices()
        {
            Task.Run(() =>
            {
                using (var serviceWaiter = new ServiceWaiter(_eventBus))
                {
                    serviceWaiter.AddServiceToWaitFor<IGameService>();
                    if (serviceWaiter.WaitForServices())
                    {
                        _gameService = ServiceManager.GetInstance().GetService<IGameService>();
                        LogInfo("Wait for services complete.");
                    }
                }
            });
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            LogInfo("Run Started");

            LoadConfiguration();

            while (RunState == RunnableState.Running)
            {
                Thread.Sleep(_config?.Active?.IntervalResolution ?? 100);

                try
                {
                    if (!Enabled)
                    {
                        if (_config.ActiveType == ModeType.Uber &&
                            PlatformState != RobotPlatformState.Unknown &&
                            ControllerState != RobotControllerState.WaitRebootStart)
                        {
                            Enabled = true;
                        }
                    }

                    if (Enabled &&
                        PlatformState != RobotPlatformState.InAudit &&
                        !_expectingAuditMenu)
                    {
                        IncrementCounters();

                        LogInfo("Idle time: " + _idleDuration);


                        if (TimeTo(_counter, _config.Active.IntervalRgSet))
                        {
                            _automator.SetResponsibleGamingTimeElapsed(_config.GetTimeElapsedOverride());

                            if (_config.GetSessionCountOverride() != 0)
                            {
                                _automator.SetRgSessionCountOverride(_config.GetSessionCountOverride());
                            }
                        }

                        switch (ControllerState)
                        {
                            case RobotControllerState.Starting:
                                {
                                    HandlerStarting();
                                    break;
                                }
                            case RobotControllerState.RequestGameLoad:
                                {
                                    HandlerRequestGameLoad();
                                    break;
                                }
                            case RobotControllerState.ForceGameExit:
                                {
                                    HandlerForceGameExit();
                                    break;
                                }
                            case RobotControllerState.RequestGameExit:
                                {
                                    HandlerRequestGameExit();
                                    break;
                                }
                            case RobotControllerState.WaitGameLoad:
                                {
                                    HandlerWaitGameLoad();
                                    break;
                                }
                            case RobotControllerState.Running:
                                {
                                    HandlerRunning();
                                    break;
                                }
                            case RobotControllerState.WaitLobbyLoad:
                                {
                                    HandlerWaitLobbyLoad();
                                    break;
                                }
                            case RobotControllerState.WaitForRecoveryStart:
                                {
                                    HandlerWaitForRecoveryStart();
                                    break;
                                }
                            case RobotControllerState.DriveRecovery:
                                {
                                    HandlerDriveRecovery();
                                    break;
                                }
                            case RobotControllerState.RecoveryComplete:
                                {
                                    HandlerRecoveryComplete();
                                    break;
                                }
                            case RobotControllerState.LoadAuditMenu:
                                {
                                    HandlerLoadAuditMenu();
                                    break;
                                }
                            case RobotControllerState.ExitAuditMenu:
                                {
                                    HandlerExitAuditMenu();
                                    break;
                                }
                            case RobotControllerState.EnterLockup:
                                {
                                    HandlerEnterLockup();
                                    break;
                                }
                            case RobotControllerState.ExitLockup:
                                {
                                    HandlerExitLockup();
                                    break;
                                }
                            case RobotControllerState.WaitCashOut:
                                {
                                    HandlerWaitCashOut();
                                    break;
                                }
                            case RobotControllerState.RequestSoftReboot:
                                {
                                    HandlerRequestSoftReboot();
                                    break;
                                }
                            case RobotControllerState.WaitRebootStart:
                                {
                                    //state placeholder so uber does not re-enable 
                                    break;
                                }
                            case RobotControllerState.SettingOperatingHours:
                                {
                                    HandlerSettingOperatingHours();
                                    break;
                                }
                            case RobotControllerState.OutOfOperatingHours:
                                {
                                    HandlerOutOfOperatingHours();
                                    break;
                                }
                            case RobotControllerState.InsertCredits:
                                {
                                    HandlerInsertCredits();
                                    break;
                                }
                            case RobotControllerState.InsertCreditsComplete:
                                {
                                    HandlerInsertCreditsComplete();
                                    break;
                                }
                            case RobotControllerState.InsertVoucher:
                                {
                                    HandlerInsertVoucher();
                                    break;
                                }
                            case RobotControllerState.InsertVoucherComplete:
                                {
                                    HandlerInsertVoucherComplete();
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
            }

            LogInfo("Run Stopped");
        }

        private void IncrementCounters()
        {
            try
            {
                _counter += _config.Active.IntervalResolution;
                _idleDuration += _config.Active.IntervalResolution;
            }
            catch (OverflowException ex)
            {
                LogInfo(ex.ToString());
                _counter = 0;
                _idleDuration = 0;
            }
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            LogInfo("Stopping");

            CancelEventSubscriptions();

            LogInfo("Stopped");
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<RobotControllerEnableEvent>(this, _ => { Enabled = !Enabled; });

            // If the runtime process hangs, and the setting to not kill it is active, then stop the robot. 
            // This will allow someone to attach a debugger to investigate.
            var doNotKillRuntime = _pm.GetValue("doNotKillRuntime", Common.Constants.False).ToUpperInvariant();
            if (doNotKillRuntime == Common.Constants.True)
            {
                _eventBus.Subscribe<GameProcessHungEvent>(this, _ => { Enabled = false; });
            }

            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, evt =>
            {
                //if we don't know what state the platform is in, we assume the platform is at the lobby if this is not caused by a disconnect
                //this event occurs during platform disconnect caused by a variety of reasons that should not change robot controller behavior
                if (PlatformState == RobotPlatformState.Unknown && evt.Online)
                {
                    PlatformState = RobotPlatformState.Lobby;
                }
            });

            _eventBus.Subscribe<VoucherRejectedEvent>(this, evt =>
            {
                if(Enabled)
                {
                    if (evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.CreditInLimitExceeded) ||
                        evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.LaundryLimitExceeded))
                    {
                        _eventBus.Publish(new CashOutButtonPressedEvent());
                    }
                }
            });

            _eventBus.Subscribe<LobbyInitializedEvent>(
                this,
                _ =>
                {
                    var protocol = _pm.GetValue(ApplicationConstants.ActiveProtocol, string.Empty);

                    if (protocol.Contains("Test") || protocol.Contains("Demonstration"))
                    {
                        if (PlatformState == RobotPlatformState.Unknown)
                        {
                            PlatformState = RobotPlatformState.Lobby;
                        }
                    }
                });

            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                this,
                evt =>
                {
                    _isTimeLimitDialogVisible = true;

                    if (evt.IsLastPrompt && Enabled)
                    {
                        _automator.EnableCashOut(true);
                        ControllerState = RobotControllerState.WaitCashOut;
                    }
                });


            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _isTimeLimitDialogVisible = false;
                });

            _eventBus.Subscribe<GameSelectedEvent>(
                this,
                evt =>
                {
                    {
                        var games = _pm.GetValues<IGameProfile>(GamingConstants.Games).ToList();
                        var gameInfo = games.FirstOrDefault(g => g.Id == evt.GameId);
                        _config.CurrentGame = gameInfo?.ThemeName;
                    }
                });

            _eventBus.Subscribe<GameExitedNormalEvent>(
                this,
                _ =>
                {
                    PlatformState = RobotPlatformState.GameExiting;
                });

            _eventBus.Subscribe<GameProcessExitedEvent>(
                this,
                _ =>
                {
                    _gameLoaded = false;
                    PlatformState = RobotPlatformState.Lobby;
                });

            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
                    if (_lobbyState != null && !_lobbyState.AllowSingleGameAutoLaunch)
                    {
                        _gameLoaded = false;
                        PlatformState = RobotPlatformState.Lobby;
                    }
                });

            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _gameLoaded = true;
                    PlatformState = PlatformState != RobotPlatformState.InRecovery
                        ? RobotPlatformState.GameLoaded
                        : RobotPlatformState.InRecovery;
                });

            _eventBus.Subscribe<GameIdleEvent>(
                this,
                _ =>
                {
                    BalanceCheck();

                    //we can get game idle after a game has exited
                    if (_gameLoaded)
                    {
                        PlatformState = RobotPlatformState.GameIdle;
                    }

                    _expectingRecovery = false;
                });

            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    if (!ExpectingLockup)
                    {
                        LogInfo("Entered audit menu unexpectedly. Disabling.");
                        Enabled = false;
                    }

                    if (_lobbyState != null && _lobbyState.AllowSingleGameAutoLaunch)
                    {
                        _expectingRecovery = true;
                    }

                    PlatformState = RobotPlatformState.InAudit;
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                    PlatformState = PreviousPlatformState;
                    ControllerState = PreviousControllerState;
                    _expectingAuditMenu = false;
                });

            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, _ => {
                PlatformState = PlatformState != RobotPlatformState.InRecovery
                    ? RobotPlatformState.GamePlaying
                    : RobotPlatformState.InRecovery;
                _idleDuration = 0;
            });

            _eventBus.Subscribe<SecondaryGameStartedEvent>(this, _ =>
            {
                PlatformState = PlatformState != RobotPlatformState.InRecovery
                    ? RobotPlatformState.GamePlaying
                    : RobotPlatformState.InRecovery;
                _idleDuration = 0;
            });

            _eventBus.Subscribe<FreeGameStartedEvent>(this, _ => {
                PlatformState = PlatformState != RobotPlatformState.InRecovery
                    ? RobotPlatformState.GamePlaying
                    : RobotPlatformState.InRecovery;
                _idleDuration = 0;
            });

            _eventBus.Subscribe<GamePresentationEndedEvent>(this, _ => {
                PlatformState = PlatformState != RobotPlatformState.InRecovery
                    ? RobotPlatformState.GamePlaying
                    : RobotPlatformState.InRecovery;
                _idleDuration = 0;
            });

            _eventBus.Subscribe<SystemDisableAddedEvent>(
                this,
                evt =>
                {
                    if (!Enabled)
                    {
                        return;
                    }

                    //We could have exited operating hours and an attempt to insert credits may occur
                    //inspect the previous controller state which should be set in the BalanceCheck call
                    if ((PreviousControllerState == RobotControllerState.OutOfOperatingHours ||
                        ControllerState == RobotControllerState.OutOfOperatingHours ||
                        ControllerState == RobotControllerState.SettingOperatingHours)
                        && evt.DisableReasons == "Outside Hours of Operation")
                    {
                        LogInfo("Not disabling because disable for Operating Hours is expected.");
                        ControllerState = RobotControllerState.OutOfOperatingHours;
                        return;
                    }

                    if (evt.DisableReasons.Contains("Disabled by the voucher"))
                    {
                        LogInfo("Not disabling for voucher device");
                        return;
                    }

                    if (evt.DisableReasons.Contains("Game Play Request Failed"))
                    {
                        LogInfo("Not disabling for game play request failed");
                        return;
                    }

                    if (evt.DisableReasons.Contains("Central Server Offline"))
                    {
                        LogInfo("Not disabling for central server offline");
                        return;
                    }

                    if (evt.DisableReasons.Contains("Protocol Initialization In Progress"))
                    {
                        LogInfo("Not disabling for protocol initialization");
                        return;
                    }

                    if (evt.DisableId == ApplicationConstants.LiveAuthenticationDisableKey)
                    {
                        LogInfo("Not disabling for signature verification");
                        return;
                    }

                    // Exempt voucher lockups from disabling robot mode because this will happen every hour
                    if (!ExpectingLockup && _config.Active.DisableOnLockup)
                    {
                        LogInfo($"Disabling for system disable {evt.DisableId}, reason: {evt.DisableReasons}");
                        Enabled = false;
                    }
                });

            _eventBus.Subscribe<OperatingHoursEnabledEvent>( this, _ => 
            {
                if (Enabled && ControllerState == RobotControllerState.OutOfOperatingHours)
                {
                    ControllerState = RobotControllerState.Running;
                }
            });

            _eventBus.Subscribe<RecoveryStartedEvent>(
                this,
                _ =>
                {
                    if (ExpectingRecovery || (_gameService != null && _gameService.Running))
                    {
                        PlatformState = RobotPlatformState.InRecovery;
                        ControllerState = RobotControllerState.DriveRecovery;
                        return;
                    }
                });

            _eventBus.Subscribe<CashOutStartedEvent>(this, _ =>
            {
                if (ControllerState == RobotControllerState.WaitCashOut)
                {
                    PlatformState = RobotPlatformState.InCashOut;
                }
            });

            _eventBus.Subscribe<VoucherIssuedEvent>(this, evt =>
            {
                if (ControllerState == RobotControllerState.WaitCashOut)
                {
                    _lastVoucherIssued = evt.Transaction;
                    PlatformState = PreviousPlatformState;
                }
            });

            _eventBus.Subscribe<TransferOutFailedEvent>(this, _ =>
            {
                if (ControllerState == RobotControllerState.WaitCashOut)
                {
                    PlatformState = PreviousPlatformState;
                }
            });

            _eventBus.Subscribe<CashOutAbortedEvent>(this, _ =>
            {
                if (ControllerState == RobotControllerState.WaitCashOut)
                {
                    PlatformState = PreviousPlatformState;
                }
                else
                {
                    LogError("CashOutAbortedEvent raised. Disabling.");
                    Enabled = false;
                }
            });

            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, _ =>
            {
                if (ControllerState == RobotControllerState.WaitCashOut)
                {
                    PlatformState = PreviousPlatformState;
                }
            });

            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if (Enabled &&
                    (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit))
                {
                    LogInfo("Keying off large win");
                    PlatformState = RobotPlatformState.InCashOut;
                    ControllerState = RobotControllerState.WaitCashOut;
                    //Task.Delay(3000).ContinueWith(_ => _eventBus.Publish(new RemoteKeyOffEvent(KeyOffType.LocalHandpay, evt.CashableAmount, evt.PromoAmount, evt.NonCashAmount)));
                    Task.Delay(1000).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_=> _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
                }
            });

            _eventBus.Subscribe<TransferOutCompletedEvent>(this, evt =>
            {
                if(_previousVoucherBalance.Amount == evt.CashableAmount &&
                    evt.Timestamp.Subtract(_previousVoucherBalance.TimeStamp).TotalSeconds < Constants.DuplicateVoucherWindow)
                {
                    LogFatal("Possible identical vouchers detected. Disabling.");
                    Enabled = false;
                }

                _previousVoucherBalance.TimeStamp = evt.Timestamp;
                _previousVoucherBalance.Amount = evt.CashableAmount;
            });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(this, _ => 
            {
                LogInfo("Keying off GamePlayRequestFailed");
                ToggleJackpotKey(1000);
            }, _ => Enabled);

            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(this, _ =>
            {
                LogInfo("Keying off UnexpectedOrNoResponseEvent");
                ToggleJackpotKey(10000);
            }, _ => Enabled);

            _eventBus.Subscribe<ExitRequestedEvent>(this, _ =>
            {
                LogInfo("Exit requested. Disabling.");
                Enabled = false;
            });
        }

        private void ToggleJackpotKey(int waitDuration)
        {
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }

        private void CancelEventSubscriptions()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private void ActionLobby(long counter)
        {
            if (_config.Active.Type == ModeType.Super || _config.ActiveType == ModeType.Uber)
            {
                if (TimeTo(counter, _config.Active.IntervalLoadGame) &&
                    (PlatformState == RobotPlatformState.GameLoaded ||
                    PlatformState == RobotPlatformState.GamePlaying ||
                    PlatformState == RobotPlatformState.GameIdle))
                {
                    _config.SelectNextGame();
                    _automator.EnableExitToLobby(true);
                    ControllerState = (_config.Active.TestRecovery ? RobotControllerState.ForceGameExit : RobotControllerState.DriveRecovery);
                    return;
                }

                if (_config.Active.IntervalLoadGame != 0 &&
                    PlatformState == RobotPlatformState.Lobby &&
                    ControllerState != RobotControllerState.OutOfOperatingHours)
                {
                    LogInfo("Queueing game load");
                    _config.SelectNextGame();
                    _automator.EnableExitToLobby(false);
                    ControllerState = (RobotControllerState.RequestGameLoad);
                }
            }
        }

        private void MessageSwarm()
        {
            int i = 0;
            while(i <= _config.Active.LogMessageLoadTestSize)
            {
                // Call to seperate method to determine message size before sending
                LogInfo("Log message swarm " + i);
                i++;
            }
            return;
        }

        private void ActionPlayer()
        {
            if(ControllerState != RobotControllerState.Running){
                return;
            }
            Random Rng = new System.Random((int)DateTime.Now.Ticks);
            var action =
                _config.CurrentGameProfile.RobotActions.ElementAt(Rng.Next(_config.CurrentGameProfile.RobotActions.Count));

            switch (action)
            {
                case Actions.SpinRequest:
                {
                    if(_config.Active.LogMessageLoadTest)
                    {
                        MessageSwarm();
                    }
                    LogInfo("Spin Request");
                    _automator.SpinRequest();
                    break;
                }
                case Actions.BetLevel:
                {
                    LogInfo("Changing bet level");

                    //increment/decrement bet level - physical id: 23 - 27
                    var betIndices = _config.GetBetIndices();

                    var index = Math.Min(betIndices[Rng.Next(betIndices.Count)], 5);

                    _automator.SetBetLevel(index);

                    break;
                }
                case Actions.BetMax:
                {
                    LogInfo("Bet Max");
                    _automator.SetBetMax();
                    break;
                }
                case Actions.LineLevel:
                {
                    LogInfo("Change Line Level");
                    // increment/decrement line level - physical id: 30 - 34
                    var lineIndices = _config.GetLineIndices();
                    _automator.SetLineLevel(lineIndices[Rng.Next(lineIndices.Count)]);
                    break;
                }
            }
        }

        private void ActionTouch()
        {
            Random Rng = new System.Random((int)DateTime.Now.Ticks);
            // touch game screen
            var x = Rng.Next(_config.GameScreen.Width);
            var y = Rng.Next(_config.GameScreen.Height);
            if (CheckDeadZones(_config.CurrentGameProfile.MainTouchDeadZones, x, y))
            {
                _automator.TouchMainScreen(x, y);
            }

            // touch VBD
            x = Rng.Next(_config.VirtualButtonDeck.Width);
            y = Rng.Next(_config.VirtualButtonDeck.Height);
            if (CheckDeadZones(_config.CurrentGameProfile.VbdTouchDeadZones, x, y))
            {
                _automator.TouchVBD(x, y);
            }

            if (_config.CurrentGameProfile != null)
            {
                // touch any auxiliary main screen areas configured
                foreach (var tb in _config.CurrentGameProfile.ExtraMainTouchAreas)
                {
                    try
                    {
                        x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                        y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                        if (CheckDeadZones(_config.CurrentGameProfile.MainTouchDeadZones, x, y))
                        {
                            _automator.TouchMainScreen(x, y);
                        }
                    }
                    catch(Exception ex)
                    {
                        LogInfo("Error while using Extra Touch Areas: " + ex);
                    }
                }

                // touch any auxiliary VBD areas configured
                foreach (var tb in _config.CurrentGameProfile.ExtraVbdTouchAreas)
                {
                    try
                    {
                        x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                        y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                        if (CheckDeadZones(_config.CurrentGameProfile.VbdTouchDeadZones, x, y))
                        {
                            _automator.TouchVBD(x, y);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogInfo("Error while using Extra Touch VBD Areas: " + ex);
                    }
                }
            }
        }

        private bool CheckDeadZones(List<TouchBoxes> deadZones, int x, int y)
        {
            foreach (TouchBoxes tb in _config.CurrentGameProfile.MainTouchDeadZones)
            {
                if (x >= tb.TopLeftX && x <= tb.BottomRightX && y >= tb.TopLeftY && y <= tb.BottomRightY)
                {
                    LogInfo($"NOT touching ({x}, {y}) as it is in a deadzone");
                    return false;
                }
            }

            return true;
        }

        private void BalanceCheck([CallerMemberName] string caller = null)
        {
            if (Enabled && !ExpectingRecovery)
            {
                if (_bank == null)
                {
                    _bank = ServiceManager.GetInstance().GetService<IBank>();
                }

                if (_bank == null)
                {
                    LogInfo("BalanceCheck. _bank is null);");
                    return;
                }

                LogInfo("BalanceCheck. Caller: " + caller);

                long minBalance = _config.GetMinimumBalance();

                long balance = _bank.QueryBalance();

                LogInfo($"Platform balance: {balance}");

                if(balance < 0)
                {
                    LogFatal("NEGATIVE BALANCE DETECTED");
                    Enabled = false;
                    return;
                }

                if (balance <= minBalance * 1000)
                {
                    LogInfo($"Insufficient balance.  Balance: {balance}, Minimum Balance: {minBalance * 1000}");

                    //inserting credits can lead to race conditions that make the platform not update the runtime balance
                    //we now support inserting credits during game round for some jurisdictions
                    if (_config?.Active?.InsertCreditsDuringGameRound == true)
                    {
                        _automator.InsertDollars(_config.GetDollarsInserted());
                    }
                    else if (ControllerState != RobotControllerState.InsertCredits &&
                        ControllerState != RobotControllerState.InsertCreditsComplete &&
                        ControllerState != RobotControllerState.OutOfOperatingHours)
                    {
                        PreviousControllerState = ControllerState;

                        //using a state for this to isolate credits entry
                        ControllerState = (RobotControllerState.InsertCredits);
                    }
                }
            }
        }

        private bool RequestGameLoad()
        {
            var result = false;

            var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();

            var gameInfo = games.FirstOrDefault(g => g.ThemeName == _config.CurrentGame && g.Enabled);

            if (gameInfo != null)
            {
                var denom = gameInfo.Denominations.First(d => d.Active == true).Value;

                LogInfo($"Requesting game {gameInfo.ThemeName} with denom {denom} be loaded.");

                _automator.RequestGameLoad(gameInfo.Id, denom);

                result = true;
            }
            else
            {
                LogInfo($"Did not find game, {_config.CurrentGame}");

                _config.SelectNextGame();
            }

            return result;
        }

        private void SetOperatingHours()
        {
            LogInfo($"Setting operating hours to timeout in 3 seconds for {_config.Active.OperatingHoursDisabledDuration} milliseconds");

            DateTime soon = DateTime.Now.AddSeconds(3);

            DateTime then = soon.AddMilliseconds(Math.Max(_config.Active.OperatingHoursDisabledDuration, 100));

            List<OperatingHours> updatedOperatingHours = new List<OperatingHours>()
            {
                new OperatingHours {Day = soon.DayOfWeek, Enabled = false, Time = (int)soon.TimeOfDay.TotalMilliseconds },
                new OperatingHours {Day = then.DayOfWeek, Enabled = true, Time = (int)then.TimeOfDay.TotalMilliseconds }
            };

            _pm.SetProperty(ApplicationConstants.OperatingHours, updatedOperatingHours);
        }

        private void LoadConfiguration()
        {
            var configPath = Path.Combine(
                ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(HardwareConstants.DataPath).FullName,
                Constants.ConfigurationFileName);

            LogInfo("Loading configuration: " + configPath);

            _config = Configuration.Load(configPath);

            if (_config != null)
            {
                _automator.SetTimeLimitButtons(_config.GetTimeLimitButtons());
                LogInfo(_config.ToString());
            }
        }

        private void LogInfo(string msg)
        {
            Logger.Info($"Controller: {ControllerState} - Platform: {PlatformState} - {msg}");
        }

        private void LogError(string msg)
        {
            Logger.Error($"Controller: {ControllerState} - Platform: {PlatformState} - {msg}");
        }

        private void LogFatal(string msg)
        {
            Logger.Fatal($"Controller: {ControllerState} - Platform: {PlatformState} - {msg}");
        }
    }
}
