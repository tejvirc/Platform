namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="ProfileData" /> entity
    /// </summary>
    public class ProfileDataMap : IEntityTypeConfiguration<ProfileData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfileDataMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<ProfileData> builder)
        {
            builder.ToTable(nameof(ProfileData));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProfileType)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.Data)
                .IsRequired();
        }
    }
}