namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatSpecialFunction" /> entity
    /// </summary>
    public class GatSpecialFunctionMap : EntityTypeConfiguration<GatSpecialFunction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionMap" /> class.
        /// </summary>
        public GatSpecialFunctionMap()
        {
            ToTable("GatSpecialFunction");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.Feature)
                .IsRequired();

            Property(t => t.GatExec)
                .IsRequired();

            HasMany(l => l.Parameters)
                .WithOptional()
                .HasForeignKey(item => item.GatSpecialFunctionId);
        }
    }
}