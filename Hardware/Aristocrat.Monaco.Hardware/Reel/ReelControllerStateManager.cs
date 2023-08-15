namespace Aristocrat.Monaco.Hardware.Reel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Stateless;

    internal class ReelControllerStateManager : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly IReadOnlyList<ReelLogicalState> NonIdleStates = new List<ReelLogicalState>
        {
            ReelLogicalState.Spinning,
            ReelLogicalState.Homing,
            ReelLogicalState.Stopping,
            ReelLogicalState.SpinningBackwards,
            ReelLogicalState.SpinningForward,
            ReelLogicalState.SpinningConstant,
            ReelLogicalState.Accelerating,
            ReelLogicalState.Decelerating,
            ReelLogicalState.Tilted
        };

        private readonly IEventBus _eventBus;
        private readonly Func<bool> _isControllerEnabledCallback;
        private readonly StateMachine<ReelControllerState, ReelControllerTrigger> _state;
        private readonly ConcurrentDictionary<int, StateMachineWithStoppingTrigger> _reelStates = new();
        private readonly ReaderWriterLockSlim _stateLock = new(LockRecursionPolicy.SupportsRecursion);

        private bool _disposed;

        public ReelControllerStateManager(IEventBus eventBus, int controllerId, Func<bool> isEnabledCallback)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _isControllerEnabledCallback = isEnabledCallback;
            _state = CreateStateMachine();
            ControllerId = controllerId;
        }

        public int ControllerId { set; get; }

        public IReadOnlyCollection<int> ConnectedReels => ReelStates
            .Where(x => x.Value != ReelLogicalState.Disconnected)
            .Select(x => x.Key)
            .ToList();

        public IReadOnlyDictionary<int, ReelLogicalState> ReelStates
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _reelStates.ToDictionary(x => x.Key, x => x.Value.StateMachine.State);
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        public ReelControllerState LogicalState
        {
            get
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
        }

        public bool CanSendCommand => LogicalState is
            not ReelControllerState.Uninitialized and
            not ReelControllerState.IdleUnknown and
            not ReelControllerState.Inspecting and
            not ReelControllerState.Disabled and
            not ReelControllerState.Homing;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = disposing;
            if (disposing)
            {
                _stateLock.Dispose();
            }
        }

        public void HandleReelControllerDisconnected(Action<DisabledReasons> disableMethod)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.Clear();
                disableMethod(DisabledReasons.Device);
                Fire(ReelControllerTrigger.Disconnected, new DisconnectedEvent(ControllerId));
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public void HandleReelControllerSlowSpinning(ReelEventArgs e)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!_reelStates.TryGetValue(e.ReelId, out _))
                {
                    Logger.Warn($"ReelControllerSlowSpinning Ignoring event for invalid reel: {e.ReelId}");
                    return;
                }

                Fire(ReelControllerTrigger.TiltReels, e.ReelId);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public void HandleReelConnected(ReelEventArgs e)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.AddOrUpdate(
                    e.ReelId,
                    i =>
                    {
                        _eventBus?.Publish(new ReelConnectedEvent(i, ControllerId));
                        return CreateReelStateMachine();
                    },
                    (i, s) =>
                    {
                        Fire(ReelControllerTrigger.Connected, i, new ReelConnectedEvent(i, ControllerId), false);
                        return s;
                    });
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public void HandleReelDisconnected(ReelEventArgs e)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _reelStates.AddOrUpdate(
                    e.ReelId,
                    i =>
                    {
                        _eventBus?.Publish(new ReelDisconnectedEvent(i, ControllerId));
                        return CreateReelStateMachine(ReelLogicalState.Disconnected);
                    },
                    (i, s) =>
                    {
                        Fire(ReelControllerTrigger.Disconnected, i, new ReelDisconnectedEvent(i, ControllerId), false);
                        return s;
                    });
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool CanFire(ReelControllerTrigger trigger)
        {
            _stateLock.EnterReadLock();
            try
            {
                var canFire = _state.CanFire(trigger);
                if (!canFire)
                {
                    Logger.Debug($"CanFire - FAILED for trigger {trigger} in state {_state.State}");
                }

                return canFire;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
        }

        public bool Fire(ReelControllerTrigger trigger)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!CanFire(trigger))
                {
                    Logger.Debug($"Fire - FAILED CanFire for trigger {trigger}");
                    return false;
                }

                _state.Fire(trigger);
                return true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool Fire<TEvent>(ReelControllerTrigger trigger, TEvent @event)
            where TEvent : BaseEvent
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!Fire(trigger))
                {
                    Logger.Debug($"Fire - FAILED for trigger {trigger}");
                    return false;
                }

                if (@event != null)
                {
                    _eventBus?.Publish(@event);
                }

                return true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool Fire(ReelControllerTrigger trigger, int reelId, bool updateControllerState = true)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var canFire = _reelStates.TryGetValue(reelId, out var reelState);
                if (!canFire)
                {
                    Logger.Debug($"Fire - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    return false;
                }
                else
                {
                    canFire = CanFire(trigger, reelId, updateControllerState);
                    if (!canFire)
                    {
                        Logger.Debug($"Fire - FAILED CanFire for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        return false;
                    }
                    else if (updateControllerState)
                    {
                        canFire = CanFire(trigger);
                        if (!canFire)
                        {
                            Logger.Debug($"Fire - FAILED CanFire for trigger {trigger} with reelState {reelState.StateMachine} in state {_state.State}");
                            return false;
                        }
                    }
                }

                reelState.StateMachine.Fire(trigger);
                return !updateControllerState || Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool Fire(IEnumerable<(ReelControllerTrigger trigger, int reelId)> reelTriggers, bool updateControllerState = true)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var reelData = reelTriggers.ToList();
                return reelData.All(x => CanFire(x.trigger, x.reelId, updateControllerState)) &&
                       reelData.All(x => Fire(x.trigger, x.reelId, updateControllerState));
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool FireAll(ReelControllerTrigger trigger)
        {
            _stateLock.EnterWriteLock();
            try
            {
                var reels = ConnectedReels;
                var fireAll = reels.All(reel => CanFire(trigger, reel)) &&
                              reels.All(reel => Fire(trigger, reel)) &&
                              (reels.Any() || Fire(trigger)); // If we have reels don't transition the state it is handled above

                if (!fireAll)
                {
                    Logger.Debug($"FireAll - FAILED for trigger {trigger}");
                }

                return fireAll;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public bool FireReelStopped(ReelControllerTrigger trigger, int reelId, ReelEventArgs args)
        {
            Logger.Debug($"FireReelStopped with trigger {trigger} for reel {reelId}");
            _stateLock.EnterWriteLock();
            try
            {
                var canFire = _reelStates.TryGetValue(reelId, out var reelState);
                if (!canFire)
                {
                    Logger.Debug($"FireReelStopped - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    return false;
                }
                else
                {
                    canFire = CanFire(trigger, reelId);
                    if (!canFire)
                    {
                        Logger.Debug($"FireReelStopped - FAILED CanFire for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        return false;
                    }
                }

                Logger.Debug($"Stopping with trigger {reelState.StoppingTrigger} from state {reelState.StateMachine.State}");
                reelState.StateMachine.Fire(reelState.StoppingTrigger, reelState.StateMachine.State, args);
                return Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private StateMachineWithStoppingTrigger CreateReelStateMachine(ReelLogicalState state = ReelLogicalState.IdleUnknown)
        {
            var stateMachine = new StateMachine<ReelLogicalState, ReelControllerTrigger>(state);
            var reelStoppedTriggerParameters = stateMachine.SetTriggerParameters<ReelLogicalState, ReelEventArgs>(ReelControllerTrigger.ReelStopped);

            stateMachine.Configure(ReelLogicalState.IdleUnknown)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .Ignore(ReelControllerTrigger.Connected);

            stateMachine.Configure(ReelLogicalState.IdleAtStop)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted)
                .Permit(ReelControllerTrigger.SpinReel, ReelLogicalState.SpinningForward)
                .Permit(ReelControllerTrigger.SpinReelBackwards, ReelLogicalState.SpinningBackwards)
                .Permit(ReelControllerTrigger.SpinConstant, ReelLogicalState.SpinningConstant)
                .Permit(ReelControllerTrigger.Accelerate, ReelLogicalState.Accelerating)
                .Permit(ReelControllerTrigger.Decelerate, ReelLogicalState.Decelerating)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .OnEntryFrom(reelStoppedTriggerParameters, (logicalState, args) => _eventBus?.Publish(new ReelStoppedEvent(args.ReelId, args.Step, logicalState == ReelLogicalState.Homing)));

            stateMachine.Configure(ReelLogicalState.Tilted)
                .Ignore(ReelControllerTrigger.TiltReels)
                .Permit(ReelControllerTrigger.HomeReels, ReelLogicalState.Homing)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected);

            stateMachine.Configure(ReelLogicalState.Spinning)
                .Permit(ReelControllerTrigger.Disconnected, ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.ReelStopped, ReelLogicalState.IdleAtStop)
                .Permit(ReelControllerTrigger.TiltReels, ReelLogicalState.Tilted);

            stateMachine.Configure(ReelLogicalState.Homing)
                .SubstateOf(ReelLogicalState.Spinning);

            stateMachine.Configure(ReelLogicalState.Disconnected)
                .Permit(ReelControllerTrigger.Connected, ReelLogicalState.IdleUnknown);

            stateMachine.Configure(ReelLogicalState.SpinningForward).SubstateOf(ReelLogicalState.Spinning);

            stateMachine.Configure(ReelLogicalState.SpinningBackwards).SubstateOf(ReelLogicalState.Spinning);
            
            stateMachine.Configure(ReelLogicalState.SpinningConstant).SubstateOf(ReelLogicalState.Spinning)
                .Permit(ReelControllerTrigger.Accelerate, ReelLogicalState.Accelerating)
                .Permit(ReelControllerTrigger.Decelerate, ReelLogicalState.Decelerating);

            stateMachine.Configure(ReelLogicalState.Accelerating).SubstateOf(ReelLogicalState.Spinning)
                .Permit(ReelControllerTrigger.Decelerate, ReelLogicalState.Decelerating)
                .Permit(ReelControllerTrigger.SpinConstant, ReelLogicalState.SpinningConstant);

            stateMachine.Configure(ReelLogicalState.Decelerating).SubstateOf(ReelLogicalState.Spinning)
                .Permit(ReelControllerTrigger.Accelerate, ReelLogicalState.Accelerating)
                .Permit(ReelControllerTrigger.SpinConstant, ReelLogicalState.SpinningConstant);

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"ReelStateMachine(Reel) - Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            stateMachine.OnUnhandledTrigger(
                (unHandledState, trigger) =>
                {
                    Logger.Error($"ReelStateMachine(Reel) - Invalid State {unHandledState} For Trigger {trigger}");
                });

            return new StateMachineWithStoppingTrigger(stateMachine, reelStoppedTriggerParameters);
        }

        private StateMachine<ReelControllerState, ReelControllerTrigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<ReelControllerState, ReelControllerTrigger>(ReelControllerState.Uninitialized);
            stateMachine.Configure(ReelControllerState.Uninitialized)
                .Permit(ReelControllerTrigger.Inspecting, ReelControllerState.Inspecting)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled)
                .PermitDynamic(
                    ReelControllerTrigger.Initialized,
                    () => _isControllerEnabledCallback() ? ReelControllerState.IdleUnknown : ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.Inspecting)
                .Ignore(ReelControllerTrigger.Disable)
                .Ignore(ReelControllerTrigger.ReelStopped)
                .Permit(ReelControllerTrigger.InspectionFailed, ReelControllerState.Uninitialized)
                .PermitDynamic(
                    ReelControllerTrigger.Initialized,
                    () => _isControllerEnabledCallback() ? ReelControllerState.IdleUnknown : ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected);

            stateMachine.Configure(ReelControllerState.IdleUnknown)
                .Ignore(ReelControllerTrigger.Connected)
                .Ignore(ReelControllerTrigger.Enable)
                .Ignore(ReelControllerTrigger.Initialized)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.IdleAtStops)
                .Ignore(ReelControllerTrigger.Connected)
                .Ignore(ReelControllerTrigger.Enable)
                .Ignore(ReelControllerTrigger.Initialized)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.SpinReel, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.SpinReelBackwards, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.SpinConstant, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.Accelerate, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.Decelerate, ReelControllerState.Spinning)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled);

            stateMachine.Configure(ReelControllerState.Homing)
                .PermitReentry(ReelControllerTrigger.HomeReels)
                .Permit(ReelControllerTrigger.Disable, ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .PermitDynamic(
                    ReelControllerTrigger.ReelStopped,
                    () => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.Homing)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted);

            stateMachine.Configure(ReelControllerState.Tilted)
                .SubstateOf(ReelControllerState.Disabled)
                .Permit(ReelControllerTrigger.HomeReels, ReelControllerState.Homing);

            stateMachine.Configure(ReelControllerState.Disabled)
                .Ignore(ReelControllerTrigger.ReelStopped)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .Ignore(ReelControllerTrigger.Disable)
                .PermitDynamic(
                    ReelControllerTrigger.Enable,
                    GetEnabledState);

            stateMachine.Configure(ReelControllerState.Disconnected)
                .Permit(ReelControllerTrigger.Connected, ReelControllerState.Inspecting);

            stateMachine.Configure(ReelControllerState.Spinning)
                .PermitReentry(ReelControllerTrigger.SpinReel)
                .PermitReentry(ReelControllerTrigger.SpinReelBackwards)
                .PermitReentry(ReelControllerTrigger.SpinConstant)
                .PermitReentry(ReelControllerTrigger.Accelerate)
                .PermitReentry(ReelControllerTrigger.Decelerate)
                .Permit(ReelControllerTrigger.TiltReels, ReelControllerState.Tilted)
                .Permit(ReelControllerTrigger.Disconnected, ReelControllerState.Disconnected)
                .PermitDynamic(
                    ReelControllerTrigger.ReelStopped,
                    () => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.Spinning);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid State {state} For Trigger {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"ReelControllerStateMachine - Trigger {transition.Trigger} Transitioned From {transition.Source} To {transition.Destination}");
                });

            return stateMachine;
        }

        private bool CanFire(ReelControllerTrigger trigger, int reelId, bool checkControllerState = true)
        {
            _stateLock.EnterReadLock();
            bool canFire;
            try
            {
                canFire = !checkControllerState || CanFire(trigger);
                if (!canFire)
                {
                    Logger.Debug($"CanFire - FAILED for trigger {trigger} and reel {reelId} in state {_state.State}");
                }
                else
                {
                    canFire = _reelStates.TryGetValue(reelId, out var reelState);
                    if (!canFire)
                    {
                        Logger.Debug($"CanFire - FAILED getting reel state for trigger {trigger} and reel {reelId} in state {_state.State}");
                    }
                    else
                    {
                        canFire = reelState.StateMachine.CanFire(trigger);
                        if (!canFire)
                        {
                            Logger.Debug($"CanFire - FAILED for trigger {trigger} and reel {reelId} with reelState {reelState.StateMachine} in state {_state.State}");
                        }
                    }
                }
            }
            finally
            {
                _stateLock.ExitReadLock();
            }

            return canFire;
        }

        private void Fire<TEvent>(
            ReelControllerTrigger trigger,
            int reelId,
            TEvent @event,
            bool updateControllerState = true)
            where TEvent : IEvent
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (!Fire(trigger, reelId, updateControllerState))
                {
                    Logger.Debug($"Fire - FAILED for trigger {trigger} and reel {reelId} with updateControllerState {updateControllerState}");
                    return;
                }

                _eventBus?.Publish(@event);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private ReelControllerState GetEnabledState()
        {
            var nonIdleReels = ReelStates.Where(x => NonIdleStates.Contains(x.Value)).ToList();
            if (nonIdleReels.Any())
            {
                return nonIdleReels.FirstOrDefault().Value switch
                {
                    ReelLogicalState.Spinning or
                        ReelLogicalState.SpinningBackwards or
                        ReelLogicalState.SpinningForward or
                        ReelLogicalState.Accelerating or
                        ReelLogicalState.Decelerating or
                        ReelLogicalState.Stopping => ReelControllerState.Spinning,
                    ReelLogicalState.Homing => ReelControllerState.Homing,
                    ReelLogicalState.Tilted => ReelControllerState.Tilted,
                    _ => ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                        ? ReelControllerState.IdleAtStops
                        : ReelControllerState.IdleUnknown
                };
            }

            return ReelStates.All(x => x.Value == ReelLogicalState.IdleAtStop)
                ? ReelControllerState.IdleAtStops
                : ReelControllerState.IdleUnknown;
        }

        private class StateMachineWithStoppingTrigger
        {
            public StateMachine<ReelLogicalState, ReelControllerTrigger> StateMachine { get; }

            public StateMachine<ReelLogicalState, ReelControllerTrigger>.TriggerWithParameters<ReelLogicalState, ReelEventArgs> StoppingTrigger { get; }

            public StateMachineWithStoppingTrigger(
                StateMachine<ReelLogicalState, ReelControllerTrigger> stateMachine,
                StateMachine<ReelLogicalState, ReelControllerTrigger>.TriggerWithParameters<ReelLogicalState, ReelEventArgs> stoppingTrigger)
            {
                StateMachine = stateMachine;
                StoppingTrigger = stoppingTrigger;
            }
        }
    }
}
