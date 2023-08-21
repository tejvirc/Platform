namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    /// <summary>
    ///     Offline method types
    /// </summary>
    public enum OfflineMethodType
    {
        /// <summary>
        ///     Option A allows an entity to treat a certificate as if the OCSP service had returned a revoked status for the
        ///     certificate. The entity does not communicate using that certificate.
        /// </summary>
        OptionA,

        /// <summary>
        ///     Option B allows an entity to use a certificate that was previously known to be good for the period of time
        ///     specified in the gsaOA parameter while OCSP services are non-responsive.
        /// </summary>
        OptionB,

        /// <summary>
        ///     When using Option C the entity MUST obtain CRLs using the CRL Distribution Points extension
        /// </summary>
        OptionC
    }
}