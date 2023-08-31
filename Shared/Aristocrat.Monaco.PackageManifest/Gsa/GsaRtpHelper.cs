namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using Common.Helpers;

    /// <summary>
    ///     GSA Manifest ReturnToPlayer related helper functions
    /// </summary>
    public static class GsaRtpHelper
    {
        /// <summary>
        ///     Converts a non-normalized raw RTP contained in a decimal type to a normalized RTP percentage.
        ///     This helper is used to abstract out the different ways RTP has been stored in the manifest,
        ///     allowing for backwards compatibility.
        /// </summary>
        /// <param name="rawRtp">The non-normalized RTP value.</param>
        /// <returns>A normalized RTP value as a decimal percent.</returns>
        public static decimal NormalizeRtp(decimal rawRtp)
        {
            if (rawRtp == 0)
            {
                return 0m;
            }

            // Newest manifests have true percentages already (normalized). No need to have the decimal injected.
            var numberOfDecimalPlaces = MathHelper.CountDecimalPlaces(rawRtp);
            if (numberOfDecimalPlaces > 0)
            {
                return rawRtp;
            }

            var rtpConvertedToLong = Convert.ToInt64(rawRtp);

            // Older manifests contained a truncated Rtp (precision of 2), represented as 9821 (98.21%).
            var rtpDigitCount = MathHelper.CountDigits(rtpConvertedToLong);
            if (rtpDigitCount == 4)
            {
                return rawRtp / 100;
            }

            // Newer manifests contained a truncated Rtp (precision of 3), represented as 98212 (98.212%).
            if (rtpDigitCount == 5)
            {
                return rawRtp / 1000;
            }

            // No version of the manifests support more than 5 digits
            if (rtpDigitCount > 5)
            {
                throw new Exception($"Unsupported number of digits for serialized RTP: ({rtpDigitCount} digits)");
            }

            // The remaining cases are the newest manifests which have true percentages already (normalized).
            // e.g. "1.5", "1.0", "0.5", 10.0" RTPs like these are usually progressive contributions.
            return rawRtp;
        }

        /// <summary>
        ///     Serializes a previously normalized RTP decimal percent, back to a long.
        /// </summary>
        /// <remarks>
        ///     Legacy warning: This method is a compatibility helper for RTP values stored in the Platform and
        ///     GSA Manifest that are serialized into a long value. This is the legacy way of serializing RTP.
        /// </remarks>
        /// <param name="rtpPercent">The RTP percent.</param>
        /// <returns>The decimal RTP value serialized to a long.</returns>
        public static long SerializeRtpToLong(decimal rtpPercent)
        {
            // Check that the decimal RTP supports being serialized to long. It must be a total of 4 or 5 digits.
            // Essentially, we should not be converting an RTP value to long unless it previously was a long
            // from the original source, before it got normalized into a decimal percent. Otherwise there will
            // be loss of precision.
            var decimalPlaces = MathHelper.CountDecimalPlaces(rtpPercent);
            var totalDigits =
                decimalPlaces + 2; // These older long serializations always have 2 digits to left of decimal

            if (totalDigits == 4)
            {
                // Move decimal over 2 digits to the right
                return Convert.ToInt64(rtpPercent * 100);
            }

            if (totalDigits == 5)
            {
                // Move decimal over 2 digits to the right
                return Convert.ToInt64(rtpPercent * 1000);
            }

            throw new Exception("Attempting to serialize an unsupported RTP decimal value to a long.");
        }
    }
}