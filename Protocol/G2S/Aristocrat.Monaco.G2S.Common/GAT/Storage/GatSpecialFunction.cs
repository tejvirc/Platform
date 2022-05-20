namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Collections.Generic;
    using Monaco.Common.Storage;

    /// <summary>
    ///     GAT special function entity
    /// </summary>
    /// <seealso cref="Aristocrat.Monaco.Common.Storage.BaseEntity" />
    public class GatSpecialFunction : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunction" /> class.
        /// </summary>
        public GatSpecialFunction()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatSpecialFunction" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GatSpecialFunction(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets function name
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        ///     Gets or sets designates the procedure that should be used to process the results
        /// </summary>
        public string GatExec { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public virtual ICollection<GatSpecialFunctionParameter> Parameters { get; set; }
    }
}