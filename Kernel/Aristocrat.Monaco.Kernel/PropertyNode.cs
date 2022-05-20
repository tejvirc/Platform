namespace Aristocrat.Monaco.Kernel
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the PropertyNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("PropertySetting")]
    public class PropertyNode : ExtensionNode
    {
        /// <summary>Gets or sets the property name</summary>
        [NodeAttribute]
        public string PropertyName { get; set; }

        /// <summary>Gets or sets the property value</summary>
        [NodeAttribute]
        public string PropertyValue { get; set; }
    }
}