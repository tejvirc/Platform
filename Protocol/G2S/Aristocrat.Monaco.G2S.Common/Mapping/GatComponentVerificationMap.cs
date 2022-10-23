namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatComponentVerification" /> entity
    /// </summary>
    public class GatComponentVerificationMap : IEntityTypeConfiguration<GatComponentVerification>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatComponentVerificationMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<GatComponentVerification> builder)
        {
            builder.ToTable("ComponentVerification");

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.AlgorithmType)
                .IsRequired();

            builder.Property(t => t.Seed)
                .IsRequired();

            builder.Property(t => t.Salt)
                .IsRequired();

            builder.Property(t => t.StartOffset)
                .IsRequired();

            builder.Property(t => t.EndOffset)
                .IsRequired();

            builder.Property(t => t.State)
                .IsRequired();

            builder.Property(t => t.Result)
                .IsRequired();

            builder.Property(t => t.GatExec)
                .IsRequired();
        }
    }
}
