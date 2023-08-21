namespace Aristocrat.Monaco.Hardware.Contracts.Discovery
{
    using System;
    using Kernel;
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components can
    ///     specify the protocol they use for IO configuration discovery.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("IOConfiguration")]
    [ExtensionNodeChild(typeof(FilePathExtensionNode))]
    public class IOConfigurationExtensionNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the protocol of the IO configuration
        /// </summary>
        [NodeAttribute]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global This is used by mono addins
        public string Protocol { get; set; }

        /// <summary>
        ///     Gets or sets the Cabinet of the IO configuration
        /// </summary>
        [NodeAttribute]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global This is used by mono addins
        public string Cabinet { get; set; }
    }
}