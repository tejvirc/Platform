namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using System.ComponentModel;

    /// <summary>A bit-field of flags for specifying hopper fault types.</summary>
    [Flags]
    public enum HopperFaultTypes
    {
        /// <summary>No fault.</summary>
        [Description("No Fault")]
        [ErrorGuid("{06C67E1C-8DBC-4F38-99FD-87D2C8B0D04F}")]
        None = 0x0000,

        /// <summary>Hopper Empty.</summary>
        [Description("Hopper Empty")]
        [ErrorGuid("{DF164461-D6A3-4C76-B0CD-4DBD4586E50A}")]
        HopperEmpty = 0x0001,

        /// <summary>Illegal Coin Out Fault.</summary>
        [Description("Illegal Coin Out")]
        [ErrorGuid("{BA3EE6BE-26D2-468D-874B-10E0FF892F1D}")]
        IllegalCoinOut = 0x0002,

        /// <summary>Hopper Disconnected.</summary>
        [Description("Hopper Disconnected")]
        [ErrorGuid("{B4EE8750-99B1-4B52-B008-1CA39C52E6C0}")]
        HopperDisconnected = 0x0004,

        /// <summary>Hopper Jam.</summary>
        [Description("Hopper Jam")]
        [ErrorGuid("{620135AF-7BD4-4614-87B4-7A0EC88BDF58}")]
        HopperJam = 0x0008
    }
}



