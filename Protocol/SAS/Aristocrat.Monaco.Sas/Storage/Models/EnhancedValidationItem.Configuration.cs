namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="EnhancedValidationItem"/>
    /// </summary>
    public class EnhancedValidationItemConfiguration : IEntityTypeConfiguration<EnhancedValidationItem>
    {
        /// <summary>
        ///     Creates an instance of <see cref="EnhancedValidationItemConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<EnhancedValidationItem> builder)
        {
            builder.ToTable(nameof(EnhancedValidationItem));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EnhancedValidationDataLog);
        }
    }
}