namespace Aristocrat.Monaco.G2S.Data.Meters
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Meter subscription repository implementation.
    /// </summary>
    public class MeterSubscriptionRepository : BaseRepository<MeterSubscription>, IMeterSubscriptionRepository
    {
        /// <inheritdoc />
        public MeterSubscription Get(DbContext context, int hostId, int deviceId, string className)
        {
            return context.Set<MeterSubscription>().FirstOrDefault(
                item => item.DeviceId == deviceId && item.HostId == hostId && item.DeviceId == deviceId);
        }
    }
}