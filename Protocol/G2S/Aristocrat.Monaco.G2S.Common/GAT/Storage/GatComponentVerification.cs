namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using Application.Contracts.Authentication;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    ///     GAT component verification entity
    /// </summary>
    /// <seealso cref="ComponentVerification" />
    public class GatComponentVerification : ComponentVerification
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatComponentVerification" /> class.
        /// </summary>
        public GatComponentVerification()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatComponentVerification" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GatComponentVerification(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets component verification state
        /// </summary>
        public ComponentVerificationState State { get; set; }

        /// <summary>
        ///     Gets or sets designates the procedure that should be used to process the results
        /// </summary>
        public string GatExec { get; set; }

        /// <summary>
        ///     Gets or sets the verification request entity.
        /// </summary>
        /// <value>
        ///     The verification request entity.
        /// </value>
        public virtual GatVerificationRequest VerificationEntity { get; set; }
    }
}
