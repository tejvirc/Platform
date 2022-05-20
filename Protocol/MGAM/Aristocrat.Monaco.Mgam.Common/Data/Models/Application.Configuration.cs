namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Application that is stored on the site-controller.
    /// </summary>
    public class ApplicationConfiguration : EntityTypeConfiguration<Application>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationConfiguration"/> class.
        /// </summary>
        public ApplicationConfiguration()
        {
            ToTable(nameof(Application));

            HasKey(t => t.Id);

            Property(t => t.ApplicationGuid)
                .IsRequired();

            Property(t => t.Name)
                .IsRequired();

            Property(t => t.Version)
                .IsRequired();
        }
    }
}
