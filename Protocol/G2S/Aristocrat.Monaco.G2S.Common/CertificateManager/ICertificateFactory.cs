namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    /// <summary>
    ///     Defines interface for certificate factory that should instantiate such classes as <c>CertificateService</c>,
    ///     <c>CertificateConfigurationRepository</c> and etc.
    /// </summary>
    public interface ICertificateFactory
    {
        /// <summary>
        ///     Gets instance of <c>ICertificateService</c>.
        /// </summary>
        /// <returns>Returns instance of <c>ICertificateService</c>.</returns>
        ICertificateService GetCertificateService();
    }
}