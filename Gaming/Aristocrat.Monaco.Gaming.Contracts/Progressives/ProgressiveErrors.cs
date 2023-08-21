namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using Hardware.Contracts;

    /// <summary>
    ///     Various error states of a progressive level
    /// </summary>
    [Flags]
    public enum ProgressiveErrors
    {
        /// <summary>
        ///     Indicates there are no progressive errors associated
        ///     with this progressive level.
        /// </summary>
        [ErrorGuid("{A3591A28-910B-44B4-BE6B-2EBDCD532FBE}")]
        None = 0x0000,

        /// <summary>
        ///     Indicates that a progressive level has not been updated
        ///     within an expected time interval.
        /// </summary>
        [ErrorGuid("{FA358CB0-4FCD-4028-BE64-8FD2FD9949E6}")]
        ProgressiveUpdateTimeout = 0x0001,

        /// <summary>
        ///     TODO: What is this error for?
        /// </summary>
        [ErrorGuid("{7F56E695-5DE1-4BE5-91BE-293FA3B42741}")]
        ProgressiveMismatch = 0x0002,

        /// <summary>
        ///     Indicates there is an error with progressive configuration
        /// </summary>
        [ErrorGuid("{1AAEC3E3-869F-46FC-997C-272E507C2805}")]
        ConfigurationError = 0x0004,

        /// <summary>
        ///     Indicates that a progressive level with a pending win
        ///     was not committed or paid out within an expected interval
        /// </summary>
        [ErrorGuid("{C153E5DA-4311-465A-8739-9B63F2F6705D}")]
        ProgCommitTimeout = 0x0008,

        /// <summary>
        ///     Indicates that a progressive link is down disconnected.
        /// </summary>
        [ErrorGuid("{782B18C4-9C9C-43C6-93C8-B50BF783A944}")]
        ProgressiveDisconnected = 0x0010,

        /// <summary>
        ///     Indicates that the current value for the progressive level does
        ///     meet the minimum value requirements.
        /// </summary>
        [ErrorGuid("{e1904c57-648d-4d8d-9cdc-f2ac7edd1ddd}")]
        MinimumThresholdNotReached = 0x0020,

        /// <summary>
        ///     Indicates that a progressive level does not meet RTP requirements
        ///     for a given jurisdiction
        /// </summary>
        [ErrorGuid("{e691139f-7555-41de-b356-54afe85c4b53}")]
        ProgressiveRtpError = 0x0040
    }
}