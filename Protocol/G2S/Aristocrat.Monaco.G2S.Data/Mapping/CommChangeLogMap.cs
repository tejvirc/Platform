namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using CommConfig;

    /// <summary>
    ///     Configuration for the <see cref="CommChangeLog" /> entity
    /// </summary>
    public class CommChangeLogMap : EntityTypeConfiguration<CommChangeLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChangeLogMap" /> class.
        /// </summary>
        public CommChangeLogMap()
        {
            ToTable("CommChangeLog");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ConfigurationId)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.ApplyCondition)
                .IsRequired();

            Property(t => t.DisableCondition)
                .IsRequired();

            Property(t => t.StartDateTime)
                .IsOptional();

            Property(t => t.EndDateTime)
                .IsOptional();

            Property(t => t.RestartAfter)
                .IsRequired();

            Property(t => t.ChangeStatus)
                .IsRequired();

            Property(t => t.ChangeDateTime)
                .IsRequired();

            Property(t => t.EgmActionConfirmed)
                .IsRequired();

            Property(t => t.ChangeException)
                .IsRequired();

            Property(t => t.ChangeData)
                .IsRequired();

            HasMany(l => l.AuthorizeItems)
                .WithOptional()
                .HasForeignKey(item => item.CommChangeLogId);
        }
    }
}