namespace Aristocrat.Monaco.Application.Protocol
{
    using System;
    using Mono.Addins;

#pragma warning disable 0649

    /// <summary>
    ///     Definition of the ProtocolTypeExtensionNode class.
    /// </summary>
    [CLSCompliant(false)]
    public class ProtocolTypeExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Specifies the unique identifier for this protocol.
        /// </summary>
        [NodeAttribute("protocolId", typeof(string), true)] private string _protocolId;

        /// <summary>
        ///     Specifies the optional server provided unique identifier for this protocol.
        /// </summary>
        [NodeAttribute("serverProtocolId", typeof(int), false)] private readonly int _serverProtocolId = -1;

        /// <summary>
        ///     Gets the storage protocol identifier used for PersistenStorage.
        /// </summary>
        public string ProtocolId => _protocolId;

        /// <summary>
        ///     Gets the protocol identifier provided by the VGT server.
        /// </summary>
        public int ServerProtocolId => _serverProtocolId;
    }

#pragma warning restore 0649
}