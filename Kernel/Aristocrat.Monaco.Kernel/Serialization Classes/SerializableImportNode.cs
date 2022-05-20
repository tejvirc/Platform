namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableImportNode class.
    /// </summary>
    public class SerializableImportNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableImportNode" /> class.
        /// </summary>
        public SerializableImportNode()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableImportNode" /> class.
        /// </summary>
        /// <param name="assembly">The import assembly</param>
        public SerializableImportNode(string assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        ///     Gets or sets the import assembly
        /// </summary>
        [XmlAttribute("assembly")]
        public string Assembly { get; set; }
    }
}
