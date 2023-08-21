namespace Aristocrat.Monaco.Mgam.Services.Event
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reactive;
    using System.Reactive.Linq;
    using Application.Contracts.Authentication;
    using Aristocrat.Mgam.Client.Common;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;
    using NoteAcceptor = Hardware.Contracts.NoteAcceptor;
    using Printer = Hardware.Contracts.Printer;

    /// <summary>
    ///     Captures events and forwards them to consumers when they are ready to receive events.
    /// </summary>
    public sealed class StartupEventListener : StartupEventListenerBase
    {
        private SubscriptionList _subscriptions = new SubscriptionList();

        private readonly IObservable<EventPattern<StartupEventArgs>> _events;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StartupEventListener"/> class.
        /// </summary>
        public StartupEventListener()
        {
            _events = Observable.FromEventPattern<StartupEventArgs>(
                h => EventReceived += h,
                h => EventReceived -= h);

        }

        private event EventHandler<StartupEventArgs> EventReceived;

        /// <summary>
        ///     Subscribes handler to startup events.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void Subscribe(Type eventType, Action<IEvent> handler)
        {
            _subscriptions.Add(_events
                .Where(e => e.EventArgs.Event.GetType() == eventType)
                .Subscribe(e => handler?.Invoke(e.EventArgs.Event)));
        }

        /// <summary>
        ///     Publish captured startup events.
        /// </summary>
        public void Publish()
        {
            Unsubscribe();

            while (EventQueue.TryDequeue(out var evt))
            {
                RaiseEventReceived(evt);
            }
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_subscriptions != null)
                {
                    _subscriptions.Dispose();
                }
            }

            _subscriptions = null;
        }

        /// <inheritdoc />
        protected override void Subscribe()
        {
            EventBus?.Subscribe<PlatformBootedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<OpenEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<ClosedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<NoteAcceptor.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<NoteAcceptor.HardwareFaultClearEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<NoteAcceptor.DisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<NoteAcceptor.InspectionFailedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Printer.InspectionFailedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Printer.DisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Printer.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<BatteryLowEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<LiveAuthenticationFailedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<StorageErrorEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<PersistentStorageIntegrityCheckFailedEvent>(this, EventQueue.Enqueue);
        }

        private void RaiseEventReceived(IEvent @event)
        {
            EventReceived?.Invoke(this, new StartupEventArgs(@event));
        }
    }
}
