namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for the <see cref="ProgressiveUpdateEntity" /> entity
    /// </summary>
    public class ProgressiveUpdateEntityConfiguration : EntityTypeConfiguration<ProgressiveUpdateEntity>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveUpdateEntityConfiguration"/> class.
        /// </summary>
        public ProgressiveUpdateEntityConfiguration()
        {
            ToTable(nameof(ProgressiveUpdateEntity));

            HasKey(t => t.Id);
        }
    }
}
