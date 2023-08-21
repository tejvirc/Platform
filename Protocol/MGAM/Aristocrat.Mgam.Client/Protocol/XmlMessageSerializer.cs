namespace Aristocrat.Mgam.Client.Protocol
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Implements the <see cref="IXmlMessageSerializer"/> interface
    /// </summary>
    public class XmlMessageSerializer : IXmlMessageSerializer
    {
        private const string DefaultNamespace = "";

        private readonly XmlSerializerNamespaces _namespaces = new XmlSerializerNamespaces();

        private readonly ConcurrentDictionary<Type, XmlSerializer> _serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="XmlMessageSerializer"/> class.
        /// </summary>
        public XmlMessageSerializer()
        {
            _namespaces.Add(string.Empty, string.Empty);
        }

        /// <inheritdoc />
        public bool TrySerialize(XmlMessage message, out string xml)
        {
            xml = null;

            var result = true;

            try
            {
                xml = Serialize(message);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <inheritdoc />
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2202:Do not dispose objects multiple times",
            Justification = "memory stream disposal is ok")]
        public string Serialize(XmlMessage message)
        {
            if (message == null)
            {
                return null;
            }

            var serializer = _serializers.GetOrAdd(message.GetType(), CreateSerializer);

            string xml;

            using (var memorySteam = new MemoryStream())
                using (var writer = new XmlTextWriter(memorySteam, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 2;

                    serializer.Serialize(writer, message, _namespaces);
                    memorySteam.Position = 0;
                    using (var reader = new StreamReader(memorySteam))
                    {
                        xml = reader.ReadToEnd();
                    }
                }

            return xml;
        }

        /// <inheritdoc />
        public bool TryDeserialize(string xml, out XmlMessage message)
        {
            message = default(XmlMessage);

            var result = true;

            try
            {
                message = Deserialize(xml);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <inheritdoc />
        public XmlMessage Deserialize(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentNullException(nameof(xml));
            }

            var messageType = GetMessageType(xml);

            var serializer = _serializers.GetOrAdd(messageType, CreateSerializer);

            object result;

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                result = serializer.Deserialize(ms);
            }

            return (XmlMessage)result;
        }

        private static XmlSerializer CreateSerializer(Type messageType)
        {
            return new XmlSerializer(messageType, GetOverrides(), new Type[] { }, null, DefaultNamespace);
        }

        private static XmlAttributeOverrides GetOverrides()
        {
            return new XmlAttributeOverrides();
        }

        private static Type GetMessageType(string xml)
        {
            var doc = new XmlDocument();

            doc.LoadXml(xml);
            Debug.Assert(doc.DocumentElement != null, "doc.DocumentElement != null");

            var name = doc.DocumentElement.Name;

            var messageTypes =
                (from type in typeof(XmlMessage).Assembly.GetTypes()
                    where typeof(XmlMessage).IsAssignableFrom(type)
                    select type)
                .ToDictionary(t => t.Name, t => t);

            if (!messageTypes.TryGetValue(name, out var messageType))
            {
                throw new InvalidOperationException($"Message type {name} not found");
            }

            return messageType;
        }
    }
}
