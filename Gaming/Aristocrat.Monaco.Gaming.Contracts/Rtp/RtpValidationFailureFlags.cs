namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;

    /// <summary>
    ///     Flags signifying the types of failures which occurred during RTP validation.
    /// </summary>
    [Flags]
    public enum RtpValidationFailureFlags
    {
        /// <summary>
        ///     Indicating no failures
        /// </summary>
        None,

        /// <summary>
        ///     The RTP exceeds jurisdictional maximum RTP
        /// </summary>
        RtpExceedsJurisdictionalMaximum,

        /// <summary>
        ///     The RTP exceeds jurisdictional minimum RTP
        /// </summary>
        RtpExceedsJurisdictionalMinimum,

        /// <summary>
        ///     This failure occurs when the Rtp percent precision is less than the GDK5 standard of 5 decimal places.
        /// </summary>
        InsufficientRtpPrecision,

        /// <summary>
        ///     This failure occurs if any of the following scenarios hold true
        /// </summary>
        /// <remarks>
        ///     <list type="number">
        ///         <item>
        ///             The Minimum value is higher than the Maximum value
        ///         </item>
        ///         <item>
        ///             The Maximum value is lower than the Minimum value
        ///         </item>
        ///         <item>
        ///             The Maximum values are higher than 100%
        ///         </item>
        ///         <item>
        ///             The Minimum value is lower than 0%
        ///         </item>
        ///     </list>
        /// </remarks>
        InvalidRtpValue
    }
}