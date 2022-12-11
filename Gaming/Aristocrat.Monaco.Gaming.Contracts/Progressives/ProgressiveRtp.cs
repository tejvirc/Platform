namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of Rtp Range
    /// </summary>
    [ProtoContract]
    public class RtpRange
    {
        /// <summary>
        ///     Constructor for an Rtp Range object
        /// </summary>
        public RtpRange(decimal min, decimal max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public RtpRange()
        {
        }

        /// <summary>
        ///     Gets or sets the minimum rtp
        /// </summary>
        [ProtoMember(1)]
        public decimal Minimum { get; }

        /// <summary>
        ///     Gets or sets the maximum rtp
        /// </summary>
        [ProtoMember(2)]
        public decimal Maximum { get; }

        /// <summary>
        ///     Returns the string representation of the rtp range
        /// </summary>
        public string GetRtpString()
        {
            return $"{Minimum.GetRtpString()} - {Maximum.GetRtpString()}";
        }

        /// <summary>
        ///     Returns the check for equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is RtpRange rtpRange && rtpRange.Minimum == Minimum && rtpRange.Maximum == Maximum;
        }

        /// <summary>
        ///     Returns the hash code
        /// </summary>
        public override int GetHashCode()
        {
            return Convert.ToInt32(Minimum);
        }
    }

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
