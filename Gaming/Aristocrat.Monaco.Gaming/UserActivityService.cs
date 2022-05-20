namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Kernel;

    public class UserActivityService : IUserActivityService, IDisposable
    {
        private readonly object _lastActionLock = new object();
        private DateTime? _lastAction;

        private readonly IEventBus _eventBus;

        private bool _disposed;

        public UserActivityService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            Initialize();
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IUserActivityService) };

        /// <inheritdoc />
        public void Initialize()
        {
            //events that reset last update time to now
            _eventBus.Subscribe<GameIdleEvent>(this, _ => HandleActivity(DateTime.UtcNow));
            _eventBus.Subscribe<SetValidationEvent>(this, _ => HandleActivity(DateTime.UtcNow));
            _eventBus.Subscribe<TransactionCompletedEvent>(this, _ => HandleActivity(DateTime.UtcNow));
            _eventBus.Subscribe<SystemEnabledEvent>(this, _ => HandleActivity(DateTime.UtcNow));

            //events that set the last action to null
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<TransactionStartedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<SystemDisabledEvent>(this, _ => HandleActivity());
        }

        private void HandleActivity(DateTime? updateTime = null)
        {
            lock (_lastActionLock)
            {
                _lastAction = updateTime;
            }
        }

        public DateTime? GetLastAction()
        {
            lock (_lastActionLock)
            {
                return _lastAction;
            }
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
    }
}
