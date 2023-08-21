namespace Aristocrat.Monaco.Sas.Consumers
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Stores open and close SasExceptionTypes for doors.
    /// </summary>
    public class SasExceptionDoorInfo
    {
        /// <summary>
        ///     The lookup table of door events to SAS exceptions to report
        /// </summary>
        public static readonly IReadOnlyDictionary<DoorLogicalId, SasExceptionDoorInfo> DoorExceptionMap =
            new Dictionary<DoorLogicalId, SasExceptionDoorInfo>
            {
                {
                    DoorLogicalId.Main,
                    new SasExceptionDoorInfo(GeneralExceptionCode.SlotDoorWasOpened, GeneralExceptionCode.SlotDoorWasClosed)
                },
                {
                    DoorLogicalId.TopBox,
                    new SasExceptionDoorInfo(GeneralExceptionCode.SlotDoorWasOpened, GeneralExceptionCode.SlotDoorWasClosed)
                },
                {
                    DoorLogicalId.DropDoor,
                    new SasExceptionDoorInfo(GeneralExceptionCode.DropDoorWasOpened, GeneralExceptionCode.DropDoorWasClosed)
                },
                {
                    DoorLogicalId.Logic,
                    new SasExceptionDoorInfo(GeneralExceptionCode.CardCageWasOpened, GeneralExceptionCode.CardCageWasClosed)
                },
                {
                    DoorLogicalId.CashBox,
                    new SasExceptionDoorInfo(GeneralExceptionCode.CashBoxDoorWasOpened, GeneralExceptionCode.CashBoxDoorWasClosed)
                },
                {
                    DoorLogicalId.Belly,
                    new SasExceptionDoorInfo(GeneralExceptionCode.BellyDoorWasOpened, GeneralExceptionCode.BellyDoorWasClosed)
                }
            };

        /// <summary>
        ///     The lookup table to power up with door access exception reporting
        /// </summary>
        public static readonly IReadOnlyDictionary<DoorLogicalId, GeneralExceptionCode> DoorAccessWhilePowerOffMap =
            new Dictionary<DoorLogicalId, GeneralExceptionCode>
            {
                { DoorLogicalId.Logic, GeneralExceptionCode.PowerOffCardCageAccess },
                { DoorLogicalId.Main, GeneralExceptionCode.PowerOffSlotDoorAccess },
                { DoorLogicalId.TopBox, GeneralExceptionCode.PowerOffSlotDoorAccess },
                { DoorLogicalId.CashBox, GeneralExceptionCode.PowerOffCashBoxDoorAccess },
                { DoorLogicalId.DropDoor, GeneralExceptionCode.PowerOffDropDoorAccess }
            };

        /// <summary>
        ///     Initializes a new instance of the SasExceptionDoorInfo class.
        /// </summary>
        /// <param name="opened">Value for the Opened property.</param>
        /// <param name="closed">Value for the Closed property.</param>
        private SasExceptionDoorInfo(GeneralExceptionCode opened, GeneralExceptionCode closed)
        {
            Opened = opened;
            Closed = closed;
        }

        /// <summary>
        ///     Gets the opened value.
        /// </summary>
        public GeneralExceptionCode Opened { get; }

        /// <summary>
        ///     Gets the closed value.
        /// </summary>
        public GeneralExceptionCode Closed { get; }
    }
}