namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using Kernel.Contracts.MessageDisplay;

    /// <summary>
    ///     The different types of faults that can occur on a reel for a reel controller
    /// </summary>
    [Flags]
    public enum ReelFaults
    {
        /// <summary>
        ///     The state for when there is no faults on reels
        /// </summary>
        None = 0,

        /// <summary>
        ///     The error indicating a reel was disconnected
        /// </summary>
        [ErrorGuid("{A69CB751-EAD8-4AC2-83F8-70653538EA15}", DisplayableMessageClassification.HardError)]
        Disconnected = 0x0001,

        /// <summary>
        ///     The error indicating that the voltage level is invalid for a reel
        /// </summary>
        [ErrorGuid("{DF57B5CC-EF18-4B77-A925-A3BEB0A07DB0}", DisplayableMessageClassification.HardError)]
        LowVoltage = 0x0002,

        /// <summary>
        ///     The error when a reel has stalled during a spin
        /// </summary>
        [ErrorGuid("{CB863818-696E-4443-9DCB-1039151CB5FE}", DisplayableMessageClassification.HardError)]
        ReelStall = 0x0008,

        /// <summary>
        ///     The error used to represent when a reel has been moved from its idle position
        /// </summary>
        [ErrorGuid("{C188AE59-D9F0-4765-8A38-A7D85F77C301}", DisplayableMessageClassification.HardError)]
        ReelTamper = 0x0010,
    }
}