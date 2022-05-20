namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatSpecialFunctionParameter" /> entity
    /// </summary>
    public class GatSpecialFunctionParameterMap : EntityTypeConfiguration<GatSpecialFunctionParameter>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionParameterMap" /> class.
        /// </summary>
        public GatSpecialFunctionParameterMap()
        {
            ToTable("GatSpecialFunctionParameter");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.Parameter)
                .IsRequired();
        }
    }
}