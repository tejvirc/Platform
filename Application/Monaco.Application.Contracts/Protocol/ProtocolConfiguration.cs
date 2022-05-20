namespace Aristocrat.Monaco.Application.Contracts.Protocol
{
    /// <summary>
    /// Data class for the Protocol Configuration settings
    /// </summary>
    public class ProtocolConfiguration
    {
        /// <summary>
        ///     Constructor for ProtocolConfiguration
        /// </summary>
        public ProtocolConfiguration(CommsProtocol protocol, bool isValidationHandled = false,
            bool isFundTransferHandled = false, bool isProgressiveHandled = false,
            bool isCentralDeterminationHandled = false)
        {
            Protocol = protocol;
            IsValidationHandled = isValidationHandled;
            IsFundTransferHandled = isFundTransferHandled;
            IsProgressiveHandled = isProgressiveHandled;
            IsCentralDeterminationHandled = isCentralDeterminationHandled;
        }

        /// <summary>
        ///     Gets or sets the protocol id of the configuration
        /// </summary>
        public CommsProtocol Protocol { get; }

        /// <summary>
        ///     Gets or sets whether this protocol handles voucher validation or not
        /// </summary>
        public bool IsValidationHandled { get; set; }

        /// <summary>
        ///     Gets or sets whether this protocol handles fund transfer or not
        /// </summary>
        public bool IsFundTransferHandled { get; set; }

        /// <summary>
        ///     Gets or sets whether this protocol handles progressives or not
        /// </summary>
        public bool IsProgressiveHandled { get; set; }

        /// <summary>
        ///     Gets or sets whether this protocol handles Central Determination System(CDS)
        /// </summary>
        public bool IsCentralDeterminationHandled { get; set; }
    }
}