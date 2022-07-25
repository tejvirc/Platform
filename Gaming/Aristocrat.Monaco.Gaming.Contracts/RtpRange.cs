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
        ///     Constructor for an Rtp Range object
        /// </summary>
        public RtpRange(decimal min, decimal max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        ///     Gets or sets the minimum rtp, in percent
        /// </summary>
        public decimal Minimum { get; }

        /// <summary>
        ///     Gets or sets the maximum rtp, in percent
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
        ///     Returns the hash code
        /// </summary>
        public override int GetHashCode()
        {
            return Convert.ToInt32(Minimum);
        }
    }
}
