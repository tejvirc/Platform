namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="ConfigChangeAuthorizeItem" /> entity
    /// </summary>
    public class ConfigChangeAuthorizeItemMap : EntityTypeConfiguration<ConfigChangeAuthorizeItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigChangeAuthorizeItemMap" /> class.
        /// </summary>
        public ConfigChangeAuthorizeItemMap()
        {
            ToTable("ConfigChangeAuthorizeItem");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.HostId)
                .IsRequired();

            Property(t => t.TimeoutDate)
                .IsOptional();

            Property(t => t.AuthorizeStatus)
                .IsRequired();

            Property(t => t.TimeoutAction)
                .IsRequired();
        }
    }
}