namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class RuntimeFlagHandler : IRuntimeFlagHandler, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IRuntime _runtime;

        private readonly object _sync = new object();

        private bool _isDirty;

        private bool _disposed;

        public RuntimeFlagHandler(IRuntime runtime, IEventBus bus)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _bus.Subscribe<GameConnectedEvent>(this, _ => SetStates());
        }

        private InternalRuntimeState CurrentState { get; set; } = InternalRuntimeState.None;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetFundsTransferring(bool value)
        {
            SetFlag(InternalRuntimeState.FundsTransferring, value);
        }

        public void SetDisplayingRGDialog(bool value)
        {
            SetFlag(InternalRuntimeState.ResponsibleGamingActive, value);
        }

        public void SetCashingOut(bool value)
        {
            SetFlag(InternalRuntimeState.CashingOut, value);
        }

        public void SetValidatingBillNote(bool value)
        {
            SetFlag(InternalRuntimeState.CashingIn, value);
        }

        public void SetDisplayingOverlay(bool value)
        {
            SetFlag(InternalRuntimeState.DisplayingOverlay, value);
        }

        public void SetServiceRequested(bool value)
        {
            SetFlag(InternalRuntimeState.ServiceRequested, value);
        }

        public void SetDisplayingTimeRemaining(bool value)
        {
            SetFlag(InternalRuntimeState.DisplayTimeRemaining, value);
        }

        public void SetAwaitingPlayerSelection(bool value)
        {
            lock (_sync)
            {
                if (_runtime.Connected)
                {
                    _runtime.UpdateFlag(RuntimeCondition.AwaitingPlayerSelection, value);
                }
            }
        }        
        
        public void SetInPlayerMenu(bool value)
        {
            lock (_sync)
            {
                if (_runtime.Connected)
                {
                    _runtime.UpdateFlag(RuntimeCondition.InPlayerMenu, value);
                }
            }
        }

        public void SetRequestExitGame(bool value)
        {
            lock (_sync)
            {
                if (_runtime.Connected)
                {
                    _runtime.UpdateFlag(RuntimeCondition.RequestExitGame, value);
                }
            }
        }

        public void SetTimeRemaining(string timeRemainingText)
        {
            if (_runtime.Connected)
            {
                _runtime.UpdateTimeRemaining(timeRemainingText);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SetFlag(InternalRuntimeState state, bool value)
        {
            lock (_sync)
            {
                if (HasFlag(state) == value)
                {
                    return;
                }

                if (value)
                {
                    CurrentState |= state;
                }
                else
                {
                    CurrentState &= ~state;
                }

                if (!_runtime.Connected)
                {
                    _isDirty = true;

                    return;
                }

                SendFlag(state, value);
            }
        }

        private bool HasFlag(InternalRuntimeState state)
        {
            return (CurrentState & state) == state;
        }

        private void SetStates()
        {
            lock (_sync)
            {
                // This will force an update if any flag is true or we need to send the current state 
                if (!_isDirty && CurrentState == InternalRuntimeState.None)
                {
                    return;
                }

                SendFlag(
                    InternalRuntimeState.ResponsibleGamingActive,
                    HasFlag(InternalRuntimeState.ResponsibleGamingActive));
                SendFlag(InternalRuntimeState.CashingOut, HasFlag(InternalRuntimeState.CashingOut));
                SendFlag(InternalRuntimeState.CashingIn, HasFlag(InternalRuntimeState.CashingIn));
                SendFlag(InternalRuntimeState.DisplayingOverlay, HasFlag(InternalRuntimeState.DisplayingOverlay));
                SendFlag(InternalRuntimeState.DisplayTimeRemaining, HasFlag(InternalRuntimeState.DisplayTimeRemaining));
                SendFlag(InternalRuntimeState.FundsTransferring, HasFlag(InternalRuntimeState.FundsTransferring));
                SendFlag(InternalRuntimeState.ServiceRequested, HasFlag(InternalRuntimeState.ServiceRequested));

                _isDirty = false;
            }
        }

        private void SendFlag(InternalRuntimeState state, bool value)
        {
            switch (state)
            {
                case InternalRuntimeState.ResponsibleGamingActive:
                    _runtime.UpdateFlag(RuntimeCondition.ResponsibleGamingActive, value);
                    break;
                case InternalRuntimeState.CashingOut:
                    _runtime.UpdateFlag(RuntimeCondition.CashingOut, value);
                    break;
                case InternalRuntimeState.CashingIn:
                    _runtime.UpdateFlag(RuntimeCondition.ValidatingCurrency, value);
                    break;
                case InternalRuntimeState.DisplayingOverlay:
                    _runtime.UpdateFlag(RuntimeCondition.DisplayingVbdOverlay, value);
                    break;
                case InternalRuntimeState.DisplayTimeRemaining:
                    _runtime.UpdateFlag(RuntimeCondition.DisplayingTimeRemaining, value);
                    break;
                case InternalRuntimeState.FundsTransferring:
                    _runtime.UpdateFlag(RuntimeCondition.FundsTransferring, value);
                    break;
                case InternalRuntimeState.ServiceRequested:
                    _runtime.UpdateFlag(RuntimeCondition.ServiceRequested, value);
                    break;
                case InternalRuntimeState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        [Flags]
        private enum InternalRuntimeState
        {
            None = 0x00,
            ResponsibleGamingActive = 0x01,
            CashingOut = 0x02,
            CashingIn = 0x04,
            DisplayingOverlay = 0x08,
            DisplayTimeRemaining = 0x10,
            FundsTransferring = 0x20,
            ServiceRequested = 0x40
        }
    }
}