namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts;
    using Application.Contracts.HardwareDiagnostics;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Events;
    using Gaming.Contracts;
    using Gaming.Diagnostics;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;
    using log4net;
    using Stateless;

    public class CurrentMachineModeStateManager : ICurrentMachineModeStateManager
    {
        private enum MachineModeTrigger
        {
            GameInIdle,
            GamePlayInProgress,
            OperatorMenuEntered,
            OperatorMenuExited,
            FatalError,
            SystemDisabledByOperator,
            SystemEnabledByOperator,
            DiagnosticTestInitiated,
            DiagnosticTestCompleted,
            GameReplayInitiated,
            GameReplayCompleted,
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        //Disable keys that represent a fatal error state
        private static readonly List<Guid> FatalErrorKeys = new List<Guid>
        {
            ApplicationConstants.StorageFaultDisableKey
        };

        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlayState;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IGameHistory _gameHistory;
        private readonly IDisableByOperatorManager _disableByOperatorManager;
        private readonly StateMachine<MachineMode, MachineModeTrigger> _state;
        private readonly ReaderWriterLockSlim _stateLock;

        private bool _disposed;

        public CurrentMachineModeStateManager(
            IEventBus eventBus,
            IGamePlayState gamePlayState,
            ISystemDisableManager disableManager,
            IGameHistory gameHistory,
            IDisableByOperatorManager disableByOperatorManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _systemDisableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _disableByOperatorManager = disableByOperatorManager ?? throw new ArgumentNullException(nameof(disableByOperatorManager));

            _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            // should thread safe due to access only class members
            _state = new StateMachine<MachineMode, MachineModeTrigger>(MachineMode.NotInOperation);
            CreateStateMachine();

            SubscribeToEvents(_eventBus);
        }

        private void CreateStateMachine()
        {
            _state.Configure(MachineMode.NotInOperation)
                .PermitIf(MachineModeTrigger.GameInIdle, MachineMode.Idle)
                .Permit(MachineModeTrigger.GamePlayInProgress, MachineMode.GameInProgress)
                .Permit(MachineModeTrigger.SystemDisabledByOperator, MachineMode.EgmOutOfService)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.Idle)
                .Ignore(MachineModeTrigger.GameInIdle)
                .Ignore(MachineModeTrigger.OperatorMenuExited) // OperatorMenuExited could be sent few times
                .Ignore(MachineModeTrigger.DiagnosticTestCompleted)
                .Permit(MachineModeTrigger.GamePlayInProgress, MachineMode.GameInProgress)
                .Permit(MachineModeTrigger.OperatorMenuEntered, MachineMode.AuditMode)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.GameInProgress)
                .Ignore(MachineModeTrigger.GamePlayInProgress)
                .Ignore(MachineModeTrigger.OperatorMenuExited) // OperatorMenuExited could be sent few times
                .Ignore(MachineModeTrigger.DiagnosticTestCompleted)
                .Permit(MachineModeTrigger.GameInIdle, MachineMode.Idle)
                .Permit(MachineModeTrigger.OperatorMenuEntered, MachineMode.AuditMode) // back up if operator menu is open
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.EgmOutOfService)
                .Ignore(MachineModeTrigger.SystemDisabledByOperator)
                .Ignore(MachineModeTrigger.OperatorMenuExited)
                .Permit(MachineModeTrigger.OperatorMenuEntered, MachineMode.AuditMode)
                .Permit(MachineModeTrigger.DiagnosticTestInitiated, MachineMode.DiagnosticTest)
                .Permit(MachineModeTrigger.GameReplayInitiated, MachineMode.GameReplayActive)
                .Permit(MachineModeTrigger.SystemEnabledByOperator, MachineMode.AuditMode)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.AuditMode)
                .Ignore(MachineModeTrigger.OperatorMenuEntered)
                .Ignore(MachineModeTrigger.SystemEnabledByOperator)
                .IgnoreIf(MachineModeTrigger.DiagnosticTestCompleted)
                .PermitIf(MachineModeTrigger.OperatorMenuExited, MachineMode.Idle, () => !_disableByOperatorManager.DisabledByOperator && _gamePlayState.Idle)
                .PermitIf(MachineModeTrigger.OperatorMenuExited, MachineMode.GameInProgress, () => !_disableByOperatorManager.DisabledByOperator && _gamePlayState.InGameRound)
                .PermitIf(MachineModeTrigger.OperatorMenuExited, MachineMode.EgmOutOfService, () => _disableByOperatorManager.DisabledByOperator)
                .Permit(MachineModeTrigger.DiagnosticTestInitiated, MachineMode.DiagnosticTest)
                .Permit(MachineModeTrigger.GameReplayInitiated, MachineMode.GameReplayActive)
                .Permit(MachineModeTrigger.SystemDisabledByOperator, MachineMode.EgmOutOfService)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.DiagnosticTest)
                .Ignore(MachineModeTrigger.DiagnosticTestInitiated)
                .PermitIf(MachineModeTrigger.DiagnosticTestCompleted, MachineMode.AuditMode, () => !_disableByOperatorManager.DisabledByOperator)
                .PermitIf(MachineModeTrigger.DiagnosticTestCompleted, MachineMode.EgmOutOfService, () => _disableByOperatorManager.DisabledByOperator)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.GameReplayActive)
                .Ignore(MachineModeTrigger.GameReplayInitiated)
                .PermitIf(MachineModeTrigger.GameReplayCompleted, MachineMode.AuditMode, () => !_disableByOperatorManager.DisabledByOperator)
                .PermitIf(MachineModeTrigger.GameReplayCompleted, MachineMode.EgmOutOfService, () => _disableByOperatorManager.DisabledByOperator)
                .Permit(MachineModeTrigger.FatalError, MachineMode.FatalError)
                ;

            _state.Configure(MachineMode.FatalError)
                .OnEntry(() =>
                {
                    _eventBus.UnsubscribeAll(this);
                })
                .Ignore(MachineModeTrigger.FatalError)
                .Ignore(MachineModeTrigger.SystemEnabledByOperator)
                .Ignore(MachineModeTrigger.SystemDisabledByOperator)
                .Ignore(MachineModeTrigger.OperatorMenuEntered)
                .Ignore(MachineModeTrigger.OperatorMenuExited)
                .Ignore(MachineModeTrigger.DiagnosticTestInitiated)
                .Ignore(MachineModeTrigger.DiagnosticTestCompleted)
                .Ignore(MachineModeTrigger.GameReplayInitiated)
                .Ignore(MachineModeTrigger.GameReplayCompleted)
                .Ignore(MachineModeTrigger.GameInIdle)
                .Ignore(MachineModeTrigger.GamePlayInProgress)
                ;
            _state.OnTransitioned(
                transition =>
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(
                            $"Transitioned From : {transition.Source} To: '{transition.Destination}' with Trigger: '{transition.Trigger}'");
                    }
                    NotifyCurrentStateChanged(transition.Destination);
                });

            _state.OnUnhandledTrigger((state, trigger) =>
                {
                    Log.Error(
                        $"Invalid Current Machine Model State Transition. State: '{state}' Trigger: '{trigger}'");
                });
        }

        private void NotifyCurrentStateChanged(MachineMode machineMode)
        {
            MachineModeChanged?.Invoke(this, machineMode);
        }

        private void SubscribeToEvents(IEventBus eventBus)
        {
            eventBus.Subscribe<AspClientStartedEvent>(this, HandleEvent);
            eventBus.Subscribe<GamePlayStateChangedEvent>(this, HandleEvent);
            eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, HandleEvent);
            eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, HandleEvent);
            eventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleEvent);
            eventBus.Subscribe<OperatorMenuExitedEvent>(this, HandleEvent);
            eventBus.Subscribe<HardwareDiagnosticTestStartedEvent>(this, HandleEvent);
            eventBus.Subscribe<HardwareDiagnosticTestFinishedEvent>(this, HandleEvent);
        }

        public MachineMode GetCurrentMode()
        {
            _stateLock.EnterReadLock();
            try
            {
                return _state.State;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }

        public void HandleEvent(InitializationCompletedEvent @event)
        {
            _eventBus.Unsubscribe<InitializationCompletedEvent>(this);
            if (IsFatalError())
            {
                TransitState(MachineModeTrigger.FatalError);
            }
            else if (_disableByOperatorManager.DisabledByOperator)
            {
                TransitState(MachineModeTrigger.SystemDisabledByOperator);
            }
            else if (_gamePlayState.InGameRound || _gameHistory.IsRecoveryNeeded)
            {
                TransitState(MachineModeTrigger.GamePlayInProgress);
            }
            else
            {
                TransitState(MachineModeTrigger.GameInIdle);
            }
        }

        private void HandleEvent(AspClientStartedEvent @event)
        {
            _eventBus.Unsubscribe<AspClientStartedEvent>(this);
            NotifyCurrentStateChanged(MachineMode.NotInOperation);
        }

        public void HandleEvent(PersistentStorageIntegrityCheckFailedEvent @event)
        {
            if (IsFatalError())
            {
                TransitState(MachineModeTrigger.FatalError);
            }
        }

        public void HandleEvent(StorageErrorEvent @event)
        {
            if (IsFatalError())
            {
                TransitState(MachineModeTrigger.FatalError);
            }
        }

        private void HandleEvent(OperatorMenuEnteredEvent @event)
        {
            TransitState(MachineModeTrigger.OperatorMenuEntered);
        }

        public void HandleEvent(SystemEnabledByOperatorEvent @event)
        {
            TransitState(MachineModeTrigger.SystemEnabledByOperator);
        }

        public void HandleEvent(SystemDisabledByOperatorEvent @event)
        {
            TransitState(MachineModeTrigger.SystemDisabledByOperator);
        }

        private void HandleEvent(GamePlayStateChangedEvent @event)
        {
            TransitState(
                _gamePlayState.InGameRound ? MachineModeTrigger.GamePlayInProgress : MachineModeTrigger.GameInIdle);
        }

        private void HandleEvent(GameDiagnosticsStartedEvent @event)
        {
            if (@event.Context is ReplayContext)
            {
                TransitState(MachineModeTrigger.GameReplayInitiated);
            }
            else
            {
                TransitState(MachineModeTrigger.DiagnosticTestInitiated);
            }
        }

        private void HandleEvent(GameDiagnosticsCompletedEvent @event)
        {
            if (@event.Context is ReplayContext)
            {
                TransitState(MachineModeTrigger.GameReplayCompleted);
            }
            else
            {
                TransitStateIf(MachineModeTrigger.DiagnosticTestCompleted, MachineMode.DiagnosticTest);
            }
        }

        private void HandleEvent(HardwareDiagnosticTestStartedEvent @event)
        {
            TransitState(MachineModeTrigger.DiagnosticTestInitiated);
        }

        private void HandleEvent(HardwareDiagnosticTestFinishedEvent @event)
        {
            TransitStateIf(MachineModeTrigger.DiagnosticTestCompleted, MachineMode.DiagnosticTest);
        }

        private void HandleEvent(OperatorMenuExitedEvent @event)
        {
            TransitStateIf(MachineModeTrigger.DiagnosticTestCompleted, MachineMode.DiagnosticTest);

            TransitStateIf(MachineModeTrigger.GameReplayCompleted, MachineMode.GameReplayActive);

            TransitState(MachineModeTrigger.OperatorMenuExited);
        }

        private bool IsFatalError()
        {
            return _systemDisableManager.CurrentDisableKeys.Any(a => FatalErrorKeys.Contains(a));
        }

        private void TransitState(MachineModeTrigger trigger)
        {
            TransitStateIf(trigger, null);
        }

        private void TransitStateIf(MachineModeTrigger trigger, MachineMode? inState)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (inState == null || _state.IsInState(inState.GetValueOrDefault()))
                {
                    _state.Fire(trigger);
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _stateLock.Dispose();
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        public event EventHandler<MachineMode> MachineModeChanged;
    }
}