using Aristocrat.Monaco.Kernel;
using System.Timers;

namespace Aristocrat.Monaco.Hhr.UI
{
    public class HHRTimer : Timer
    {
        private bool _disposed;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisbleManager;

        public HHRTimer(int interval) : base(interval)
        {
            _eventBus = ServiceManager.GetInstance()?.GetService<IEventBus>();
            _systemDisbleManager = ServiceManager.GetInstance()?.GetService<ISystemDisableManager>();
        }

        public new void Stop()
        {
            _eventBus?.Unsubscribe<SystemDisabledEvent>(this);
            _eventBus?.Unsubscribe<SystemEnabledEvent>(this);
            base.Stop();
        }

        public new void Start()
        {
            _eventBus?.Subscribe<SystemDisabledEvent>(this, OnSystemDisabled);
            _eventBus?.Subscribe<SystemEnabledEvent>(this, OnSystemEnabled);
            base.Start();
        }

        private void OnSystemDisabled(SystemDisabledEvent theEvent)
        {
            if (_systemDisbleManager.DisableImmediately)
            {
                base.Stop();
            }
        }

        private void OnSystemEnabled(SystemEnabledEvent theEvent)
        {
            base.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}