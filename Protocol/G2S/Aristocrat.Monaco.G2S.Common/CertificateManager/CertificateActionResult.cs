namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     Represents certificate action result.
    /// </summary>
    public class CertificateActionResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateActionResult" /> class.
        /// </summary>
        /// <param name="requestData">CSR request data.</param>
        /// <param name="signingCertificate">The signing certificate</param>
        /// <param name="status">Result status.</param>
        public CertificateActionResult(
            byte[] requestData,
            X509Certificate2 signingCertificate,
            CertificateRequestStatus status)
        {
            RequestData = requestData;
            SigningCertificate = signingCertificate;
            Status = status;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateActionResult" /> class.
        /// </summary>
        /// <param name="certificate">Certificate instance.</param>
        /// <param name="status">Result status.</param>
        public CertificateActionResult(X509Certificate2 certificate, CertificateRequestStatus status)
        {
            Certificate = certificate;
            Status = status;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateActionResult" /> class.
        /// </summary>
        /// <param name="status">Result status.</param>
        public CertificateActionResult(CertificateRequestStatus status)
        {
            Status = status;
        }

        /// <summary>
        ///     Gets the certificate result.
        /// </summary>
        public X509Certificate2 Certificate { get; }

        /// <summary>
        ///     Gets the current status.
        /// </summary>
        public CertificateRequestStatus Status { get; }

        /// <summary>
        ///     Gets request data
        /// </summary>
        /// <remarks>
        ///     In case Status is Pending then this request data should be used to get certificate status update.
        /// </remarks>
        public byte[] RequestData { get; }

        /// <summary>
        ///     Gets the signing certificate.
        /// </summary>
        public X509Certificate2 SigningCertificate { get; }
    }
}