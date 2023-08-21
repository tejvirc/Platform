namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Storage
{
    using Models;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Certificate configuration repository implementation
    /// </summary>
    public class PkiConfigurationRepository : BaseRepository<PkiConfiguration>,
        IPkiConfigurationRepository
    {
    }
}