namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatSpecialFunctionParameter" /> entity
    /// </summary>
    public class GatSpecialFunctionParameterMap : IEntityTypeConfiguration<GatSpecialFunctionParameter>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionParameterMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<GatSpecialFunctionParameter> builder)
        {
            builder.ToTable(nameof(GatSpecialFunctionParameter));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Parameter)
                .IsRequired();
        }
    }
}