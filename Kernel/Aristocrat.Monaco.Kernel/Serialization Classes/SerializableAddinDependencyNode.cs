namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableAddinDependencyNode class.
    /// </summary>
    public class SerializableAddinDependencyNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinDependencyNode" /> class.
        /// </summary>
        public SerializableAddinDependencyNode()
            : this(null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinDependencyNode" /> class.
        /// </summary>
        /// <param name="id">The addin's id</param>
        /// <param name="version">The addin's version</param>
        public SerializableAddinDependencyNode(string id, string version)
        {
            Id = id;
            Version = version;
        }

        /// <summary>
        ///     Gets or sets the addin's id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the addin's version
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }
    }
}
