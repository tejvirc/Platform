namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Storage
{
    using Models;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for certificate configuration repository.
    /// </summary>
    public interface IPkiConfigurationRepository : IRepository<PkiConfiguration>
    {
    }
}