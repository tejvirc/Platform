namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableAddinConfigurationGroupNode class.
    /// </summary>
    [CLSCompliant(false)]
    public class SerializableAddinConfigurationGroupNode : SerializableBaseExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinConfigurationGroupNode" /> class.
        /// </summary>
        public SerializableAddinConfigurationGroupNode()
            : this(
                MonoAddinsHelper.AddinConfigurationGroupExtensionPoint,
                new LinkedList<AddinConfigurationGroupNode>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinConfigurationGroupNode" /> class.
        /// </summary>
        /// <param name="path">The extension path being extended</param>
        public SerializableAddinConfigurationGroupNode(string path)
            : this(path, new LinkedList<AddinConfigurationGroupNode>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinConfigurationGroupNode" /> class.
        /// </summary>
        /// <param name="path">The extension path being extended</param>
        /// <param name="addinConfigurationGroupNodes">The extension's AddinConfigurationGroupNodes</param>
        public SerializableAddinConfigurationGroupNode(
            string path,
            ICollection<AddinConfigurationGroupNode> addinConfigurationGroupNodes)
            : base(path)
        {
            AddinConfigurationGroupNodes = addinConfigurationGroupNodes;
        }

        /// <summary>
        ///     Gets the extension's AddinConfigurationGroupNodes
        /// </summary>
        [XmlIgnore]
        public ICollection<AddinConfigurationGroupNode> AddinConfigurationGroupNodes { get; }

        /// <summary>
        ///     Gets or sets the extension's AddinConfigurationGroupNodes
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("AddinConfigurationGroup")]
        public AddinConfigurationGroupNode[] SerializableAddinConfigurationGroupNodes
        {
            get => AddinConfigurationGroupNodes.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        AddinConfigurationGroupNodes.Add(node);
                    }
                }
            }
        }
    }
}
