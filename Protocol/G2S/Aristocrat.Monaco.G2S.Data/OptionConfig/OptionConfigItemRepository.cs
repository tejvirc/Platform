namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;

    /// <summary>
    ///     OptionConfigItemRepository
    /// </summary>
    public class OptionConfigItemRepository : BaseRepository<OptionConfigItem>, IOptionConfigItemRepository
    {
        /// <inheritdoc />
        public OptionConfigItem GetByByIdentifiers(
            DbContext context,
            string optionId,
            string optionGroupId,
            int deviceId)
        {
            var query = Get(context, x => x.OptionId == optionId);

            query =
                query.Where(
                    x =>
                        x.OptionConfigGroup.OptionGroupId == optionGroupId
                        && x.OptionConfigGroup.OptionConfigDevice.DeviceId == deviceId);

            return query.SingleOrDefault();
        }

        /// <inheritdoc />
        public IQueryable<OptionConfigItem> GetByOptionIds(DbContext context, string[] optionIds)
        {
            return Get(context, x => optionIds.Contains(x.OptionId));
        }
    }
}