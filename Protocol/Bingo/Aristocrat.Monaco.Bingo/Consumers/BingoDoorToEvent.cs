namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System.Collections.Generic;
    using Common;
    using Hardware.Contracts.Door;

    public sealed class BingoDoorToEvent
    {
        /// <summary>
        ///     The lookup table of door events to Bingo server events to report.
        ///
        ///     The Bingo server lists the following doors which don't currently appear in the
        ///     DoorLogicalId enumeration:
        ///       Base Door
        ///       Aux Door
        /// </summary>
        public static readonly IReadOnlyDictionary<DoorLogicalId, BingoDoorToEvent> DoorEventMap =
            new Dictionary<DoorLogicalId, BingoDoorToEvent>
            {
                {
                    DoorLogicalId.Main,
                    new BingoDoorToEvent(ReportableEvent.MainDoorOpened, ReportableEvent.MainDoorClosed)
                },
                {
                    DoorLogicalId.UniversalInterfaceBox,
                    new BingoDoorToEvent(ReportableEvent.PizzaBoxDoorOpened, ReportableEvent.PizzaBoxDoorClosed)
                },
                {
                    DoorLogicalId.TopBox,
                    new BingoDoorToEvent(ReportableEvent.LcdDoorOpened, ReportableEvent.LcdDoorClosed)
                },
                {
                    DoorLogicalId.DropDoor,
                    new BingoDoorToEvent(ReportableEvent.CashDoorOpened, ReportableEvent.CashDoorClosed) // Drop door is what the server calls the cash door
                },
                {
                    DoorLogicalId.Logic,
                    new BingoDoorToEvent(ReportableEvent.LogicDoorOpened, ReportableEvent.LogicDoorClosed)
                },
                {
                    DoorLogicalId.CashBox,
                    new BingoDoorToEvent(ReportableEvent.StackerDoorOpened, ReportableEvent.StackerDoorClosed) // CashBox is the stack door
                },
                {
                    DoorLogicalId.Belly,
                    new BingoDoorToEvent(ReportableEvent.BellyDoorOpened, ReportableEvent.BellyDoorClosed)
                }
            };

        private BingoDoorToEvent(ReportableEvent opened, ReportableEvent closed)
        {
            Opened = opened;
            Closed = closed;
        }

        public ReportableEvent Opened { get; }

        public ReportableEvent Closed { get; }
    }
}