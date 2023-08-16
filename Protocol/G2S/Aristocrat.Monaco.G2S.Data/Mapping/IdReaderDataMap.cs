namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="IdReaderDataMap" /> entity
    /// </summary>
    public class IdReaderDataMap : IEntityTypeConfiguration<IdReaderData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderDataMap" /> class. 
        /// </summary>
        public void Configure(EntityTypeBuilder<IdReaderData> builder)
        {
            builder.ToTable(nameof(IdReaderData));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.IdNumber)
                .IsRequired();

            builder.Property(t => t.IdType)
                .IsRequired();

            builder.Property(t => t.IdValidDateTime)
                .IsRequired();

            builder.Property(t => t.IdValidSource)
                .IsRequired();

            builder.Property(t => t.IdState)
                .IsRequired();

            builder.Property(t => t.IdPreferName)
                .IsRequired();

            builder.Property(t => t.IdFullName)
                .IsRequired();

            builder.Property(t => t.IdClass)
                .IsRequired();

            builder.Property(t => t.LocaleId)
                .IsRequired();

            builder.Property(t => t.PlayerId)
                .IsRequired();

            builder.Property(t => t.IdValidExpired)
                .IsRequired();

            builder.Property(t => t.IdVip)
                .IsRequired();

            builder.Property(t => t.IdBirthday)
                .IsRequired();

            builder.Property(t => t.IdAnniversary)
                .IsRequired();

            builder.Property(t => t.IdBanned)
                .IsRequired();

            builder.Property(t => t.IdPrivacy)
                .IsRequired();

            builder.Property(t => t.IdGender)
                .IsRequired();

            builder.Property(t => t.IdRank)
                .IsRequired();

            builder.Property(t => t.IdAge)
                .IsRequired();

            builder.Property(t => t.IdDisplayMessages)
                .IsRequired();
        }
    }
}
