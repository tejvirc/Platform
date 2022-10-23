namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Supported event repository implementation.
    /// </summary>
    public class SupportedEventRepository : BaseRepository<SupportedEvent>, ISupportedEventRepository
    {
        /// <inheritdoc />
        public SupportedEvent Get(DbContext context, string eventCode, int deviceId)
        {
            return context.Set<SupportedEvent>().FirstOrDefault(
                item => item.DeviceId == deviceId && item.EventCode == eventCode);
        }

        /// <inheritdoc />
        public void Delete(DbContext context, string eventCode, int deviceId)
        {
            var record = context.Set<SupportedEvent>().FirstOrDefault(
                item => item.DeviceId == deviceId && item.EventCode == eventCode);

            if (record != null)
            {
                Delete(context, record);
            }
        }
    }
}