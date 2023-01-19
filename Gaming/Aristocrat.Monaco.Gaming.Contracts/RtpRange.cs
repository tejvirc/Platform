namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Localization.Properties;

    /// <summary>
    ///     Definition of Rtp Range, expressed in actual percentages (e.g. 91.23)
    /// </summary>
    [Serializable]
    public class RtpRange
    {
        /// <summary>
        ///     Constructs a new immutable RTP (Return to Player) range in percent.
        /// </summary>
        public RtpRange(decimal min, decimal max)
        {
            if (min > max)
            {
                throw new ArgumentException("The RTP min value is greater than the RTP max value.");
            }

            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min), "RTP percentages cannot be negative.");
            }

            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "RTP percentages cannot be negative.");
            }

            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        ///     Gets or sets the minimum RTP, in percent
        /// </summary>
        public decimal Minimum { get; }

        /// <summary>
        ///     Gets or sets the maximum RTP, in percent
        /// </summary>
        public decimal Maximum { get; }

        /// <summary>
        ///     Returns the string representation of the rtp
        /// </summary>
        public override string ToString()
        {
            if (Maximum == decimal.MaxValue && Minimum == decimal.MinValue)
            {
                return Resources.NoLimit;
            }

            if (Maximum == decimal.MaxValue)
            {
                return $"{Resources.AtLeast} {Minimum.GetRtpString()}";
            }

            if (Minimum == decimal.MinValue)
            {
                return $"{Resources.AtMost} {Maximum.GetRtpString()}";
            }

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
        ///     Returns the unique hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
            }
        }
    }
}