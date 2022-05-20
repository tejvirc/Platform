namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     Definition of Rtp Range
    /// </summary>
    [Serializable]
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
        ///     Gets or sets the minimum rtp
        /// </summary>
        public decimal Minimum { get; }

        /// <summary>
        ///     Gets or sets the maximum rtp
        /// </summary>
        public decimal Maximum { get; }

        /// <summary>
        ///     Returns the string representation of the rtp
        /// </summary>
        public override string ToString()
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
    [Serializable]
    public class ProgressiveRtp
    {
        /// <summary>
        ///     Gets or sets the reset rtp
        /// </summary>
        public RtpRange Reset { get; set; }

        /// <summary>
        ///     Gets or sets the increment rtp
        /// </summary>
        public RtpRange Increment { get; set; }

        /// <summary>
        ///     Gets or sets the base and reset rtp
        /// </summary>
        public RtpRange BaseAndReset { get; set; }

        /// <summary>
        ///     Gets or sets the base and reset and increment rtp
        /// </summary>
        public RtpRange BaseAndResetAndIncrement { get; set; }
    }
}
