namespace Aristocrat.Monaco.Bingo.CompositionRoot
{
    using System;
    using System.Threading;
    using Application.Contracts;
    using Gaming.Contracts.Lobby;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;
    using Monaco.Common;
    using Sas.Contracts.SASProperties;
    using Bna = Hardware.Contracts.NoteAcceptor;
    using IdReader = Hardware.Contracts.IdReader;
    using Printer = Hardware.Contracts.Printer;

    public class StartupEventListener : StartupEventListenerBase
    {
        protected override void Subscribe()
        {
            EventBus.Subscribe<DoorClosedMeteredEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<DoorOpenMeteredEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<BatteryLowEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<PersistentStorageIntegrityCheckFailedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.IdPresentedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.IdClearedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.DisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.HardwareFaultClearEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<IdReader.InspectionFailedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.DisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.HardwareFaultClearEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.InspectionFailedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.HardwareWarningEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Printer.HardwareWarningClearEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.NoteAcceptorChangedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.DisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.HardwareFaultClearEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.InspectionFailedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.DocumentRejectedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.VoucherReturnedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<Bna.CurrencyReturnedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<PlatformBootedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<PropertyChangedEvent>(this, EventQueue.Enqueue,
                @event => @event.PropertyName == SasProperties.SasFeatureSettings);
            EventBus.Subscribe<PeriodMetersClearedEvent>(this, EventQueue.Enqueue);
            EventBus.Subscribe<LobbyInitializedEvent>(this, EventQueue.Enqueue);
        }

        protected override void ConsumeEvent(IEvent data, Func<Type, dynamic> getConsumers)
        {
            var asyncConsumer = typeof(IAsyncConsumer<>);
            Type[] typeArgs = { data.GetType() };
            var consumer = getConsumers(asyncConsumer.MakeGenericType(typeArgs));
            if (consumer is null)
            {
                base.ConsumeEvent(data, getConsumers);
            }
            else
            {
                TaskExtensions.RunOnCurrentThread(() => consumer.Consume((dynamic)data, CancellationToken.None));
            }
        }
    }
}