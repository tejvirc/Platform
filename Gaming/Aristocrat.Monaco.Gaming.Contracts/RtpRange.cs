namespace Aristocrat.Monaco.Gaming.Contracts
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
}
