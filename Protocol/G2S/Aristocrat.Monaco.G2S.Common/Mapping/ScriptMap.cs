namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using PackageManager.Storage;

    /// <summary>
    ///     Configuration for the <see cref="Script" /> entity
    /// </summary>
    public class ScriptMap : EntityTypeConfiguration<Script>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptMap" /> class.
        /// </summary>
        public ScriptMap()
        {
            ToTable("Script");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ScriptId)
                .IsRequired();

            Property(t => t.DeviceId)
                .IsRequired();

            Property(t => t.State)
                .IsRequired();

            Property(t => t.ApplyCondition)
                .IsRequired();

            Property(t => t.DisableCondition)
                .IsRequired();

            Property(t => t.AuthorizeDateTime)
                .IsOptional();

            Property(t => t.ScriptException)
                .IsRequired();

            Property(t => t.CompletedDateTime)
                .IsOptional();

            Property(t => t.StartDateTime)
                .IsOptional();

            Property(t => t.EndDateTime)
                .IsOptional();

            Property(t => t.TransactionId)
                .IsRequired();

            Property(t => t.ScriptCompleteHostAcknowledged)
                .IsRequired();

            Property(t => t.ReasonCode)
                .IsRequired();

            Property(t => t.CommandData)
                .IsRequired();

            HasMany(l => l.AuthorizeItems)
                .WithOptional()
                .HasForeignKey(item => item.ScriptChangeLogId);
        }
    }
}