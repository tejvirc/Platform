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
    ///     Definition of the AddinConfigurationGroupNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNodeChild(typeof(AddinConfigurationGroupReferenceNode))]
    [ExtensionNodeChild(typeof(ExtensionPointConfigurationNode))]
    public class AddinConfigurationGroupNode : FilterableExtensionNode
    {
        /// <summary>
        ///     configures extension points
        /// </summary>
        [XmlIgnore] private ICollection<ExtensionPointConfigurationNode> _extensionPointConfigurations;

        /// <summary>
        ///     references to other addin configuration groups
        /// </summary>
        [XmlIgnore] private ICollection<AddinConfigurationGroupReferenceNode> _groupReferences;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddinConfigurationGroupNode" /> class.
        /// </summary>
        public AddinConfigurationGroupNode()
            : this(null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddinConfigurationGroupNode" /> class.
        /// </summary>
        /// <param name="name">The name of this group, which may be referenced by AddinConfigurationGroupReferencesNode</param>
        /// <param name="description">A description of what this configuration provides</param>
        public AddinConfigurationGroupNode(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        ///     Gets or sets a description of what this configuration provides.
        /// </summary>
        [NodeAttribute("description")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Gets the references to other addin configuration groups
        /// </summary>
        [XmlIgnore]
        public ICollection<AddinConfigurationGroupReferenceNode> GroupReferences
        {
            get
            {
                if (_groupReferences == null)
                {
                    _groupReferences = MonoAddinsHelper.GetChildNodes<AddinConfigurationGroupReferenceNode>(this);
                }

                return _groupReferences;
            }
        }

        /// <summary>
        ///     Gets the configurations of extension points
        /// </summary>
        [XmlIgnore]
        public ICollection<ExtensionPointConfigurationNode> ExtensionPointConfigurations
        {
            get
            {
                if (_extensionPointConfigurations == null)
                {
                    _extensionPointConfigurations =
                        MonoAddinsHelper.GetChildNodes<ExtensionPointConfigurationNode>(this);
                }

                return _extensionPointConfigurations;
            }
        }

        /// <summary>
        ///     Gets or sets the name of this group, which may be referenced by AddinConfigurationGroupReferencesNode
        /// </summary>
        [NodeAttribute("name")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the configurations of extension points
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("ExtensionPointConfiguration")]
        public ExtensionPointConfigurationNode[] SerializableExtensionPointConfigurations
        {
            get => ExtensionPointConfigurations.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        ExtensionPointConfigurations.Add(node);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the references to other addin configuration groups
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("AddinConfigurationGroupReference")]
        public AddinConfigurationGroupReferenceNode[] SerializableGroupReferences
        {
            get => GroupReferences.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        GroupReferences.Add(node);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the addin configuration group defined in addin.xml with the specified name
        /// </summary>
        /// <param name="name">The name of the addin configuration group to return</param>
        /// <returns>The addin configuration group defined in addin.xml with the specified name</returns>
        public static AddinConfigurationGroupNode Get(string name)
        {
            AddinConfigurationGroupNode result = null;

            foreach (AddinConfigurationGroupNode node in AddinManager.GetExtensionNodes(
                MonoAddinsHelper.AddinConfigurationGroupExtensionPoint))
            {
                if (node.Name.Equals(name))
                {
                    result = node;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        ///     <para>Returns an extension point configuration at an extension path,</para>
        ///     <para>or null if no configuration exists</para>
        /// </summary>
        /// <param name="extensionPath">the extension path to find a configuration for</param>
        /// <returns>an extension point configuration at an extension path</returns>
        public ExtensionPointConfigurationNode GetExtensionPointConfiguration(string extensionPath)
        {
            ExtensionPointConfigurationNode result = null;

            foreach (var node in ExtensionPointConfigurations)
            {
                if (node.ExtensionPath.Equals(extensionPath))
                {
                    result = node;
                }
            }

            return result;
        }
    }
}
