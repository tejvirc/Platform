namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    /// Provides access to ProtocolCapabilityAttribute in an injectable, unit-testable form
    /// </summary>
    public interface IProtocolCapabilityAttributeProvider
    {
        /// <summary>
        /// Returns ProtocolCapabilityAttribute for the specified protocol host
        /// </summary>
        /// <param name="protocolName">The name of the protocol whose host has the attribute we want</param>
        /// <returns>ProtocolCapabilityAttribute for the specified protocol</returns>
        ProtocolCapabilityAttribute GetAttribute(string protocolName);
    }
}