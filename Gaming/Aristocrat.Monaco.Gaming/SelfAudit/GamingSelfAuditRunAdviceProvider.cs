namespace Aristocrat.Monaco.Gaming.SelfAudit
{
    using System;
    using Accounting.Contracts.SelfAudit;
    using Contracts;
    using Kernel;

    public class GamingSelfAuditRunAdviceProvider : ISelfAuditRunAdviceProvider, IDisposable
    {
        private readonly IGamePlayState _gameState;
        private readonly IEventBus _eventBus;
        private bool _disposed;

        public GamingSelfAuditRunAdviceProvider()
            : this(
                ServiceManager.GetInstance().GetService<IGamePlayState>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public GamingSelfAuditRunAdviceProvider(
            IGamePlayState gameState,
            IEventBus eventBus)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                this,
                _ => { RunAdviceChanged?.Invoke(this, EventArgs.Empty); });
        }

        /// <summary>
        ///     Unsubscribe the event subscriptions and dispose the provider
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public event EventHandler RunAdviceChanged;

        /// <inheritdoc />
        public bool SelfAuditOkToRun()
        {
            return _gameState.CurrentState == PlayState.Idle;
        }

        /// <summary>
        ///     Disposing the provider and unsubscribing the events.
        /// </summary>
        /// <param name="disposing">If disposing the provider</param>
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
    }
}