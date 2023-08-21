namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using Application.Contracts.Authentication;

    /// <summary>
    ///     Algorithm interface
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        ///     Gets or sets algorithm type
        /// </summary>
        AlgorithmType Type { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports offsetting
        /// </summary>
        bool SupportsOffsets { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports a seed
        /// </summary>
        bool SupportsSeed { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the verification algorithm supports salt
        /// </summary>
        bool SupportsSalt { get; set; }
    }
}
