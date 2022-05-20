namespace Aristocrat.Monaco.Application.Contracts.Protocol
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to save and retrieve the configured multi protocol settings.
    /// </summary>
    public interface IMultiProtocolConfigurationProvider
    {
        /// <summary>
        ///  Property for the multi protocol configuration
        /// </summary>
        /// <returns></returns>
        IEnumerable<ProtocolConfiguration> MultiProtocolConfiguration { get; set; }

        /// <summary>
        ///  Property to keep track of whether a validation protocol selection is required or not
        /// </summary>
        bool IsValidationRequired { get; set; }

        /// <summary>
        ///  Property to keep track of whether a funds transfer protocol selection is required or not
        /// </summary>
        bool IsFundsTransferRequired { get; set; }

        /// <summary>
        ///  Property to keep track of whether a progressive protocol selection is required or not
        /// </summary>
        bool IsProgressiveRequired { get; set; }

        /// <summary>
        ///  Property to keep track of whether a central determination system protocol selection is required or not
        /// </summary>
        bool IsCentralDeterminationSystemRequired { get; set; }
    }
}