namespace Aristocrat.Monaco.G2S.Data.Meters
{
    using System.Data.Entity;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base interface for meters subscription repository.
    /// </summary>
    public interface IMeterSubscriptionRepository : IRepository<MeterSubscription>
    {
        /// <summary>
        ///     Gets single meters subscription by keys like event code and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="hostId">Host id.</param>
        /// <param name="deviceId">Device id.</param>
        /// <param name="className">Meter class name.</param>
        /// <returns>Returns meter subscription or null.</returns>
        MeterSubscription Get(DbContext context, int hostId, int deviceId, string className);
    }
}