namespace Aristocrat.Monaco.Mgam.Common.Data.Repositories
{
    using System.Linq;
    using Models;
    using Protocol.Common.Storage.Repositories;

    /// <summary>
    ///     Extension methods for the <see cref="Host"/> repository.
    /// </summary>
    public static class HostRepository
    {
        /// <summary>
        ///     Gets the VLT Service name on the site-controller.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static string GetServiceName(this IRepository<Host> repository)
        {
            return repository
                .Queryable()
                .Select(h => h.ServiceName)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Gets registration info used to register an instance with the VLT service.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static RegistrationInfo GetRegistrationInfo(this IRepository<Host> repository)
        {
            return (from host in repository.Entities.Take(1)
                from device in repository.Context.Set<Device>().Take(1)
                from installation in repository.Context.Set<Installation>().Take(1)
                from application in repository.Context.Set<Application>().Take(1)
                select new RegistrationInfo
                {
                    IcdVersion = host.IcdVersion,
                    ManufacturerName = device.ManufacturerName,
                    DeviceGuid = device.DeviceGuid,
                    DeviceName = device.Name,
                    InstallationGuid = installation.InstallationGuid,
                    InstallationName = installation.Name,
                    ApplicationGuid = application.ApplicationGuid,
                    ApplicationName = application.Name
                })
                .FirstOrDefault();
        }
    }
}
