namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    using CaClients.Ocsp;

    /// <summary>
    ///     Get Ocsp certificate status result
    /// </summary>
    public class GetOcspCertificateStatusResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOcspCertificateStatusResult" /> class.
        /// </summary>
        /// <param name="ocspServiceStatus">The ocsp service status.</param>
        /// <param name="ocspCertificateStatus">The ocsp certificate status.</param>
        public GetOcspCertificateStatusResult(
            OcspServiceStatus ocspServiceStatus,
            OcspCertificateStatus ocspCertificateStatus)
        {
            OcspServiceStatus = ocspServiceStatus;
            OcspCertificateStatus = ocspCertificateStatus;
        }

        /// <summary>
        ///     Gets the ocsp service status.
        /// </summary>
        /// <value>
        ///     The ocsp service status.
        /// </value>
        public OcspServiceStatus OcspServiceStatus { get; }

        /// <summary>
        ///     Gets the ocsp certificate status.
        /// </summary>
        /// <value>
        ///     The ocsp certificate status.
        /// </value>
        public OcspCertificateStatus OcspCertificateStatus { get; }
    }
}