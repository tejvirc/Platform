namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="ProfileData" /> entity
    /// </summary>
    public class ProfileDataMap : EntityTypeConfiguration<ProfileData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfileDataMap" /> class.
        /// </summary>
        public ProfileDataMap()
        {
            ToTable("ProfileData");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ProfileType)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.Data)
                .IsRequired();
        }
    }
}