namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatSpecialFunction" /> entity
    /// </summary>
    public class GatSpecialFunctionMap : IEntityTypeConfiguration<GatSpecialFunction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<GatSpecialFunction> builder)
        {
            builder.ToTable("GatSpecialFunction");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Feature)
                .IsRequired();

            builder.Property(t => t.GatExec)
                .IsRequired();

            builder.HasMany(l => l.Parameters)
                .WithOne()
                .HasForeignKey(item => item.GatSpecialFunctionId);
        }
    }
}