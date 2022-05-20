namespace Aristocrat.Monaco.Hardware.Contracts.Audio
{
    using System;

    /// <summary>
    ///     Defines bit flag for speaker mix
    /// </summary>
    [Flags]
    public enum SpeakerMix : byte
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Side left speaker
        /// </summary>
        SideLeft = 0x01,

        /// <summary>
        ///     Side right speaker
        /// </summary>
        SideRight = 0x02,

        /// <summary>
        ///     Front left speaker
        /// </summary>
        FrontLeft = 0x04,

        /// <summary>
        ///     Front right speaker
        /// </summary>
        FrontRight = 0x08,

        /// <summary>
        ///     Center speaker
        /// </summary>
        Center = 0x10,

        /// <summary>
        ///     Rear left speaker
        /// </summary>
        RearLeft = 0x20,

        /// <summary>
        ///     Rear right speaker
        /// </summary>
        RearRight = 0x40,

        /// <summary>
        ///     LowFrequency speaker
        /// </summary>
        LowFrequency = 0x80,

        /// <summary>
        ///     All speakers
        /// </summary>
        All = 0xFF
    }
}