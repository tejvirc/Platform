namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Linq;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     A set of utilities that can be used to read and parse configuration files
    /// </summary>
    public static class ConfigurationUtilities
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Provides safe deserialization of the specified file into the specified type
        /// </summary>
        /// <typeparam name="T">The serialized type</typeparam>
        /// <param name="file">The file to read</param>
        /// <returns>The type specified</returns>
        public static T Deserialize<T>(string file)
            where T : class
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(T))
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var serializer = new XmlSerializer(typeof(T), theXmlRootAttribute ?? new XmlRootAttribute(nameof(T)));
            using (var reader = new StreamReader(file))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        ///     Provides safe deserialization of the specified file into the specified type
        /// </summary>
        /// <typeparam name="T">The serialized type</typeparam>
        /// <param name="file">The file to read</param>
        /// <returns>The type specified</returns>
        public static T SafeDeserialize<T>(string file)
            where T : class
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            try
            {
                return Deserialize<T>(file);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return default(T);
            }
        }

        /// <summary>
        ///     Reads the configuration for the specified extension path
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="extensionPath">The extension path</param>
        /// <param name="defaultOnError">The default value to use in the event of an error</param>
        /// <returns>The parsed configuration or the default value in the event of an error</returns>
        public static T GetConfiguration<T>(string extensionPath, Func<T> defaultOnError)
            where T : class
        {
            var config = GetConfiguration<T>(extensionPath);

            return config ?? defaultOnError();
        }

        private static T GetConfiguration<T>(string extensionPath)
            where T : class
        {
            string path;

            try
            {
                if (!AddinManager.IsInitialized)
                {
                    return default(T);
                }

                var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<FilePathExtensionNode>(extensionPath);
                path = node.FilePath;
                Logger.Debug($"Found {extensionPath} node: {node.FilePath}");
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error($"Extension path {extensionPath} not found", ex);

                return default(T);
            }

            return SafeDeserialize<T>(path);
        }
    }
}