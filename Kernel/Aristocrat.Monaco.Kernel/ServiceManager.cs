namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     ServiceManager provides the singleton access to the concrete ServiceManager.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Singleton of the IServiceManager.
        /// </summary>
        private static IServiceManager _instance;

        /// <summary>
        ///     Gets the ServiceManager instance.
        /// </summary>
        /// <returns>Instance of the ServiceManager.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     Thrown when ServiceManager is unable to load because zero or more than
        ///     one extension node was found.
        /// </exception>
        public static IServiceManager GetInstance()
        {
            if (_instance == null)
            {
                var nodes = AddinManager.GetExtensionNodes("/Kernel/ServiceManager");

                if (nodes.Count != 1)
                {
                    Logger.Fatal(
                        "Unable to load ServiceManager\n nodes found: " +
                        Convert.ToString(nodes.Count, CultureInfo.InvariantCulture));
                    throw new ConfigurationErrorsException(
                        "Unable to load ServiceManager\n nodes found: " +
                        Convert.ToString(nodes.Count, CultureInfo.InvariantCulture));
                }

                TypeExtensionNode typeExtensionNode = (TypeExtensionNode)nodes[0];

                _instance = (IServiceManager)typeExtensionNode.CreateInstance();
            }

            return _instance;
        }
    }
}