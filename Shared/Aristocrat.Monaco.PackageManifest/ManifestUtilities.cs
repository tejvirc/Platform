namespace Aristocrat.Monaco.PackageManifest
{
    using System;
    using System.Linq;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    ///     Basic manifest utilities
    /// </summary>
    public static class ManifestUtilities
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        ///     Parses the provided file
        /// </summary>
        /// <param name="file">The file to parse</param>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <returns>An instance of T</returns>
        /// <exception cref="ArgumentNullException">Thrown if file is null or empty</exception>
        public static T Parse<T>(string file)
            where T : class
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            var settings = new XmlReaderSettings();

            using (var reader = XmlReader.Create(file, settings))
            {
                var serializer = Serializers.GetOrAdd(typeof(T), t =>
                {
                    var theXmlRootAttribute = Attribute.GetCustomAttributes(t.GetType())
                        .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                    var serializer = new XmlSerializer(t.GetType(), theXmlRootAttribute ?? new XmlRootAttribute(t.GetType().Name));
                    return serializer;
                });

                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        ///     Parses the provided file
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <returns>An instance of T</returns>
        /// <exception cref="ArgumentNullException">Thrown if file is null or empty</exception>
        public static T Parse<T>(Stream stream)
            where T : class
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var serializer = Serializers.GetOrAdd(typeof(T), t =>
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(t.GetType())
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(t.GetType(), theXmlRootAttribute ?? new XmlRootAttribute(t.GetType().Name));
                return serializer;
            });

            return (T)serializer.Deserialize(stream);
        }
    }
}