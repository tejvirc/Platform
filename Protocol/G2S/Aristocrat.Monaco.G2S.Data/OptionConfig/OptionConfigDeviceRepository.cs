namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///   OptionConfigDeviceRepository  
    /// </summary>
    public class OptionConfigDeviceRepository : BaseRepository<OptionConfigDeviceEntity>, IOptionConfigDeviceRepository
    {
        /// <inheritdoc />
        public override IQueryable<OptionConfigDeviceEntity> GetAll(DbContext context)
        {
            var devices = base.GetAll(context)
                .Include("OptionConfigGroups.OptionConfigItems");

            return devices;
        }

        /// <inheritdoc />
        public IQueryable<OptionConfigDeviceEntity> GetDevicesByFilter(
            DbContext context,
            DeviceClass deviceClass,
            int deviceId)
        {
            var devices = GetAll(context);

            if (deviceClass != DeviceClass.All)
            {
                devices = devices.Where(x => x.DeviceClass == deviceClass);
            }

            if (deviceId != -1)
            {
                devices = devices.Where(x => x.DeviceId == deviceId);
            }

            return devices;
        }
    }
}