namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Linq;
    using Discovery;
    using Kernel;

    /// <summary>
    ///     <see cref="IIO" /> Extension Methods
    /// </summary>
    [CLSCompliant(false)]
    public static class IOExtensions
    {
        private const string IoConfigurationExtensionPath = "/IO/Configuration";

        /// <summary>
        ///     Gets the configuration information for the specified <see cref="IIO" /> implementation
        /// </summary>
        /// <param name="this">An <see cref="IIO" /> instance</param>
        /// <returns>The configuration</returns>
        public static IOConfigurations GetConfiguration(this IIO @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return ConfigurationUtilities.Deserialize<IOConfigurations>(
                GetPath(@this.DeviceConfiguration.Protocol, @this.DeviceConfiguration.VariantName));
        }

        private static string GetPath(string type, string cabinet)
        {
            var configurationNodes =
                MonoAddinsHelper.GetSelectedNodes<IOConfigurationExtensionNode>(IoConfigurationExtensionPath);
            var extensionNode =
                configurationNodes.Where(x => x.Cabinet == cabinet)
                    .Select(MonoAddinsHelper.GetChildNodes<FilePathExtensionNode>).FirstOrDefault() ??
                configurationNodes.Where(x => x.Protocol == type)
                    .Select(MonoAddinsHelper.GetChildNodes<FilePathExtensionNode>).First();

            return extensionNode.First().FilePath;
        }
    }
}