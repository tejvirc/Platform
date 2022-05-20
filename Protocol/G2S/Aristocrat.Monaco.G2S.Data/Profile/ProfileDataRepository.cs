namespace Aristocrat.Monaco.G2S.Data.Profile
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Profile data repository implementation.
    /// </summary>
    public class ProfileDataRepository : BaseRepository<ProfileData>, IProfileDataRepository
    {
        /// <inheritdoc />
        public ProfileData Get(DbContext context, string profileType, int deviceId)
        {
            return context.Set<ProfileData>()
                .SingleOrDefault(
                    item =>
                        item.DeviceId == deviceId &&
                        item.ProfileType.Equals(profileType, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}