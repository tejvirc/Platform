namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Storage
{
    using Microsoft.EntityFrameworkCore; 
    using Models;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for certificate repository.
    /// </summary>
    public interface ICertificateRepository : IRepository<Certificate>
    {
        /// <summary>
        ///     Gets the default certificate
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>Gets the default certificate or null if one doesn't exist</returns>
        Certificate GetDefault(DbContext context);
    }
}