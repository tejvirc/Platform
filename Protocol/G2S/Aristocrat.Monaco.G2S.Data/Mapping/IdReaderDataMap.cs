namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="IdReaderDataMap" /> entity
    /// </summary>
    public class IdReaderDataMap : EntityTypeConfiguration<IdReaderData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderDataMap" /> class. 
        /// </summary>
        public IdReaderDataMap()
        {
            ToTable("IdReaderData");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.IdNumber)
                .IsRequired();

            Property(t => t.IdType)
                .IsRequired();

            Property(t => t.IdValidDateTime)
                .IsRequired();

            Property(t => t.IdValidSource)
                .IsRequired();

            Property(t => t.IdState)
                .IsRequired();

            Property(t => t.IdPreferName)
                .IsRequired();

            Property(t => t.IdFullName)
                .IsRequired();

            Property(t => t.IdClass)
                .IsRequired();

            Property(t => t.LocaleId)
                .IsRequired();

            Property(t => t.PlayerId)
                .IsRequired();

            Property(t => t.IdValidExpired)
                .IsRequired();

            Property(t => t.IdVip)
                .IsRequired();

            Property(t => t.IdBirthday)
                .IsRequired();

            Property(t => t.IdAnniversary)
                .IsRequired();

            Property(t => t.IdBanned)
                .IsRequired();

            Property(t => t.IdPrivacy)
                .IsRequired();

            Property(t => t.IdGender)
                .IsRequired();

            Property(t => t.IdRank)
                .IsRequired();

            Property(t => t.IdAge)
                .IsRequired();

            Property(t => t.IdDisplayMessages)
                .IsRequired();
        }
    }
}
