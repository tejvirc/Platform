namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     Repository interface for OptionConfigItem.
    /// </summary>
    public interface IOptionConfigItemRepository : IRepository<OptionConfigItem>
    {
        /// <summary>
        ///     Gets the by by identifiers.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="optionGroupId">The option group identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>OptionConfigItem or null if not exists</returns>
        OptionConfigItem GetByByIdentifiers(DbContext context, string optionId, string optionGroupId, int deviceId);

        /// <summary>
        ///     Gets the by option identifier.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="optionIds">The option identifiers.</param>
        /// <returns>Option config items</returns>
        IQueryable<OptionConfigItem> GetByOptionIds(DbContext context, string[] optionIds);
    }
}