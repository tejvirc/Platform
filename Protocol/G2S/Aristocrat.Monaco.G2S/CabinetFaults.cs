namespace Aristocrat.Monaco.G2S
{
    /// <summary>
    ///  Defines a constant and utility methods for general faults
    /// </summary>
    public static class Faults
    {
        /// <summary>
        ///     The base value for all general faults
        /// </summary>
        /// <remarks>
        ///     NOTE: This is only negative to avoid conflicts with logical device Ids, which define the other faults that are tracked by the cabinet device
        /// </remarks>
        public const int General = -500;

        /// <summary>
        ///     Determines if the specified fault is a general fault
        /// </summary>
        /// <param name="fault">The fault to evaluate</param>
        /// <returns>true if it's a general fault, otherwise false</returns>
        public static bool IsGeneral(int fault)
        {
            return fault <= General;
        }
    }

    /// <summary>
    ///     Cabinet fault options
    /// </summary>
    public enum CabinetFaults
    {
        HardMeterDisabled = -100,
        StorageFault = -101,
        DisplayDisconnected = -102,
        NoGamesEnabled = Faults.General,
        ButtonDeckDisconnected = Faults.General - 1,
        AudioDisconnected = Faults.General - 2,
        ReelControllerDisabled = Faults.General - 3,
        ReelControllerDisconnected = Faults.General - 4,
        ReelControllerFault = Faults.General - 5,
        ReelControllerInspectionFailed = Faults.General - 6,
        ReelDisconnected = Faults.General - 7, /* Reserve Faults.General-7 to Faults.General-13 for reel disconnected (-7 - reel number) */
        ReelFault = Faults.General - 14, /* Reserve Faults.General-14 to Faults.General-1038 for reel fault (-14 - 0x0400) */
    }
}
