namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Monaco.Common.Storage;

    /// <summary>
    ///     GAT special function parameter entity
    /// </summary>
    /// <seealso cref="Aristocrat.Monaco.Common.Storage.BaseEntity" />
    public class GatSpecialFunctionParameter : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionParameter" /> class.
        /// </summary>
        public GatSpecialFunctionParameter()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunctionParameter" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GatSpecialFunctionParameter(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the foreign key.Reference on SpecialFunction identifier
        /// </summary>
        public long GatSpecialFunctionId { get; set; }

        /// <summary>
        ///     Gets or sets the special function.
        /// </summary>
        /// <value>
        ///     The special function.
        /// </value>
        [ForeignKey("GatSpecialFunctionId")]
        public virtual GatSpecialFunction SpecialFunction { get; set; }

        /// <summary>
        ///     Gets or sets parameter for the function
        /// </summary>
        public string Parameter { get; set; }
    }
}