namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    public class ManualHandicapEntityConfiguration : EntityTypeConfiguration<ManualHandicapEntity>
    {
        public ManualHandicapEntityConfiguration()
        {
            ToTable(nameof(ManualHandicapEntity));

            HasKey(t => t.Id);

            Property(t => t.IsCompleted)
                .IsRequired();
        }
    }
}