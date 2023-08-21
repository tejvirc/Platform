namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Ocsp
{
    /// <summary>
    ///     Outlines available certificate statuses for Online Certificate Status Protocol.
    /// </summary>
    public enum OcspCertificateStatus
    {
        /// <summary>
        ///     The good
        /// </summary>
        Good = 1,

        /// <summary>
        ///     The revoked
        /// </summary>
        Revoked = 2,

        /// <summary>
        ///     The unknown
        /// </summary>
        Unknown = 3
    }
}