namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Kernel;
    using Mono.Addins;
    using Protocol;

    /// <inheritdoc cref="IProtocolCapabilityAttributeProvider" />
    public class ProtocolCapabilityAttributeProvider : IProtocolCapabilityAttributeProvider, IService
    {
        private const string ProtocolExtensionPath = "/Protocol/Runnables";

        /// <inheritdoc />
        public ProtocolCapabilityAttribute GetAttribute(string protocolName)
        {
            var protocolNode = AddinManager.GetExtensionNodes<ProtocolTypeExtensionNode>(ProtocolExtensionPath).SingleOrDefault(x => x.ProtocolId == protocolName);
            return protocolNode == null ? null : (ProtocolCapabilityAttribute)Attribute.GetCustomAttribute(protocolNode.Type, typeof(ProtocolCapabilityAttribute));
        }

        public string Name => nameof(ProtocolCapabilityAttributeProvider);

        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IProtocolCapabilityAttributeProvider) };

        public void Initialize(){}
    }
}