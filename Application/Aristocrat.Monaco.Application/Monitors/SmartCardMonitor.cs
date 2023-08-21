namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Drm;
    using Kernel;

    public class SmartCardMonitor : IService, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IMeterManager _meters;

        private bool _disposed;

        public SmartCardMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IDigitalRights>())
        {
        }

        public SmartCardMonitor(IEventBus bus, IMeterManager meters, IDigitalRights digitalRights)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            if (digitalRights == null)
            {
                throw new ArgumentNullException(nameof(digitalRights));
            }

            if (digitalRights.Disabled)
            {
                IncrementMeters();
            }

            _bus.Subscribe<SoftwareProtectionModuleDisconnectedEvent>(this, _ => IncrementMeters());
            _bus.Subscribe<SoftwareProtectionModuleErrorEvent>(this, _ => IncrementMeters());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(SmartCardMonitor).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(SmartCardMonitor) };

        public void Initialize()
        {
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

        private void IncrementMeters()
        {
            _meters.GetMeter(ApplicationMeters.SmartCardErrorCount).Increment(1);
        }
    }
}