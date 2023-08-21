namespace Aristocrat.Monaco.G2S.Data.Profile
{
    using Microsoft.EntityFrameworkCore;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base interface for profile data repository.
    /// </summary>
    public interface IProfileDataRepository : IRepository<ProfileData>
    {
        /// <summary>
        ///     Gets single profile data by keys like profile type and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="profileType">Profile type full name.</param>
        /// <param name="deviceId">Device id.</param>
        /// <returns>Returns profile data or null.</returns>
        ProfileData Get(DbContext context, string profileType, int deviceId);
    }
}