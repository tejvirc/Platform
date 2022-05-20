namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Scep
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using Models;
    using Org.BouncyCastle.Asn1;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.X509;

    /// <summary>
    ///     Simple Certificate Enrollment Protocol service implementation.
    /// </summary>
    public class ScepClient
    {
        /// <summary>
        ///     Enrolls a new certificate.
        /// </summary>
        /// <param name="signingCertificate">Certificate that should be used to sign PKCS7 message.</param>
        /// <param name="configuration">CA configuration.</param>
        /// <param name="secret">The pre-shared secret associated with this enrollment request</param>
        /// <returns>Returns enrollment operation result.</returns>
        public CertificateActionResult Enroll(
            X509Certificate2 signingCertificate,
            PkiConfiguration configuration,
            string secret)
        {
            if (signingCertificate == null)
            {
                throw new ArgumentNullException(nameof(signingCertificate));
            }

            var attributes = new Dictionary<DerObjectIdentifier, string>
            {
                { X509Name.CN, configuration.CommonName },
                { X509Name.OU, configuration.OrganizationUnit }
            };

            var subject = new X509Name(attributes.Keys.ToList(), attributes);

            using (var client = new WebClient { BaseAddress = configuration.CertificateManagerLocation })
            {
                var request = ScepHelper.GeneratePkcs10CertificationRequest(configuration.KeySize, subject, secret);

                return Enroll(client, signingCertificate, configuration, request);
            }
        }

        /// <summary>
        ///     Renews an existing certificate.
        /// </summary>
        /// <param name="issuedCertificate">The issued certificate.</param>
        /// <param name="configuration">CA configuration.</param>
        /// <returns>An CertificateActionResult</returns>
        public CertificateActionResult Renew(X509Certificate2 issuedCertificate, PkiConfiguration configuration)
        {
            if (issuedCertificate == null)
            {
                throw new ArgumentNullException(nameof(issuedCertificate));
            }

            var subject =
                PrincipalUtilities.GetSubjectX509Principal(DotNetUtilities.FromX509Certificate(issuedCertificate));

            using (var client = new WebClient { BaseAddress = configuration.CertificateManagerLocation })
            {
                var request = ScepHelper.GeneratePkcs10CertificationRequest(
                    configuration.KeySize,
                    subject,
                    string.Empty);

                return Enroll(client, issuedCertificate, configuration, request);
            }
        }

        /// <summary>
        ///     Checks enroll status of specified certificate
        /// </summary>
        /// <param name="previousRequest">The previous request</param>
        /// <param name="signingCertificate">The signing certificate</param>
        /// <param name="configuration">CA configuration.</param>
        /// <returns>Returns certificate status.</returns>
        public CertificateActionResult CheckStatus(
            byte[] previousRequest,
            X509Certificate2 signingCertificate,
            PkiConfiguration configuration)
        {
            using (var client = new WebClient { BaseAddress = configuration.CertificateManagerLocation })
            {
                var certificateAuthorityChain =
                    ScepHelper.GetCertificateAuthorityFromServer(client, configuration.ScepCaIdent);

                if (certificateAuthorityChain == null)
                {
                    return new CertificateActionResult(CertificateRequestStatus.Error);
                }

                var response = ScepHelper.SubmitRequestToScep(client, previousRequest);

                return ScepHelper.ParseScepResponse(
                    certificateAuthorityChain,
                    response,
                    previousRequest,
                    signingCertificate);
            }
        }

        /// <summary>
        ///     Gets the certificate chaining from the certificate manager.
        /// </summary>
        /// <param name="configuration">Certificate configuration</param>
        /// <returns>the certificate chain.</returns>
        public X509Certificate2Collection GetCaCertificateChain(PkiConfiguration configuration)
        {
            using (var client = new WebClient { BaseAddress = configuration.CertificateManagerLocation })
            {
                return ScepHelper.GetCertificateAuthorityFromServer(client, configuration.ScepCaIdent);
            }
        }

        /// <summary>
        ///     Gets certificate authority.
        /// </summary>
        /// <param name="certificateManagerLocation">The address of the certificate manager</param>
        /// <returns>Returns certificate authority.</returns>
        public string GetCaCertificateThumbprint(string certificateManagerLocation)
        {
            using (var client = new WebClient { BaseAddress = certificateManagerLocation })
            {
                var certificateAuthorityChain = ScepHelper.GetCertificateAuthorityFromServer(client);
                if (certificateAuthorityChain == null)
                {
                    return string.Empty;
                }

                var cert = ScepHelper.GetRootCertificateFromChain(certificateAuthorityChain);
                if (cert == null)
                {
                    return string.Empty;
                }

                // The NDES Device Enrollment page displays the MD5 of the cert, but G2S requires the SHA1
                using (var hasher = SHA1.Create())
                {
                    var hash = hasher.ComputeHash(cert.RawData);

                    return BitConverter.ToString(hash).ToUpper(CultureInfo.InvariantCulture).Replace(@"-", @" ");
                }
            }
        }

        private static CertificateActionResult Enroll(
            WebClient client,
            X509Certificate2 signingCertificate,
            PkiConfiguration configuration,
            string request)
        {
            var certificateAuthorityChain =
                ScepHelper.GetCertificateAuthorityFromServer(client, configuration.ScepCaIdent);
            if (certificateAuthorityChain == null)
            {
                return new CertificateActionResult(CertificateRequestStatus.Error);
            }

            var encryptedMessageEnvelope = ScepHelper.GetPkcs7Envelope(request, certificateAuthorityChain);

            var encryptedAndSignedMessageEnvelope =
                ScepHelper.GetSignedPkcs7Envelope(encryptedMessageEnvelope, signingCertificate);

            var response = ScepHelper.SubmitRequestToScep(client, encryptedAndSignedMessageEnvelope);

            return ScepHelper.ParseScepResponse(
                certificateAuthorityChain,
                response,
                encryptedAndSignedMessageEnvelope,
                signingCertificate);
        }
    }
}