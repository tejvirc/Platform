namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using ProtoBuf;
    using Rtp;

    /// <summary>
    ///     Definition of a Progressive rtp
    /// </summary>
    [ProtoContract]
    public class ProgressiveRtp
    {
        /// <summary>
        ///     Gets or sets the reset rtp
        /// </summary>
        [ProtoMember(1)]
        public RtpRange Reset { get; set; }
        /// <summary>
        ///     Gets or sets the increment rtp
        /// </summary>
        [ProtoMember(2)]
        public RtpRange Increment { get; set; }
        /// <summary>
        ///     Gets or sets the base and reset rtp
        /// </summary>
        [ProtoMember(3)]
        public RtpRange BaseAndReset { get; set; }
        /// <summary>
        ///     Gets or sets the base and reset and increment rtp
        /// </summary>
        [ProtoMember(4)]
        public RtpRange BaseAndResetAndIncrement { get; set; }
    }
}