namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Implementation of IPropertiesManager for use in unit tests of components
    ///     that require a properties manager service.
    /// </summary>
    public sealed class TestPropertiesManager : IService, IPropertiesManager, IDisposable
    {
        /// <summary>
        ///     Create a m_log for use in this class
        /// </summary>
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Holds a reference to the default property provider which is used to
        ///     handle keys that other providers don't handle.
        /// </summary>
        private DefaultPropertyProvider defaultProvider = new DefaultPropertyProvider();

        /// <summary>
        ///     Indicates whether or not the instance has been disposed
        /// </summary>
        private bool m_disposed;

        /// <summary>
        ///     Maps a property to the property provider that manages that property.
        /// </summary>
        private Dictionary<string, IPropertyProvider> propertyProviders = new Dictionary<string, IPropertyProvider>();

        /// <summary>
        ///     A list of unique property providers.
        /// </summary>
        private List<IPropertyProvider> uniqueProviders = new List<IPropertyProvider>();

        /// <summary>
        ///     Initializes a new instance of the TestPropertiesManager class.
        /// </summary>
        public TestPropertiesManager()
        {
            m_log.Debug("Adding the default property provider");
            this.AddPropertyProvider(this.defaultProvider);
            this.uniqueProviders.Add(this.defaultProvider);

            ++InstanceCount;
            m_log.InfoFormat("Constructed, InstanceCount = {0}", InstanceCount);
        }

        /// <summary>
        ///     Gets the number of instantiated object instances at this time.
        /// </summary>
        public static int InstanceCount { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether or not the instantiated object was initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        ///     Gets the number of providers managed by this instance.
        /// </summary>
        public int ProvidersCount
        {
            get { return this.uniqueProviders.Count; }
        }

        /// <summary>
        ///     Gets the number of keys in our property collection.
        /// </summary>
        public int PropertiesCount
        {
            get { return this.propertyProviders.Count; }
        }

        /// <summary>
        ///     Gets the collection of managed property names.
        /// </summary>
        public ICollection<string> Properties
        {
            get { return this.propertyProviders.Keys; }
        }

        /// <summary>
        ///     The dispose method for this class.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;
                GC.SuppressFinalize(this);
                this.propertyProviders.Clear();
                this.defaultProvider.Dispose();
            }
        }

        /// <summary>
        ///     Adds all the key/value pairs a property provider controls to our collection of key->provider
        /// </summary>
        /// <param name="provider">The provider of a property or properties</param>
        /// <exception cref="ArgumentException">
        ///     Will throw an Argument Exception if a property with the same name is already
        ///     present
        /// </exception>
        public void AddPropertyProvider(IPropertyProvider provider)
        {
            ICollection<KeyValuePair<string, object>> collection = provider.GetCollection;
            if (collection.Count > 0)
            {
                m_log.DebugFormat(CultureInfo.InvariantCulture, "Adding property provider: {0}", provider);
                this.uniqueProviders.Add(provider);

                // Iterate through the list of properties being provided and add them to our collection
                foreach (KeyValuePair<string, object> property in provider.GetCollection)
                {
                    m_log.DebugFormat("Adding property {0}", property.Key);
                    this.propertyProviders.Add(property.Key, provider);
                }
            }
        }

        /// <summary>
        ///     Gets a property.
        /// </summary>
        /// <param name="propertyName">The name of the property to get</param>
        /// <param name="defaultValue">the default value to return if the property wasn't found</param>
        /// <returns>The property associated with the property name, or the default value if the property wasn't found.</returns>
        public object GetProperty(string propertyName, object defaultValue)
        {
            IPropertyProvider propertyProvider;

            // check if the property key is in our dictionary
            if (this.propertyProviders.TryGetValue(propertyName, out propertyProvider))
            {
                object foundProperty = propertyProvider.GetProperty(propertyName);
                m_log.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Property found for property name: {0}, value: {1}",
                    propertyName,
                    foundProperty);
                return foundProperty;
            }

            m_log.WarnFormat(
                CultureInfo.InvariantCulture,
                "No property found for property name: {0}, returning default value",
                propertyName);

            // nothing matched if we got to here.
            return defaultValue;
        }

        /// <summary>
        ///     Sets a property value. If the property doesn't exist, a new property is
        ///     created in the default property provider.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        public void SetProperty(string propertyName, object propertyValue)
        {
            m_log.DebugFormat(
                CultureInfo.InvariantCulture,
                "Setting property: {0} with value {1}",
                propertyName,
                propertyValue);
            IPropertyProvider propertyProvider;

            // check if the property key is already in our dictionary
            if (this.propertyProviders.TryGetValue(propertyName, out propertyProvider))
            {
                propertyProvider.SetProperty(propertyName, propertyValue);
            }
            else
            {
                m_log.Info("Did not find property - adding it");

                // didn't find the property key. Assume its a new property and add it to the default property provider
                this.defaultProvider.SetProperty(propertyName, propertyValue);

                // also add it to our dictionary
                this.propertyProviders.Add(propertyName, this.defaultProvider);
            }
        }

        /// <summary>
        ///     Sets a property value.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="isConfig">Indicates whether or not this is a property used for configuration.</param>
        public void SetProperty(string propertyName, object propertyValue, bool isConfig)
        {
            m_log.DebugFormat(
                CultureInfo.InvariantCulture,
                "Setting property: {0} with value {1}",
                propertyName,
                propertyValue);
            IPropertyProvider propertyProvider;

            // check if the property key is already in our dictionary
            if (this.propertyProviders.TryGetValue(propertyName, out propertyProvider))
            {
                propertyProvider.SetProperty(propertyName, propertyValue);
            }
            else
            {
                m_log.Info("Did not find property - adding it");

                // didn't find the property key. Assume its a new property and add it to the default property provider
                this.defaultProvider.SetProperty(propertyName, propertyValue);

                // also add it to our dictionary
                this.propertyProviders.Add(propertyName, this.defaultProvider);
            }
        }

        /// <summary>
        ///     Gets the name of the service.
        /// </summary>
        /// <returns>The name of the service as a string.</returns>
        public string Name
        {
            get { return "Test Properties Manager"; }
        }

        /// <summary>
        ///     Gets the type of service offered.
        /// </summary>
        /// <returns>A service type.</returns>
        public ICollection<Type> ServiceTypes
        {
            get { return new[] { typeof(IPropertiesManager) }; }
        }

        /// <summary>
        ///     Initializes the instance
        /// </summary>
        /// <exception cref="ServiceException">
        ///     Thrown if already initialized
        /// </exception>
        public void Initialize()
        {
            if (Initialized)
            {
                string errorMessage = "Cannot initialize more than once";
                m_log.Error(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Initialized = true;
            m_log.Debug("Initialized");
        }

        /// <summary>
        ///     Finalizes an instance of the TestPropertiesManager class.
        /// </summary>
        ~TestPropertiesManager()
        {
            --InstanceCount;
            m_log.InfoFormat("Destructed, InstanceCount = {0}", InstanceCount);
        }

        /// <summary>
        ///     Returns whether or not an object of the given type is registered as a provider.
        /// </summary>
        /// <param name="providerType">The type of object</param>
        /// <returns>
        ///     Whether or not an object of the given type is registered as a provider.
        /// </returns>
        public bool IsProvider(Type providerType)
        {
            bool isProvider = false;
            foreach (IPropertyProvider provider in this.uniqueProviders)
            {
                if (provider.GetType() == providerType)
                {
                    isProvider = true;
                    break;
                }
            }

            return isProvider;
        }

        /// <summary>
        ///     Returns whether or not a property provider is registered with this manager.
        /// </summary>
        /// <param name="provider">A reference to the provider.</param>
        /// <returns>
        ///     Whether or not a property provider is registered with this manager.
        /// </returns>
        public bool IsProvider(IPropertyProvider provider)
        {
            return this.uniqueProviders.Contains(provider);
        }

        /// <summary>
        ///     Returns whether or not a property with the supplied name is provided
        ///     by this properties manager.
        /// </summary>
        /// <param name="propertyName">The name of the property in question.</param>
        /// <returns>
        ///     Whether or not a property with the supplied name is provided
        ///     by this properties manager.
        /// </returns>
        public bool IsProvided(string propertyName)
        {
            return this.propertyProviders.ContainsKey(propertyName);
        }
    }
}