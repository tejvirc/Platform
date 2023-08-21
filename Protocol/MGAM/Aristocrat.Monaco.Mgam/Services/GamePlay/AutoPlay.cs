namespace Aristocrat.Monaco.Mgam.Services.GamePlay
{
    using System;
    using Aristocrat.Mgam.Client;
    using Attributes;
    using Common.Events;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Manages auto play requests from the server.
    /// </summary>
    public class AutoPlay : IAutoPlay, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IAttributeManager _attributes;
        private readonly IAutoPlayStatusProvider _autoPlayProvider;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoPlay"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="attributes"><see cref="IAttributeManager"/>.</param>
        /// <param name="autoPlayProvider"><see cref="IAutoPlayStatusProvider"/>.</param>
        public AutoPlay(
            IEventBus eventBus,
            IAttributeManager attributes,
            IAutoPlayStatusProvider autoPlayProvider)
        {
            _eventBus = eventBus;
            _attributes = attributes;
            _autoPlayProvider = autoPlayProvider;

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~AutoPlay()
        {
            Dispose(false);
        }

        /// <inheritdoc />
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

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AttributeChangedEvent>(this, Handle, e => e.AttributeName == AttributeNames.AutoPlay);
            _eventBus.Subscribe<GamePlayEnabledEvent>(this, Handle);
        }

        private void Handle(GamePlayEnabledEvent evt)
        {
            var status = _attributes.Get(AttributeNames.AutoPlay, false);
            if (status)
            {
                _autoPlayProvider.StartSystemAutoPlay();
            }
        }

        private void Handle(AttributeChangedEvent evt)
        {
            var status = _attributes.Get(AttributeNames.AutoPlay, false);
            if (status)
            {
                _autoPlayProvider.StartSystemAutoPlay();
            }
            else
            {
                _autoPlayProvider.EndAutoPlayIfActive();
            }
        }
    }
}
