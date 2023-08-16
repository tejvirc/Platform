namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using Common.Storage;
    using ProtoBuf;

    /// <summary>
    ///     ComponentVerification defines the state of a component's verification.
    /// </summary>
    [CLSCompliant(false)]
    [ProtoContract]
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
        [ProtoMember(1)]
        public long RequestId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the foreign key. Reference on Component identification
        /// </summary>
        [ProtoMember(2)]
        public string ComponentId { get; set; }

        /// <summary>
        ///     Gets or sets authentication algorithm to run on the component
        /// </summary>
        public AlgorithmType AlgorithmType { get; set; }

        /// <summary>
        ///     Gets or sets seed for the algorithm
        /// </summary>
        [ProtoMember(3)]
        public byte[] Seed { get; set; }

        /// <summary>
        ///     Gets or sets arbitrary bytes that are prefixed to the component’s byte buffer before the hash is generated
        /// </summary>
        [ProtoMember(4)]
        public byte[] Salt { get; set; }

        /// <summary>
        ///     Gets or sets starting offset for the verification component
        /// </summary>
        [ProtoMember(5)]
        public long StartOffset { get; set; }

        /// <summary>
        ///     Gets or sets ending offset for the verification component
        /// </summary>
        [ProtoMember(6)]
        public long EndOffset { get; set; }

        /// <summary>
        ///     Gets or sets result of the verification request
        /// </summary>
        [ProtoMember(7)]
        public byte[] Result { get; set; }

        /// <summary>
        ///     Gets or sets the time of the result
        /// </summary>
        [ProtoMember(8)]
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
