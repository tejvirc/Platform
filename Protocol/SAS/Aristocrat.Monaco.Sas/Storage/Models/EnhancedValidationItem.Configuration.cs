namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="EnhancedValidationItem"/>
    /// </summary>
    public class EnhancedValidationItemConfiguration : EntityTypeConfiguration<EnhancedValidationItem>
    {
        /// <summary>
        ///     Creates an instance of <see cref="EnhancedValidationItemConfiguration"/>
        /// </summary>
        public EnhancedValidationItemConfiguration()
        {
            ToTable(nameof(EnhancedValidationItem));
            HasKey(x => x.Id);
            Property(x => x.EnhancedValidationDataLog);
        }
    }
}