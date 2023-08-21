namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using Aristocrat.Monaco.G2S.Data.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Script" /> entity
    /// </summary>
    public class ScriptMap : IEntityTypeConfiguration<Script>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<Script> builder)
        {
            builder.ToTable(nameof(Script));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ScriptId)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.State)
                .IsRequired();

            builder.Property(t => t.ApplyCondition)
                .IsRequired();

            builder.Property(t => t.DisableCondition)
                .IsRequired();

            builder.Property(t => t.AuthorizeDateTime);

            builder.Property(t => t.ScriptException)
                .IsRequired();

            builder.Property(t => t.CompletedDateTime);

            builder.Property(t => t.StartDateTime);

            builder.Property(t => t.EndDateTime);

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.ScriptCompleteHostAcknowledged)
                .IsRequired();

            builder.Property(t => t.ReasonCode)
                .IsRequired();

            builder.Property(t => t.CommandData)
                .IsRequired();

            builder.HasMany(l => l.AuthorizeItems).WithOne()
                .HasForeignKey(item => item.ScriptChangeLogId);
        }
    }
}