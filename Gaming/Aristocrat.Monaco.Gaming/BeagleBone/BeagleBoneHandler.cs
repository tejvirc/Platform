namespace Aristocrat.Monaco.Gaming.BeagleBone
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;

    /// <summary>
    ///     Implementation of BeagleBoneHandler
    /// </summary>
    /// <remarks>
    ///     BeagleBoneHandler handles events for the Beagle Bone lighting controller
    /// </remarks>
    public class BeagleBoneHandler : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IBeagleBoneController _beagleBoneController;

        private bool _disposed;

        public BeagleBoneHandler(IEventBus eventBus, IBeagleBoneController beagleBoneController)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _beagleBoneController = beagleBoneController ?? throw new ArgumentNullException(nameof(beagleBoneController));

            _eventBus.Subscribe<AttractModeEntered>(this, Handle);
            _eventBus.Subscribe<AttractModeExited>(this, Handle);
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, Handle);
            _eventBus.Subscribe<PrimaryGameEndedEvent>(this, Handle);
            _eventBus.Subscribe<GamePresentationEndedEvent>(this, Handle);
            _eventBus.Subscribe<SecondaryGameEndedEvent>(this, Handle);
        }

        private void Handle(AttractModeEntered evt)
        {
            _beagleBoneController.SendShow(LightShows.Attract);
        }

        private void Handle(AttractModeExited evt)
        {
            _beagleBoneController.SendShow(LightShows.Default);
        }

        private void Handle(PrimaryGameStartedEvent evt)
        {
            _beagleBoneController.SendShow(LightShows.Start);
        }

        private void Handle(PrimaryGameEndedEvent evt)
        {
            _beagleBoneController.SendShow(LightShows.Default);
        }

        private void Handle(GamePresentationEndedEvent evt)
        {
            _beagleBoneController.SendShow(LightShows.Default);
        }

        private void Handle(SecondaryGameEndedEvent evt)
        {
            _beagleBoneController.SendShow(LightShows.Default);
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
                _beagleBoneController.SendShow(LightShows.Default);
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
