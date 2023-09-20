namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;
    using Protocol.Common.Storage;

    /// <summary>
    ///     Configuration for the <see cref="ConfigChangeAuthorizeItem" /> entity
    /// </summary>
    public class ConfigChangeAuthorizeItemMap : IEntityTypeConfiguration<ConfigChangeAuthorizeItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigChangeAuthorizeItemMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<ConfigChangeAuthorizeItem> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable(nameof(ConfigChangeAuthorizeItem));

            // Primary Key
            builder.HasKey(t => t.Id);
            builder.Property(t => t.HostId)
                .IsRequired();
            builder.Property(t => t.TimeoutDate)
                .IsRequired(false);
            builder.Property(t => t.AuthorizeStatus)
                .IsRequired();
            builder.Property(t => t.TimeoutAction)
                .IsRequired();
        }
    }
}