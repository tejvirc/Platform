namespace Aristocrat.Monaco.G2S.Data.Profile
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Common.Storage;
    using Model;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Profile data repository implementation.
    /// </summary>
    public class ProfileDataRepository : BaseRepository<ProfileData>, IProfileDataRepository
    {
        private static string SqliteIgnoreCase = "NOCASE";

        /// <inheritdoc />
        public ProfileData Get(DbContext context, string profileType, int deviceId)
        {
            return context.Set<ProfileData>()
                .SingleOrDefault(
                    item =>
                        EF.Functions.Collate(item.DeviceId, SqliteIgnoreCase) == deviceId &&
                        EF.Functions.Like(EF.Functions.Collate(item.ProfileType, SqliteIgnoreCase), $"{profileType}"));
        }
    }
}