namespace Aristocrat.Monaco.G2S.Data.Hosts
{
    using Microsoft.EntityFrameworkCore;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Repository interface for Host.
    /// </summary>
    public interface IHostRepository : IRepository<Host>
    {
        /// <summary>
        ///     Gets the by host identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="hostId">The host identifier.</param>
        /// <returns>
        ///     Host entity.
        /// </returns>
        Host GetByHostId(DbContext context, int hostId);
    }
}