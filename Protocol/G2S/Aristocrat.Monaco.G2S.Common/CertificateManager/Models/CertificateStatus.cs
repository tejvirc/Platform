namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    /// <summary>
    ///     Certificate statuses
    /// </summary>
    public enum CertificateStatus
    {
        /// <summary>
        ///     The certificate is good
        /// </summary>
        Good,

        /// <summary>
        ///     The certificate has been revoked
        /// </summary>
        Revoked,

        /// <summary>
        ///     The certificate has expired
        /// </summary>
        Expired
    }
}