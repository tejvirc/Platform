namespace Aristocrat.Monaco.G2S.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Operations;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts.Events;
    using DisabledEvent = Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.NoteAcceptor.EnabledEvent;
    using HardMeterDisabledEvent = Hardware.Contracts.HardMeter.DisabledEvent;
    using HardMeterEnabledEvent = Hardware.Contracts.HardMeter.EnabledEvent;

    public class StartupEventListener : StartupEventListenerBase
    {
        /// <inheritdoc />
        protected override void Subscribe()
        {
            EventBus?.Subscribe<PlatformBootedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<DoorOpenMeteredEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<OpenEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<DoorClosedMeteredEvent>(
                this,
                @event =>
                {
                    if (@event.WhilePoweredDown && !EventQueue.Any(
                        e => e is DoorOpenMeteredEvent doorOpen &&
                             doorOpen.LogicalId == @event.LogicalId &&
                             doorOpen.WhilePoweredDown))
                    {
                        EventQueue.Enqueue(new DoorOpenMeteredEvent(@event.LogicalId, true, true, @event.DoorName));
                    }

                    EventQueue.Enqueue(@event);
                });
            EventBus?.Subscribe<OperatingHoursExpiredEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<OperatingHoursEnabledEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<TouchDisplayDisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<DisplayDisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<DisabledEvent>(
                this,
                e =>
                {
                    if ((e.Reasons & DisabledReasons.Error) == DisabledReasons.Error)
                    {
                        EventQueue.Enqueue(e);
                    }
                });
            EventBus?.Subscribe<EnabledEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<HardMeterDisabledEvent>(
                this,
                e =>
                {
                    if ((e.Reasons & DisabledReasons.Error) == DisabledReasons.Error)
                    {
                        EventQueue.Enqueue(e);
                    }
                });
            EventBus?.Subscribe<HardMeterEnabledEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<ProtocolLoadedEvent>(
                this,
                e =>
                {
                    if (!e.Protocols.Contains(ProtocolNames.G2S))
                    {
                        Task.Run(() => EventBus.UnsubscribeAll(this));
                        while (EventQueue.TryDequeue(out var _)) { }
                    }
                });
            EventBus?.Subscribe<BatteryLowEvent>(this, EventQueue.Enqueue);
        }
    }
}