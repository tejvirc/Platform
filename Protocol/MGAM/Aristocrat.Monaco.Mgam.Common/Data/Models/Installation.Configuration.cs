namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Installation that is stored on the site-controller.
    /// </summary>
    public class InstallationConfiguration : IEntityTypeConfiguration<Installation>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstallationConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Installation> builder)
        {
            builder.ToTable(nameof(Installation));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.InstallationGuid)
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired();
        }
    }
}
