namespace Aristocrat.Monaco.Sas.Base
{
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Authentication;
    using Application.Contracts.NoteAcceptorMonitor;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts.Events;
    using log4net;
    using Bna = Hardware.Contracts.NoteAcceptor;
    using Prn = Hardware.Contracts.Printer;

    /// <summary>A class to handle the startup events</summary>
    public class StartupEventListener : StartupEventListenerBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _bnaInspected;
        private bool _bnaInspectionFailureHandle;
        private bool _printerInspected;
        private bool _printerInspectionFailureHandle;

        /// <inheritdoc />
        protected override void Subscribe()
        {
            // door's early events
            EventBus?.Subscribe<DoorOpenMeteredEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<DoorClosedMeteredEvent>(
                this,
                @event =>
                {
                    if (@event.WhilePoweredDown && !EventQueue.Any(
                            e => e is DoorOpenMeteredEvent doorOpen &&
                                 doorOpen.LogicalId == @event.LogicalId &&
                                 doorOpen.WhilePoweredDown))
                    {
                        Logger.Warn(
                            $"Found a door closed while powering up without a corresponding door open event for {@event.DoorName}.  Posting a power down with door open event to SAS");
                        EventQueue.Enqueue(new DoorOpenMeteredEvent(@event.LogicalId, true, true, @event.DoorName));
                    }

                    EventQueue.Enqueue(@event);
                });

            // BNA early events
            EventBus?.Subscribe<PlatformBootedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Bna.InspectedEvent>(this, _ => { _bnaInspected = true; });
            EventBus?.Subscribe<Bna.NoteAcceptorChangedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Bna.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Bna.HardwareFaultClearEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Bna.DisconnectedEvent>(
                this,
                @event =>
                {
                    if (_bnaInspected)
                    {
                        EventQueue.Enqueue(@event);
                    }
                });
            EventBus?.Subscribe<Bna.InspectionFailedEvent>(
                this,
                _ =>
                {
                    if (!_bnaInspectionFailureHandle)
                    {
                        _bnaInspectionFailureHandle = true;
                        EventQueue.Enqueue(new Bna.DisconnectedEvent());
                    }
                });
            EventBus?.Subscribe<NoteAcceptorDocumentCheckOccurredEvent>(this, EventQueue.Enqueue);

            // printer's early events
            EventBus?.Subscribe<Prn.InspectedEvent>(this, _ => { _printerInspected = true; });
            EventBus?.Subscribe<Prn.DisconnectedEvent>(this, EnqueuePrinterEventPredicate);
            EventBus?.Subscribe<Prn.HardwareFaultEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Prn.HardwareWarningEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<Prn.InspectionFailedEvent>(
                this,
                _ =>
                {
                    if (!_printerInspectionFailureHandle)
                    {
                        _printerInspectionFailureHandle = true;
                        EventQueue.Enqueue(new Prn.DisconnectedEvent());
                    }
                });

            // game events
            EventBus?.Subscribe<GameAddedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<GameConnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<GameProcessExitedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<GameRequestedLobbyEvent>(this, EventQueue.Enqueue);

            // display events
            EventBus?.Subscribe<DisplayDisconnectedEvent>(this, EventQueue.Enqueue);
            EventBus?.Subscribe<TouchDisplayDisconnectedEvent>(this, EventQueue.Enqueue);

            // battery low event
            EventBus?.Subscribe<BatteryLowEvent>(this, EventQueue.Enqueue);

            //LiveAuthenticationFailedEvent event
            EventBus?.Subscribe<LiveAuthenticationFailedEvent>(this, EventQueue.Enqueue);

            // LinkedProgressiveExpiredEvent event
            EventBus?.Subscribe<LinkedProgressiveExpiredEvent>(this, EventQueue.Enqueue);
        }

        private void EnqueuePrinterEventPredicate(IEvent @event)
        {
            if (_printerInspected)
            {
                EventQueue.Enqueue(@event);
            }
        }
    }
}