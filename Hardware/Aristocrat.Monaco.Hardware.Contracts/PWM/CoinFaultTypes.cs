namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;
    using System.ComponentModel;

    /// <summary>A bit-field of flags for specifying coin fault types.</summary>
    [Flags]
    public enum CoinFaultTypes
    {
        /// <summary>No fault.</summary>
        [Description("None")]
        [ErrorGuid("{47B2E969-4B8D-47F7-84A6-A1068AF08993}")]
        None = 0x0000,

        /// <summary>Optic fault.</summary>
        [Description("Optic Fault")]
        [ErrorGuid("{4BC2DEC7-19C2-4E60-A7AF-F4F2FE4CD07E}")]
        Optic = 0x0001,

        /// <summary>YoYo Fault.</summary>
        [Description("YoYo Fault")]
        [ErrorGuid("{E9E5A59E-8653-483F-A0CB-2DB7E9A33AE6}")]
        YoYo = 0x0002,

        /// <summary>Divert Error.</summary>
        [Description("Divert Fault")]
        [ErrorGuid("{29CB777C-C4FA-4EDD-9FF1-46363CFFB421}")]
        Divert = 0x0004,

        /// <summary>Invalid Error.</summary>
        [Description("Invalid")]
        [ErrorGuid("{0866AE3D-9360-4612-8D33-6D9B1B3A5D22}")]
        Invalid = 0x0008,
    }
}
