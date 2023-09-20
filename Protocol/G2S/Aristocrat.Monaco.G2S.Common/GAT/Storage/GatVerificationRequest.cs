namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System;
    using System.Collections.Generic;
    using Monaco.Common.Storage;

    /// <summary>
    ///     GAT verification request entity
    /// </summary>
    /// <seealso cref="Aristocrat.Monaco.Common.Storage.BaseEntity" />
    public class GatVerificationRequest : BaseEntity, ILogSequence
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatVerificationRequest" /> class.
        /// </summary>
        public GatVerificationRequest()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatVerificationRequest" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GatVerificationRequest(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets Verification identifier (primary key)
        /// </summary>
        public long VerificationId { get; set; }

        /// <summary>
        ///     Gets or sets transaction identifier
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification request is completed
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        ///     Gets or sets device identifier of the device that generated the transaction
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets function type
        /// </summary>
        public FunctionType FunctionType { get; set; }

        /// <summary>
        ///     Gets or sets  identifier of the employee present at the EGM when the function was executed
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        ///     Gets or sets date and time that the GAT function was executed
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        ///     Gets or sets the component verifications.
        /// </summary>
        /// <value>
        ///     The component verifications.
        /// </value>
        public virtual ICollection<GatComponentVerification> ComponentVerifications { get; set; }
    }
}