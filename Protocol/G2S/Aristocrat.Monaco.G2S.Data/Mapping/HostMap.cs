namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="Host" /> entity
    /// </summary>
    public class HostMap : EntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostMap" /> class.
        /// </summary>
        public HostMap()
        {
            ToTable("Host");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.Address)
                .IsRequired();

            Property(t => t.Registered)
                .IsRequired();
        }
    }
}