namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    /// <summary>
    ///     Pki Constants
    /// </summary>
    public static class PkiConstants
    {
        /// <summary>
        ///     Defines the minimum random period for OCSP service requests/attempts
        /// </summary>
        public const int DefaultMinimumPeriod = 5;

        /// <summary>
        ///     Defines the maximum random period for OCSP service requests/attempts
        /// </summary>
        public const int DefaultMaximumPeriod = 10;
    }
}