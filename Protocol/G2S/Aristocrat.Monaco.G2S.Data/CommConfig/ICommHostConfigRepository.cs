namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Common.Storage;

    /// <summary>
    ///     Repository interface for CommHostConfig.
    /// </summary>
    public interface ICommHostConfigRepository : IRepository<CommHostConfig>
    {
        /// <summary>
        ///     Gets host config entity which includes all hosts by indexes provided.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="hostIndexes">Indexes of the hosts.</param>
        /// <returns>Host config entity which indexes all hosts by indexes provided.</returns>
        CommHostConfig GetByHostIndexes(DbContext context, IEnumerable<int> hostIndexes);

        /// <summary>
        ///     Gets the common configuration.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Host config entity which indexes all hosts and devices.</returns>
        CommHostConfig GetCommConfiguration(DbContext context);
    }
}