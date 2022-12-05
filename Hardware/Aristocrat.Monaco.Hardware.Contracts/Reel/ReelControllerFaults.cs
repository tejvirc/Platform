namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using Kernel;

    /// <summary>
    ///     The different types of faults that can occur on a reel controller
    /// </summary>
    [Flags]
    public enum ReelControllerFaults
    {
        /// <summary>
        ///     The state for when there are no faults on the reel controller
        /// </summary>
        None = 0,

        /// <summary>
        ///     The error indicating the reel controller was disconnected
        /// </summary>
        [ErrorGuid("{028C86C2-1A11-464B-897C-88F9FA9F8733}", DisplayableMessageClassification.HardError)]
        Disconnected = 0x0001,

        /// <summary>
        ///     The error indicating a firmware fault on the reel controller
        /// </summary>
        [ErrorGuid("{F7E5E84A-F7FB-4330-BCDB-B980487AFB5D}", DisplayableMessageClassification.HardError)]
        FirmwareFault = 0x0002,

        /// <summary>
        ///     The error indicating a hardware event on the reel controller.  A hardware event
        ///     indicates an issue with the hardware which may be able to be cleared in software.
        /// </summary>
        [ErrorGuid("{26E72A20-E3EA-48F5-BE66-204D7014714D}", DisplayableMessageClassification.HardError)]
        HardwareError = 0x0004,

        /// <summary>
        ///     The error indicating the lights/lamps have an issue
        /// </summary>
        [ErrorGuid("{858DDE61-5BEB-4A82-B246-2D31E579EA39}", DisplayableMessageClassification.HardError)]
        LightError = 0x0008,

        /// <summary>
        ///     The error indicating that the voltage level is invalid for the reel controller
        /// </summary>
        [ErrorGuid("{1716476D-5C53-4A25-B4B2-44F80FA02BB3}", DisplayableMessageClassification.HardError)]
        LowVoltage = 0x0010,

        /// <summary>
        ///     The error indicating a communication error on the reel controller.
        /// </summary>
        [ErrorGuid("{D7C35CBB-9D7F-4C0D-9792-0C27E2E594D0}", DisplayableMessageClassification.HardError)]
        CommunicationError = 0x0020,

        /// <summary>A command request received a NAK response to a well-formed command.</summary>
        [ErrorGuid("{77594C12-3F17-4872-97F3-DD62129D15E5}", DisplayableMessageClassification.HardError)]
        RequestError = 0x0040,
    }
}