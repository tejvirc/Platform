namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Storage
{
    using System.Data.Entity;
    using System.Linq;
    using Models;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Certificate repository implementation
    /// </summary>
    public class CertificateRepository : BaseRepository<Certificate>, ICertificateRepository
    {
        /// <inheritdoc />
        public Certificate GetDefault(DbContext context)
        {
            return Get(context, c => c.Default).FirstOrDefault();
        }
    }
}