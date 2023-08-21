namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Implementation of SystemDrivenAutoPlayHandler
    /// </summary>
    /// <remarks>
    ///     SystemDrivenAutoPlayHandler handles the SystemDrivenAutoPlayEvent event and if allowed will initiate auto play
    /// </remarks>
    public class SystemDrivenAutoPlayHandler : IDisposable
    {
        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        public SystemDrivenAutoPlayHandler(IRuntime runtime, IEventBus eventBus)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<AutoPlayRequestedEvent>(this, Handle);
            _eventBus.Subscribe<AutoPlayPauseRequestedEvent>(this, HandlePause);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Handle(AutoPlayRequestedEvent evt)
        {
            if (evt.Enable)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void HandlePause(AutoPlayPauseRequestedEvent evt)
        {
            _runtime.UpdateFlag(RuntimeCondition.PlatformDisableAutoPlay, evt.Pause);
        }

        private void Start()
        {
            _runtime.UpdateFlag(RuntimeCondition.StartSystemDrivenAutoPlay, true);

            _eventBus.Subscribe<CallAttendantButtonOffEvent>(this, _ => Stop());
            _eventBus.Subscribe<CashOutButtonPressedEvent>(this, _ => Stop());
            _eventBus.Subscribe<SystemDisabledEvent>(this, _ => Stop());
        }

        private void Stop()
        {
            _runtime.UpdateFlag(RuntimeCondition.StartSystemDrivenAutoPlay, false);

            _eventBus.Unsubscribe<CallAttendantButtonOffEvent>(this);
            _eventBus.Unsubscribe<CashOutButtonPressedEvent>(this);
            _eventBus.Unsubscribe<SystemDisabledEvent>(this);
        }
    }
}