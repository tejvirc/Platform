namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Application.Contracts.Authentication;

    /// <summary>
    ///     Verify component model
    /// </summary>
    public class VerifyComponent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VerifyComponent" /> class.
        /// </summary>
        /// <param name="componentId">Component identifier</param>
        /// <param name="algorithmType">hash type algorithm</param>
        /// <param name="seed">
        ///     The seed for the algorithm. Certain algorithms, such as checksum(CRC), require a seed to define the
        ///     starting value
        /// </param>
        /// <param name="salt">Arbitrary bytes that are prefixed to the component’s byte buffer before the hash is generated</param>
        /// <param name="startOffset">Starting offset for the verification component</param>
        /// <param name="endOffset">Ending offset for the verification component</param>
        public VerifyComponent(
            string componentId,
            AlgorithmType algorithmType,
            string seed,
            string salt,
            long startOffset,
            long endOffset)
        {
            if (string.IsNullOrEmpty(componentId))
            {
                throw new ArgumentNullException(nameof(componentId));
            }

            ComponentId = componentId;
            AlgorithmType = algorithmType;
            Seed = seed;
            Salt = salt;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        /// <summary>
        ///     Gets component identifier
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        ///     Gets hash algorithm type
        /// </summary>
        public AlgorithmType AlgorithmType { get; }

        /// <summary>
        ///     Gets seed for the algorithm
        /// </summary>
        public string Seed { get; }

        /// <summary>
        ///     Gets arbitrary bytes that are prefixed to the component’s byte buffer before the hash is generated
        /// </summary>
        public string Salt { get; }

        /// <summary>
        ///     Gets starting offset for the verification component
        /// </summary>
        public long StartOffset { get; }

        /// <summary>
        ///     Gets ending offset for the verification component
        /// </summary>
        public long EndOffset { get; }
    }
}
