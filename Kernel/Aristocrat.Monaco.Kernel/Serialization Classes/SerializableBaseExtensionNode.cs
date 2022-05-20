namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableBaseExtensionNode class.
    /// </summary>
    public abstract class SerializableBaseExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableBaseExtensionNode" /> class.
        /// </summary>
        /// <param name="path">The extension path being extended</param>
        protected SerializableBaseExtensionNode(string path)
        {
            Path = path;
        }

        /// <summary>
        ///     Gets or sets the path to the file's original location
        /// </summary>
        [XmlAttribute("path")]
        public string Path { get; set; }
    }
}
