namespace Aristocrat.Monaco.Gaming
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Hardware.Contracts.Door;
    using Kernel;
    using Kernel.Contracts.Events;
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Hardware.Contracts.Printer;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;

    public class StartupEventListener : StartupEventListenerBase
    {
        private readonly List<Guid> _startupEvents = new List<Guid>();
        private bool _disposed;
        private readonly object _lockObject = new object();

        protected override void Subscribe()
        {
            EventBus.Subscribe<TransactionSavedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<DoorClosedMeteredEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<TransferOutStartedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<TransferOutCompletedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<TransferOutFailedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<HandpayStartedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<VoucherOutStartedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<WatTransferInitiatedEvent>(this, AddStartupEvent);
            EventBus.Subscribe<CashoutNotificationEvent>(this, AddStartupEvent);
            EventBus.Subscribe<HardwareWarningClearEvent>(this, AddStartupEvent);
            EventBus.Subscribe<HardwareWarningEvent>(this, AddStartupEvent);
            EventBus.Subscribe<DocumentRejectedEvent>(this, AddStartupEvent);
        }

        protected override void ConsumeEvent(IEvent data, Func<Type, dynamic> getConsumers)
        {
            lock (_lockObject)
            {
                if (_startupEvents.Contains(data.GloballyUniqueId))
                {
                     EventBus?.Publish(new MissedStartupEvent(data));
                    _startupEvents.Remove(data.GloballyUniqueId);
                    return;
                }
            }

            base.ConsumeEvent(data, getConsumers);
        }

        private void AddStartupEvent<TEvent>(TEvent evt)
            where TEvent : IEvent
        {
            EventQueue.Enqueue(evt);
            lock (_lockObject)
            {
                _startupEvents.Add(evt.GloballyUniqueId);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                lock (_lockObject)
                {
                    _startupEvents.Clear();
                }
            }

            _disposed = true;
        }
    }
}