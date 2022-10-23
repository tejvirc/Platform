namespace Aristocrat.Monaco.G2S.Data.Hosts
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Host repository
    /// </summary>
    public class HostRepository : BaseRepository<Host>, IHostRepository
    {
        /// <inheritdoc />
        public Host GetByHostId(DbContext context, int hostId)
        {
            return Get(context, x => x.HostId == hostId).FirstOrDefault();
        }
    }
}