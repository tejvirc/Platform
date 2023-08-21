namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Models;
    using Monaco.Common.Models;

    /// <summary>
    ///     Base interface for certificate service.
    /// </summary>
    public interface ICertificateService
    {
        /// <summary>
        ///     Gets certificate authority.
        /// </summary>
        /// <param name="certificateManagerLocation">The address of the certificate manager.</param>
        /// <returns>Returns certificate authority.</returns>
        string GetCaCertificateThumbprint(string certificateManagerLocation);

        /// <summary>
        ///     Enrolls a new certificate.
        /// </summary>
        /// <param name="secret">The pre-shared secret associated with this enrollment request</param>
        /// <returns>Returns enrollment operation result.</returns>
        CertificateActionResult Enroll(string secret);

        /// <summary>
        ///     Checks enroll status of specified certificate
        /// </summary>
        /// <param name="requestData">Request data used in the initial enrollment request.</param>
        /// <param name="signingCertificate">The certificate used to sign the original request.</param>
        /// <returns>Returns certificate status.</returns>
        CertificateActionResult Poll(byte[] requestData, X509Certificate2 signingCertificate);

        /// <summary>
        ///     Gets the date for the next renewal
        /// </summary>
        /// <returns>Returns the date/time the current certificate should be renewed.</returns>
        DateTime NextRenewal();

        /// <summary>
        ///     Renews an existing certificate.
        /// </summary>
        /// <returns>Returns renew operation result.</returns>
        CertificateActionResult Renew();

        /// <summary>
        ///     Renews an existing certificate.
        /// </summary>
        /// <returns>Returns the date/time the current certificate should be exchanged for the new certificate.</returns>
        DateTime NextExchange();

        /// <summary>
        ///     Exchanges the current certificate.
        /// </summary>
        /// <param name="certificate">Certificate to be installed.</param>
        void Exchange(X509Certificate2 certificate);

        /// <summary>
        ///     Installs specified certificate to local machine.
        /// </summary>
        /// <param name="certificate">Certificate to be installed.</param>
        /// <param name="isDefault">True if this is the default certificate</param>
        /// <returns>The result.</returns>
        Certificate InstallCertificate(X509Certificate2 certificate, bool isDefault = false);

        /// <summary>
        ///     Removes certificate.
        /// </summary>
        /// <param name="certificate">Certificate to be removed.</param>
        /// <returns>Returns true in case certificate has been removed. In other case - false.</returns>
        bool RemoveCertificate(X509Certificate2 certificate);

        /// <summary>
        ///     Returns the current certificate status
        /// </summary>
        /// <returns>A certificate status model</returns>
        GetCertificateStatusResult GetCertificateStatus();

        /// <summary>
        ///     Returns the current certificate status of the provided certificate
        /// </summary>
        /// <returns>The certificate status</returns>
        GetCertificateStatusResult GetCertificateStatus(Certificate certificate);

        /// <summary>
        ///     Gets the collections of valid certificates
        /// </summary>
        /// <returns>a collection of Certificate objects</returns>
        IList<Certificate> GetCertificates();

        /// <summary>
        ///     Determines if a valid, default certificate exists
        /// </summary>
        /// <returns>true if there is a valid, default certificate</returns>
        bool IsEnrolled();

        /// <summary>
        ///     Determines if certificate management is enabled
        /// </summary>
        /// <returns>true if there is a valid, default certificate</returns>
        bool IsCertificateManagementEnabled();

        /// <summary>
        ///     Verifies the status of the default certificate, if any
        /// </summary>
        /// <returns>true if there is a valid, default certificate</returns>
        bool HasValidCertificate();

        /// <summary>
        ///     Gets certificate configuration.
        /// </summary>
        /// <returns>Returns certificate configuration.</returns>
        PkiConfiguration GetConfiguration();

        /// <summary>
        ///     Saves or updates certificate configuration.
        /// </summary>
        /// <param name="configuration">Certificate configuration.</param>
        /// <returns>A SaveEntityResult</returns>
        SaveEntityResult SaveConfiguration(PkiConfiguration configuration);

        /// <summary>
        ///     Returns the the result of an OCSP test
        /// </summary>
        /// <returns>A certificate status model</returns>
        OcspQueryResult TestOcsp();
    }
}