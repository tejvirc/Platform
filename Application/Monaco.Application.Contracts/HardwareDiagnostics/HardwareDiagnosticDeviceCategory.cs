namespace Aristocrat.Monaco.Application.Contracts.HardwareDiagnostics
{
    /// <summary>
    ///     Enum of categories of devices that can have hardware diagnostic tests run
    /// </summary>
    public enum HardwareDiagnosticDeviceCategory
    {
        /// <summary>
        /// Hardware device is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Hardware device being tested relates to Buttons
        /// </summary>
        Buttons,

        /// <summary>
        /// Hardware device being tested relates to Displays
        /// </summary>
        Displays,

        /// <summary>
        /// Hardware device being tested relates to Id Readers
        /// </summary>
        IdReader,

        /// <summary>
        /// Hardware device being tested relates to Lamps
        /// </summary>
        Lamps,

        /// <summary>
        /// Hardware device being tested relates to Edge Lighting
        /// </summary>
        EdgeLighting,

        /// <summary>
        /// Hardware device being tested relates to Note Acceptors
        /// </summary>
        NoteAcceptor,

        /// <summary>
        /// Hardware device being tested relates to Printers
        /// </summary>
        Printer,

        /// <summary>
        /// Hardware device being tested relates to Sound
        /// </summary>
        Sound,

        /// <summary>
        /// Hardware device being tested relates to Mechanical Reels
        /// </summary>
        MechanicalReels,

        /// <summary>
        /// Hardware device being tested relates to Battery
        /// </summary>
        Battery,

        /// <summary>
        /// Hardware device being tested relates to Bell
        /// </summary>
        Bell,

        /// <summary>
        /// Hardware device being tested relates to BeagleBone
        /// </summary>
        BeagleBone,

        /// <summary>
        /// Hardware device being tested relates to Keys
        /// </summary>
        Keys,

        /// <summary>
        /// Hardware device being tested relates to Doors
        /// </summary>
        Doors,
    }
}