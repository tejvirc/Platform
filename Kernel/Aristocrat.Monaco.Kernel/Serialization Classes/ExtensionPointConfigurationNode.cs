namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Mono.Addins;

    #endregion

    /// <summary>
    ///     Definition of the ExtensionPointConfigurationNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("ExtensionPointConfiguration")]
    [ExtensionNodeChild(typeof(NodeSpecificationNode))]
    public class ExtensionPointConfigurationNode : ExtensionNode
    {
        /// <summary>
        ///     The addins to be loaded at this extension point
        /// </summary>
        [XmlIgnore] private ICollection<NodeSpecificationNode> _extensionNodeSpecifications;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionPointConfigurationNode" /> class.
        /// </summary>
        public ExtensionPointConfigurationNode()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionPointConfigurationNode" /> class.
        /// </summary>
        /// <param name="extensionPath">The extension path of nested AddinSpecificationNodes</param>
        public ExtensionPointConfigurationNode(string extensionPath)
        {
            ExtensionPath = extensionPath;
        }

        /// <summary>
        ///     Gets or sets the extension path of nested AddinSpecificationNodes
        /// </summary>
        [NodeAttribute("extensionPath")]
        [XmlAttribute("extensionPath")]
        public string ExtensionPath { get; set; }

        /// <summary>
        ///     Gets the addins to be loaded at this extension point
        /// </summary>
        [XmlIgnore]
        public ICollection<NodeSpecificationNode> ExtensionNodeSpecifications
        {
            get
            {
                if (_extensionNodeSpecifications == null)
                {
                    _extensionNodeSpecifications = MonoAddinsHelper.GetChildNodes<NodeSpecificationNode>(this);
                }

                return _extensionNodeSpecifications;
            }
        }

        /// <summary>
        ///     Gets or sets the addins to be loaded at this extension point
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("NodeSpecification")]
        public NodeSpecificationNode[] SerializableExtensionNodeSpecifications
        {
            get => ExtensionNodeSpecifications.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        ExtensionNodeSpecifications.Add(node);
                    }
                }
            }
        }
    }
}
