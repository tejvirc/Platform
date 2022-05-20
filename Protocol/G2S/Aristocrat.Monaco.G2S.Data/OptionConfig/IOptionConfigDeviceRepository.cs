namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Repository interface for OptionConfigDevice.
    /// </summary>
    public interface IOptionConfigDeviceRepository : IRepository<OptionConfigDeviceEntity>
    {
        /// <summary>
        ///     Gets the option config devices filtered by device class and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="deviceClass">The device class.</param>
        /// <param name="deviceId">The device id.</param>
        /// <returns>Option config device entities.</returns>
        IQueryable<OptionConfigDeviceEntity> GetDevicesByFilter(
            DbContext context,
            DeviceClass deviceClass,
            int deviceId);
    }
}