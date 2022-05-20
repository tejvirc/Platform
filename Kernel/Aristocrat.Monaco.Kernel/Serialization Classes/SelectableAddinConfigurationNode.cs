namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System;
    using System.Xml.Serialization;
    using Mono.Addins;

    #endregion

    /// <summary>
    ///     Definition of the SelectableAddinConfigurationNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("SelectableAddinConfigurationNode")]
    [ExtensionNodeChild(typeof(AddinConfigurationGroupReferenceNode))]
    public class SelectableAddinConfigurationNode : ExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectableAddinConfigurationNode" /> class.
        /// </summary>
        public SelectableAddinConfigurationNode()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectableAddinConfigurationNode" /> class.
        /// </summary>
        /// <param name="name">The name of the addin selection</param>
        public SelectableAddinConfigurationNode(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets or sets the name of the selection.
        /// </summary>
        [NodeAttribute("name")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the order in which the selection should be processed.
        /// </summary>
        [NodeAttribute("order")]
        [XmlAttribute("order")]
        public int Order { get; set; }
    }
}