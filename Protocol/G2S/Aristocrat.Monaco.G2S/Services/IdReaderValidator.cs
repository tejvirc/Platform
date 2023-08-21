namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Common.Events;
    using Gaming.Contracts.Session;
    using Kernel;

    public class IdReaderValidator : IIdReaderValidator, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IG2SEgm _g2SEgm;

        private bool _disposed;

        public IdReaderValidator(IG2SEgm g2SEgm, IEventBus eventBus)
        {
            _g2SEgm = g2SEgm ?? throw new ArgumentNullException(nameof(g2SEgm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool CanAccept(int idReaderId)
        {
            var playerDevice = _g2SEgm.GetDevice<IPlayerDevice>();

            if (playerDevice.IdReader == 0)
            {
                return false;
            }

            if (idReaderId == playerDevice.IdReader)
            {
                return true;
            }

            return playerDevice.UseMultipleIdDevices && playerDevice.IdReaders.Contains(idReaderId);
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IIdReaderValidator) };

        public void Initialize()
        {
            _eventBus.Subscribe<DeviceConfigurationChangedEvent>(this, HandleEvent);
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

        private void HandleEvent(DeviceConfigurationChangedEvent evt)
        {
            if (!(evt.Device is IPlayerDevice))
            {
                return;
            }

            // We could be a bit more pro-active with this and check to see if the associated devices, etc. changes, but this isn't going to happen often
            _eventBus.Publish(new ValidationDeviceChangedEvent());
        }
    }
}
