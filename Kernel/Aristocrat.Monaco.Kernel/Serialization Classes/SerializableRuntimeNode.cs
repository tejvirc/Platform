namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    #endregion

    /// <summary>
    ///     Definition of the SerializableRuntimeNode class.
    /// </summary>
    public class SerializableRuntimeNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableRuntimeNode" /> class.
        /// </summary>
        public SerializableRuntimeNode()
            : this(new LinkedList<SerializableImportNode>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableRuntimeNode" /> class.
        /// </summary>
        /// <param name="imports">An addin's imports</param>
        public SerializableRuntimeNode(ICollection<SerializableImportNode> imports)
        {
            Imports = imports;
        }

        /// <summary>
        ///     Gets an addin's dependencies
        /// </summary>
        [XmlIgnore]
        public ICollection<SerializableImportNode> Imports { get; }

        /// <summary>
        ///     Gets or sets an addin's imports
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("Import")]
        public SerializableImportNode[] SerializableDependencies
        {
            get => Imports.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        Imports.Add(node);
                    }
                }
            }
        }
    }
}
