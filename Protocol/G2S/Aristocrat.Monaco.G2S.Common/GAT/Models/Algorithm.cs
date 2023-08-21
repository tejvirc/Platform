namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using Application.Contracts.Authentication;

    /// <summary>
    ///     Algorithm
    /// </summary>
    /// <seealso cref="IAlgorithm" />
    public class Algorithm : IAlgorithm
    {
        /// <summary>
        ///     Gets or sets algorithm type
        /// </summary>
        public AlgorithmType Type { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports offsetting
        /// </summary>
        public bool SupportsOffsets { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports a seed
        /// </summary>
        public bool SupportsSeed { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports salt
        /// </summary>
        public bool SupportsSalt { get; set; }
    }
}
