namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Model configuration for an Application that is stored on the site-controller.
    /// </summary>
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.ToTable(nameof(Application));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ApplicationGuid)
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired();

            builder.Property(t => t.Version)
                .IsRequired();
        }
    }
}
