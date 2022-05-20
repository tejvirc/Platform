namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An attribute that indicates the functions a protocol is capable of handling.
    /// </summary>
    public class ProtocolCapabilityAttribute : Attribute
    {
        /// <summary>
        /// Name of the protocol
        /// </summary>
        public CommsProtocol Protocol { get; }

        /// <summary>
        /// Specifies if the protocol supports validation.
        /// </summary>
        public bool IsValidationSupported{ get; }

        /// <summary>
        /// Specifies if the protocol supports fund transfer.
        /// </summary>
        public bool IsFundTransferSupported {  get;  }

        /// <summary>
        /// Specifies if the protocol supports progressives.
        /// </summary>
        public bool IsProgressivesSupported { get; }

        /// <summary>
        /// Specifies if the protocol supports Central Determination System(CDS).
        /// </summary>
        public bool IsCentralDeterminationSystemSupported { get; }

        /// <summary>
        /// Provides list of Functions this protocol provides
        /// </summary>
        public List<Functionality> ProvidedFunctions { get; } = new();

        /// <summary>
        /// <param name="protocol"></param>
        /// <param name="isValidationSupported"></param>
        /// <param name="isFundTransferSupported"></param>
        /// <param name="isProgressivesSupported"></param>
        /// <param name="isCentralDeterminationSystemSupported"></param>
        /// </summary>
        public ProtocolCapabilityAttribute(
            CommsProtocol protocol,
            bool isValidationSupported,
            bool isFundTransferSupported,
            bool isProgressivesSupported,
            bool isCentralDeterminationSystemSupported)
        {
            Protocol = protocol;
            IsValidationSupported = isValidationSupported;
            IsFundTransferSupported = isFundTransferSupported;
            IsProgressivesSupported = isProgressivesSupported;
            IsCentralDeterminationSystemSupported = isCentralDeterminationSystemSupported;

            if (isValidationSupported) ProvidedFunctions.Add(Functionality.Validation);
            if (isFundTransferSupported) ProvidedFunctions.Add(Functionality.FundsTransfer);
            if (isProgressivesSupported) ProvidedFunctions.Add(Functionality.Progressive);
            if (isCentralDeterminationSystemSupported) ProvidedFunctions.Add(Functionality.CentralDeterminationSystem);
        }
    }
}