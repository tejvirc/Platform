namespace Aristocrat.Monaco.Kernel.Contracts.Events
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components
    ///     can specify a particular startup event listener implementation.
    /// </summary>
    [ExtensionNode("StartupEventListenerImpl")]
    [CLSCompliant(false)]
    public class StartupEventListenerImplementationExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Gets or sets the protocol (SAS, G2S, etc) name.
        /// </summary>
        [NodeAttribute]
        public string ProtocolName { get; set; }
    }
}