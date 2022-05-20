namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for <see cref="Checksum"/> model.
    /// </summary>
    public class ChecksumConfiguration : EntityTypeConfiguration<Checksum>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="ChecksumConfiguration"/> class.
        /// </summary>
        public ChecksumConfiguration()
        {
            ToTable(nameof(Checksum));

            HasKey(t => t.Id);

            Property(t => t.Seed)
                .IsRequired();
        }
    }
}
