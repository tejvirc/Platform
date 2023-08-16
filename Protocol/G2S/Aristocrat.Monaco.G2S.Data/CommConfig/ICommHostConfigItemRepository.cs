namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using Microsoft.EntityFrameworkCore;
    using Common.Storage;

    /// <summary>
    ///     Repository interface for CommHostConfigItem.
    /// </summary>
    public interface ICommHostConfigItemRepository : IRepository<CommHostConfigItem>
    {
        /// <summary>
        ///     Gets the by host identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="hostId">The host identifier.</param>
        /// <returns>Comm host item entity.</returns>
        CommHostConfigItem GetByHostId(DbContext context, int hostId);
    }
}