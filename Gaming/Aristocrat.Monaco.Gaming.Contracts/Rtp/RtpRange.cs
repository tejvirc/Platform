namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;
    using System.Linq;
    using Localization.Properties;

    /// <summary>
    ///     Definition of Rtp Range, expressed in actual percentages (e.g. 91.23)
    /// </summary>
    [Serializable]
    public class RtpRange
    {
        /// <summary>
        ///     Represents a zero RTP range which essentially means "unused."
        /// </summary>
        public static RtpRange Zero = new ();

        /// <summary>
        ///     Constructs a new RTP (Return to Player) range in percent.
        /// </summary>
        public RtpRange(decimal min = decimal.Zero, decimal max = decimal.Zero)
        {
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
        ///     Totals the specified RTP ranges together. The resulting range has the lowest minimum and highest maximum of all ranges.
        /// </summary>
        /// <param name="rtpRanges">The RTP ranges to be totaled together.</param>
        /// <returns>A new <see cref="RtpRange"/> which is the total of a many ranges.</returns>
        public static RtpRange Total(params RtpRange[] rtpRanges) => rtpRanges.Aggregate((r1, r2) => r1.TotalWith(r2));

        /// <summary>
        ///     Totals this RTP range with another RTP range.
        /// </summary>
        /// <param name="otherRtp">The other RTP range.</param>
        /// <returns>A new <see cref="RtpRange"/> with the most minimum and maximum values of the two ranges.</returns>
        public RtpRange TotalWith(RtpRange otherRtp) => new(Math.Min(Minimum, otherRtp.Minimum), Math.Max(Maximum, otherRtp.Maximum));

        /// <summary>
        ///     Overloaded Addition operator +, used for finding the summation of two <see cref="RtpRange"s/>
        /// </summary>
        /// <param name="r1">RTP Range 1</param>
        /// <param name="r2">RTP Range 2</param>
        /// <returns>The sum of the two ranges</returns>
        public static RtpRange operator +(RtpRange r1, RtpRange r2) => new(r1.Minimum + r2.Minimum, r1.Maximum + r2.Maximum);

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