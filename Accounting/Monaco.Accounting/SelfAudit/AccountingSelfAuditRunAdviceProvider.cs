namespace Aristocrat.Monaco.Accounting.SelfAudit
{
    using System;
    using Contracts.SelfAudit;
    using Kernel;

    public class AccountingSelfAuditRunAdviceProvider : ISelfAuditRunAdviceProvider, IDisposable
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private bool _disposed;

        public AccountingSelfAuditRunAdviceProvider()
            : this(
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public AccountingSelfAuditRunAdviceProvider(
            ISystemDisableManager disableManager,
            IEventBus eventBus)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<SystemEnabledEvent>(this, x => { RunAdviceChanged?.Invoke(this, EventArgs.Empty); });
            _eventBus.Subscribe<SystemDisabledEvent>(this, x => { RunAdviceChanged?.Invoke(this, EventArgs.Empty); });
        }

        /// <summary>
        ///     Unsubscribe the event subscriptions and dispose the provider
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public event EventHandler RunAdviceChanged;

        /// <inheritdoc />
        public bool SelfAuditOkToRun()
        {
            return !_disableManager.IsDisabled;
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