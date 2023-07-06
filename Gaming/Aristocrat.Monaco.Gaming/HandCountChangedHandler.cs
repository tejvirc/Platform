namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Accounting.Contracts.HandCount;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class HandCountChangedHandler : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IRuntime _runtime;
        private bool _disposed;

        public HandCountChangedHandler(IRuntime runtime, IEventBus bus)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _bus.Subscribe<HandCountChangedEvent>(this, e => _runtime.UpdateHandCount(e.HandCount));
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
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}