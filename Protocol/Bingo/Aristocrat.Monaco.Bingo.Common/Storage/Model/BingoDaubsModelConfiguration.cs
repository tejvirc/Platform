namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The <see cref="BingoDaubsModel"/> configuration for persistence
    /// </summary>
    public class BingoDaubsModelConfiguration : IEntityTypeConfiguration<BingoDaubsModel>
    {
        /// <summary>
        ///     Creates an instance of <see cref="BingoDaubsModelConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<BingoDaubsModel> builder)
        {
            builder.ToTable(nameof(BingoDaubsModel));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CardIsDaubed).IsRequired();
        }
    }
}