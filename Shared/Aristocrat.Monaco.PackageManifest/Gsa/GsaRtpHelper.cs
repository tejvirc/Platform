namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using Common.Helpers;

    /// <summary>
    ///     GSA Manifest ReturnToPlayer related helper functions
    /// </summary>
    public static class GsaRtpHelper
    {
        private const int LegacyRtpDigitCount4 = 4;
        private const int LegacyRtpDigitCount5 = 5;

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
            if (rtpDigitCount == LegacyRtpDigitCount4)
            {
                return rawRtp / 100;
            }

            // Newer manifests contained a truncated Rtp (precision of 3), represented as 98212 (98.212%).
            if (rtpDigitCount == LegacyRtpDigitCount5)
            {
                return rawRtp / 1000;
            }

            // No version of the manifests support more than 5 digits
            if (rtpDigitCount > LegacyRtpDigitCount5)
            {
                throw new RtpValueException($"Unsupported number of digits for serialized RTP value ({rtpDigitCount} digits)");
            }

            // The remaining cases are the newest manifests which have true percentages already (normalized).
            // e.g. "1.5", "1.0", "0.5", 10.0" RTPs like these are usually progressive contributions.
            return rawRtp;
        }
    }
}