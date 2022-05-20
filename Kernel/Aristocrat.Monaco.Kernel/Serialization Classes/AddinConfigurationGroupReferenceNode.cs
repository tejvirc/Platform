namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System;
    using System.Xml.Serialization;
    using Mono.Addins;

    #endregion

    /// <summary>
    ///     Definition of the AddinConfigurationGroupReferenceNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("AddinConfigurationGroupReference")]
    public class AddinConfigurationGroupReferenceNode : ExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddinConfigurationGroupReferenceNode" /> class.
        /// </summary>
        public AddinConfigurationGroupReferenceNode()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddinConfigurationGroupReferenceNode" /> class.
        /// </summary>
        /// <param name="name">The name of the AddinConfigurationGroupNode that this references</param>
        public AddinConfigurationGroupReferenceNode(string name)
            : this(name, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddinConfigurationGroupReferenceNode" /> class.
        /// </summary>
        /// <param name="name">The name of the AddinConfigurationGroupNode that this references</param>
        /// <param name="optional">A value indicating whether an exception should be thrown if the reference is not found</param>
        public AddinConfigurationGroupReferenceNode(string name, bool optional)
        {
            Name = name;
            Optional = optional;
        }

        /// <summary>
        ///     Gets or sets the name of the AddinConfigurationGroupNode that this references.
        /// </summary>
        [NodeAttribute("name")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether an exception should be thrown if the reference is not found.
        /// </summary>
        [NodeAttribute("optional")]
        [XmlAttribute("optional")]
        public bool Optional { get; set; }
    }
}
