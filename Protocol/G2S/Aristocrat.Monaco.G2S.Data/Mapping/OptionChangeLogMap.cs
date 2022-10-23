namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionChangeLog" /> entity
    /// </summary>
    public class OptionChangeLogMap : IEntityTypeConfiguration<OptionChangeLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeLogMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<OptionChangeLog> builder)
        {
            builder.ToTable(nameof(OptionChangeLog));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.ConfigurationId)
                .IsRequired();

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.ApplyCondition)
                .IsRequired();

            builder.Property(t => t.DisableCondition)
                .IsRequired();

            builder.Property(t => t.StartDateTime).IsRequired(false);

            builder.Property(t => t.EndDateTime).IsRequired(false);

            builder.Property(t => t.RestartAfter).IsRequired();

            builder.Property(t => t.ChangeStatus)
                .IsRequired();

            builder.Property(t => t.EgmActionConfirmed)
                .IsRequired();

            builder.Property(t => t.ChangeDateTime)
                .IsRequired();

            builder.Property(t => t.ChangeException)
                .IsRequired();

            builder.Property(t => t.ChangeData)
                .IsRequired();

            builder.HasMany(l => l.AuthorizeItems)
                .WithOne()
                .HasForeignKey(item => item.OptionChangeLogId);
        }
    }
}