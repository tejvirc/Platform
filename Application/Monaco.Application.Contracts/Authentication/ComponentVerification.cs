namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     ComponentVerification defines the state of a component's verification.
    /// </summary>
    [CLSCompliant(false)]
    public class ComponentVerification : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentVerification" /> class.
        /// </summary>
        public ComponentVerification()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentVerification" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ComponentVerification(long id)
        {
            Id = id;
            ClearResults();
        }

        /// <summary>
        ///     Gets or sets verification request identifier (primary key)
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the foreign key. Reference on Component identification
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        ///     Gets or sets authentication algorithm to run on the component
        /// </summary>
        public AlgorithmType AlgorithmType { get; set; }

        /// <summary>
        ///     Gets or sets seed for the algorithm
        /// </summary>
        public byte[] Seed { get; set; }

        /// <summary>
        ///     Gets or sets arbitrary bytes that are prefixed to the component’s byte buffer before the hash is generated
        /// </summary>
        public byte[] Salt { get; set; }

        /// <summary>
        ///     Gets or sets starting offset for the verification component
        /// </summary>
        public long StartOffset { get; set; }

        /// <summary>
        ///     Gets or sets ending offset for the verification component
        /// </summary>
        public long EndOffset { get; set; }

        /// <summary>
        ///     Gets or sets result of the verification request
        /// </summary>
        public byte[] Result { get; set; }

        /// <summary>
        ///     Gets or sets the time of the result
        /// </summary>
        public DateTime ResultTime { get; set; }

        /// <summary>
        ///     Clear any validation results.
        /// </summary>
        public void ClearResults()
        {
            Result = null;
            ResultTime = DateTime.MinValue;
        }
    }
}
