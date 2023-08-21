namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using CommConfig;

    /// <summary>
    ///     Configuration for the <see cref="CommChangeLog" /> entity
    /// </summary>
    public class CommChangeLogMap : IEntityTypeConfiguration<CommChangeLog>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<CommChangeLog> builder)
        {
            builder.ToTable(nameof(CommChangeLog));

            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ConfigurationId)
                .IsRequired();

            builder.Property(t => t.DeviceId)
                .IsRequired();

            builder.Property(t => t.TransactionId)
                .IsRequired();

            builder.Property(t => t.ApplyCondition)
                .IsRequired();

            builder.Property(t => t.DisableCondition)
                .IsRequired();

            builder.Property(t => t.StartDateTime).IsRequired(false);

            builder.Property(t => t.EndDateTime).IsRequired(false);

            builder.Property(t => t.RestartAfter)
                .IsRequired();

            builder.Property(t => t.ChangeStatus)
                .IsRequired();

            builder.Property(t => t.ChangeDateTime)
                .IsRequired();

            builder.Property(t => t.EgmActionConfirmed)
                .IsRequired();

            builder.Property(t => t.ChangeException)
                .IsRequired();

            builder.Property(t => t.ChangeData)
                .IsRequired();

            builder.HasMany(l => l.AuthorizeItems)
                .WithOne()
                .HasForeignKey(item => item.CommChangeLogId);
        }
    }
}