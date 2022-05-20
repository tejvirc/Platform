namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Model configuration for an Installation that is stored on the site-controller.
    /// </summary>
    public class InstallationConfiguration : EntityTypeConfiguration<Installation>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstallationConfiguration"/> class.
        /// </summary>
        public InstallationConfiguration()
        {
            ToTable(nameof(Installation));

            HasKey(t => t.Id);

            Property(t => t.InstallationGuid)
                .IsRequired();

            Property(t => t.Name)
                .IsRequired();
        }
    }
}
