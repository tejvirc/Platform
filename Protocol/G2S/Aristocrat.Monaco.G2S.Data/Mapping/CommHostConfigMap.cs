namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using CommConfig;

    /// <summary>
    ///     Configuration for the <see cref="CommHostConfig" /> entity
    /// </summary>
    public class CommHostConfigMap : EntityTypeConfiguration<CommHostConfig>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigMap" /> class.
        /// </summary>
        public CommHostConfigMap()
        {
            ToTable("CommHostConfig");

            // Primary Key
            HasKey(t => t.Id);

            HasMany(x => x.CommHostConfigItems)
                .WithRequired()
                .HasForeignKey(item => item.CommHostConfigId);
        }
    }
}