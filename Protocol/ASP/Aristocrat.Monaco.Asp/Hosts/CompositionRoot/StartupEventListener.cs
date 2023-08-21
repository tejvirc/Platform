namespace Aristocrat.Monaco.Asp.Hosts.CompositionRoot
{
    using Application.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;

    /// <summary>A class to handle the startup events</summary>
    public class StartupEventListener : StartupEventListenerBase
    {
        /// <inheritdoc />
        protected override void Subscribe()
        {
            // kernel events
            EventBus?.Subscribe<InitializationCompletedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<SystemDisableAddedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<SystemDisableRemovedEvent>(this, EventQueue.Enqueue);

            // hardware events
            EventBus?.Subscribe<ClosedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<OpenEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<PersistentStorageIntegrityCheckFailedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<StorageErrorEvent>(this, EventQueue.Enqueue);

            // application events
            EventBus?.Subscribe<DoorOpenMeteredEvent>(this, EventQueue.Enqueue); // triggered by OpenEvent
            EventBus?.Subscribe<SystemDisabledByOperatorEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<SystemEnabledByOperatorEvent>(this, EventQueue.Enqueue);
        }
    }
}