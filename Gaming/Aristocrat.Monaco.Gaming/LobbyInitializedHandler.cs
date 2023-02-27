namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
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
    public class LobbyInitializeHandler 
    {
        private readonly IHandCount _handCount;
        private readonly IEventBus _eventBus;

        public LobbyInitializeHandler(IHandCount handCount, IEventBus eventBus)
        {
            _handCount = handCount ?? throw new ArgumentNullException(nameof(handCount));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<LobbyInitializedEvent>(this, Handle);
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

        private void Handle(LobbyInitializedEvent evt)
        {
            _handCount.SendHandCountChangedEvent();
        }
    }
}