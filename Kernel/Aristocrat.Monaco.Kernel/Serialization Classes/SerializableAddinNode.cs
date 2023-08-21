namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the SerializableAddinNode class.
    /// </summary>
    [XmlRoot("Addin")]
    public class SerializableAddinNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinNode" /> class.
        /// </summary>
        public SerializableAddinNode()
            : this(null, null, null, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinNode" /> class.
        /// </summary>
        /// <param name="id">The addin id</param>
        /// <param name="addinNamespace">The addin namespace</param>
        /// <param name="version">The addin version</param>
        /// <param name="isRoot">True if the addin has no dependencies</param>
        public SerializableAddinNode(string id, string addinNamespace, string version, bool isRoot)
            : this(
                id,
                addinNamespace,
                version,
                isRoot,
                new SerializableRuntimeNode(),
                new SerializableDependenciesNode(),
                new LinkedList<SerializableBaseExtensionNode>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializableAddinNode" /> class.
        /// </summary>
        /// <param name="id">The addin id</param>
        /// <param name="addinNamespace">The addin namespace</param>
        /// <param name="version">The addin version</param>
        /// <param name="isRoot">True if the addin has no dependencies</param>
        /// <param name="runtime">The addin runtime imports</param>
        /// <param name="dependencies">The addin dependencies</param>
        /// <param name="extensions">The addin extensions</param>
        public SerializableAddinNode(
            string id,
            string addinNamespace,
            string version,
            bool isRoot,
            SerializableRuntimeNode runtime,
            SerializableDependenciesNode dependencies,
            ICollection<SerializableBaseExtensionNode> extensions)
        {
            Id = id;
            Namespace = addinNamespace;
            Version = version;
            IsRoot = isRoot;
            Runtime = runtime;
            Dependencies = dependencies;
            Extensions = extensions;
        }

        /// <summary>
        ///     Gets or sets the addin id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the addin namespace
        /// </summary>
        [XmlAttribute("namespace")]
        public string Namespace { get; set; }

        /// <summary>
        ///     Gets or sets the addin version
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the addin has dependencies or not
        /// </summary>
        [XmlAttribute("isroot")]
        public bool IsRoot { get; set; }

        /// <summary>
        ///     Gets or sets the addin runtime imports
        /// </summary>
        [XmlElement("Runtime")]
        public SerializableRuntimeNode Runtime { get; set; }

        /// <summary>
        ///     Gets or sets the addin dependencies
        /// </summary>
        [XmlElement("Dependencies")]
        public SerializableDependenciesNode Dependencies { get; set; }

        /// <summary>
        ///     Gets or sets the addin extensions
        /// </summary>
        [XmlIgnore]
        public ICollection<SerializableBaseExtensionNode> Extensions { get; set; }

        /// <summary>
        ///     Gets or sets the addin extensions
        ///     <para>This property should only be used by XmlSerializer</para>
        /// </summary>
        [XmlElement("Extension")]
        public SerializableBaseExtensionNode[] SerializableExtensions
        {
            get => Extensions.ToArray();

            set
            {
                if (value != null)
                {
                    foreach (var node in value)
                    {
                        Extensions.Add(node);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the overrides to ignore ExtensionNode properties
        /// </summary>
        [XmlIgnore]
        private static XmlAttributeOverrides XmlOverrides
        {
            get
            {
                var ignore = new XmlAttributes { XmlIgnore = true };
                var result = new XmlAttributeOverrides();
                result.Add(typeof(ExtensionNode), "Addin", ignore);
                result.Add(typeof(ExtensionNode), "ChildNodes", ignore);
                result.Add(typeof(ExtensionNode), "ExtensionContext", ignore);
                result.Add(typeof(ExtensionNode), "HasId", ignore);
                result.Add(typeof(ExtensionNode), "Id", ignore);
                result.Add(typeof(ExtensionNode), "Parent", ignore);
                result.Add(typeof(ExtensionNode), "Path", ignore);

                return result;
            }
        }

        /// <summary>
        ///     Gets an XmlSerializer capable of writing this to xml
        /// </summary>
        private XmlSerializer Serializer
        {
            get
            {
                ICollection<Type> types = new LinkedList<Type>();
                foreach (var node in Extensions)
                {
                    types.Add(node.GetType());
                }

                return new XmlSerializer(
                    typeof(SerializableAddinNode),
                    XmlOverrides,
                    types.ToArray(),
                    new XmlRootAttribute("Addin"),
                    string.Empty);
            }
        }

        /// <summary>
        ///     Returns a SerializableAddinNode deserialized from an xml file
        ///     <para>In most cases, the deserialization capabilities of Mono Addins should be used instead of this method</para>
        /// </summary>
        /// <param name="path">the file path to SerializableAddinNode xml</param>
        /// <param name="extensionNodeTypes">
        ///     Extensions of SerializableBaseExtensionNode needed to deserialize extensions of the
        ///     addin
        /// </param>
        /// <returns>A SerializableAddinNode deserialized from an xml file</returns>
        public static SerializableAddinNode FromXmlFile(string path, IEnumerable<Type> extensionNodeTypes)
        {
            var serializer = new XmlSerializer(
                typeof(SerializableAddinNode),
                XmlOverrides,
                extensionNodeTypes.ToArray(),
                new XmlRootAttribute("Addin"),
                string.Empty);

            SerializableAddinNode result;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                result = (SerializableAddinNode)serializer.Deserialize(stream);
            }

            return result;
        }

        /// <summary>
        ///     Returns SerializableAddinNode Xml
        /// </summary>
        /// <returns>SerializableAddinNode Xml</returns>
        public XmlDocument ToXmlDocument()
        {
            var stringWriter = new StringWriter(CultureInfo.CurrentCulture);
            var writerSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "    ",
                OmitXmlDeclaration = true
            };

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            Serializer.Serialize(XmlWriter.Create(stringWriter, writerSettings), this, namespaces);

            var result = new XmlDocument();
            result.LoadXml(stringWriter.ToString());

            return result;
        }
    }
}
