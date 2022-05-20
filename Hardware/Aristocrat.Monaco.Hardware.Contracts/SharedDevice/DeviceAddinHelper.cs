namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using Kernel;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     This class provides non-static access to AddinManager static methods
    ///     to support unit testing.
    /// </summary>
    public class DeviceAddinHelper : IAddinFactory
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Object used to synchronize access to methods in this class.
        /// </summary>
        private static readonly object SyncLock = new object();

        /// <inheritdoc/>
        public T CreateAddin<T>(string extensionPath, string protocolName)
        {
            return (T)GetDeviceImplementationObject(extensionPath, protocolName);
        }

        /// <summary>
        ///     Finds first extension node at the given extension node expecting extensions of type
        ///     DeviceImplementationExtensionNode and matching the specified protocol name.
        ///     Creates and returns an object associated with the extension type.
        /// </summary>
        /// <param name="extensionPath">The path to look for the extension nodes of type DeviceImplementationExtensionNode.</param>
        /// <param name="protocolName">The name of the protocol used to select the correct extension.</param>
        /// <returns>The first extension object or null if no extension was found.</returns>
        public virtual object GetDeviceImplementationObject(string extensionPath, string protocolName)
        {
            lock (SyncLock)
            {
                Logger.Debug($"Searching \"{extensionPath}\" for an extension with protocol name \"{protocolName}\"...");

                var nodes = AddinManager.GetExtensionNodes<DeviceImplementationExtensionNode>(extensionPath);

                Logger.Debug($"Found {nodes.Count} extensions at \"{extensionPath}\"");

                foreach (var node in nodes)
                {
                    if (node.ProtocolName.Contains(protocolName) || node.ProtocolName == "Any")
                    {
                        Logger.Debug($"Found match, creating instance of type {node.Type}");

                        return node.CreateInstance();
                    }
                }

                return null;
            }
        }

        /// <inheritdoc/>
        public bool DoesAddinExist(string extensionPath, string protocolName)
        {
            return DoesDeviceImplementationExist(extensionPath, protocolName);
        }

        /// <summary>
        ///     Finds first extension node at the given extension node expecting extensions of type
        ///     DeviceImplementationExtensionNode and matching the specified protocol name.
        ///     Returns true if such an extension exists.
        /// </summary>
        /// <param name="extensionPath">The path to look for the extension nodes of type DeviceImplementationExtensionNode.</param>
        /// <param name="protocolName">The name of the protocol used to select the correct extension.</param>
        /// <returns>True if the implementation at the extension exists.</returns>
        public virtual bool DoesDeviceImplementationExist(string extensionPath, string protocolName)
        {
            lock (SyncLock)
            {
                Logger.Debug($"Searching \"{extensionPath}\" for an extension with protocol name \"{protocolName}\"...");

                var nodes = AddinManager.GetExtensionNodes<DeviceImplementationExtensionNode>(extensionPath);

                Logger.Debug($"Found {nodes.Count} extensions at \"{extensionPath}\"");

                if (nodes.Any(e => e.ProtocolName.Contains(protocolName) || e.ProtocolName == "Any"))
                {
                    Logger.DebugFormat($"Found match with protocol name \"{protocolName}\"");
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public string FindFirstFilePath(string extensionPath)
        {
            try
            {
                var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<FilePathExtensionNode>(extensionPath);
                Logger.Debug($"Found {extensionPath} node: {node.FilePath}");
                return node.FilePath;
            }
            catch (ConfigurationErrorsException e)
            {
                Logger.Error($"Extension path {extensionPath} not found {e}");
                throw;
            }
        }
    }
}
