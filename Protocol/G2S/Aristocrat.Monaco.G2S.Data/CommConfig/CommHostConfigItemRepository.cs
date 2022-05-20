namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     Comm Host Config Item Repository
    /// </summary>
    public class CommHostConfigItemRepository : BaseRepository<CommHostConfigItem>, ICommHostConfigItemRepository
    {
        /// <inheritdoc />
        public override IQueryable<CommHostConfigItem> GetAll(DbContext context)
        {
            return base.GetAll(context).Include(x => x.CommHostConfigDevices);
        }

        /// <inheritdoc />
        public CommHostConfigItem GetByHostId(DbContext context, int hostId)
        {
            return Get(context, x => x.HostId == hostId).FirstOrDefault();
        }
    }
}