namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using OptionConfig;

    /// <summary>
    ///     Configuration for the <see cref="OptionChangeLog" /> entity
    /// </summary>
    public class OptionChangeLogMap : EntityTypeConfiguration<OptionChangeLog>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeLogMap" /> class.
        /// </summary>
        public OptionChangeLogMap()
        {
            ToTable("OptionChangeLog");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.ConfigurationId)
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

            Property(t => t.EgmActionConfirmed)
                .IsRequired();

            Property(t => t.ChangeDateTime)
                .IsRequired();

            Property(t => t.ChangeException)
                .IsRequired();

            Property(t => t.ChangeData)
                .IsRequired();

            HasMany(l => l.AuthorizeItems)
                .WithOptional()
                .HasForeignKey(item => item.OptionChangeLogId);
        }
    }
}