namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableDependenciesNode class.
    /// </summary>
    public class SerializableDependenciesNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableDependenciesNode" /> class.
        /// </summary>
        public SerializableDependenciesNode()
            : this(new LinkedList<SerializableAddinDependencyNode>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableDependenciesNode" /> class.
        /// </summary>
        /// <param name="dependencies">An addin's dependencies</param>
        public SerializableDependenciesNode(ICollection<SerializableAddinDependencyNode> dependencies)
        {
            AddinDependencies = dependencies;
        }

        /// <summary>
        ///     Gets an addin's dependencies
        /// </summary>
        [XmlIgnore]
        public ICollection<SerializableAddinDependencyNode> AddinDependencies { get; }

        /// <summary>
        ///     Gets or sets an addin's dependencies
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("Addin")]
        public SerializableAddinDependencyNode[] SerializableAddinDependencies
        {
            get => AddinDependencies.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        AddinDependencies.Add(node);
                    }
                }
            }
        }
    }
}
