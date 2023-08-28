namespace Aristocrat.Monaco.PackageManifest
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Security;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    ///     Basic manifest utilities
    /// </summary>
    public static class ManifestUtilities
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers = new ();

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
                var serializer = Serializers.GetOrAdd(typeof(T), t => new XmlSerializer(t));
                serializer.UnknownNode += new XmlNodeEventHandler(UnknownXmlNodeHandler);

                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        ///     Handler of unknown XML node
        /// </summary>
        private static void UnknownXmlNodeHandler(object sender, XmlNodeEventArgs e) => throw new XmlSyntaxException();

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

            var serializer = Serializers.GetOrAdd(typeof(T), t => new XmlSerializer(t));

            return (T)serializer.Deserialize(stream);
        }
    }
}