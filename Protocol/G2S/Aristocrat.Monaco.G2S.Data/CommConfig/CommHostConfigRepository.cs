namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     Comm Host Config Repository
    /// </summary>
    public class CommHostConfigRepository : BaseRepository<CommHostConfig>, ICommHostConfigRepository
    {
        /// <inheritdoc />
        public CommHostConfig GetByHostIndexes(DbContext context, IEnumerable<int> hostIndexes)
        {
            var query = GetAll(context).Include(x => x.CommHostConfigItems.Select(y => y.CommHostConfigDevices));

            // TODO review filtering.
            var indexList = hostIndexes as IList<int> ?? hostIndexes.ToList();
            if (indexList.FirstOrDefault() != -1)
            {
                var commHostConfig = query.SingleOrDefault();

                if (commHostConfig != null)
                {
                    commHostConfig.CommHostConfigItems =
                        commHostConfig.CommHostConfigItems.Where(x => indexList.Contains(x.HostIndex)).ToList();

                    return commHostConfig;
                }

                return null;
            }

            return query.SingleOrDefault();
        }

        /// <inheritdoc />
        public CommHostConfig GetCommConfiguration(DbContext context)
        {
            var query = GetAll(context).Include(x => x.CommHostConfigItems.Select(y => y.CommHostConfigDevices));
            return query.SingleOrDefault();
        }
    }
}