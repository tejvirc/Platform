namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class is the default property provider for the system.
    ///     Components would add new properties to this property provider.
    /// </summary>
    internal sealed class DefaultPropertyProvider : IPropertyProvider, IDisposable
    {
        /// <summary>
        ///     Create a m_log for use in this class
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Indicates whether or not the instance has been disposed
        /// </summary>
        private bool m_disposed;

        /// <summary>
        ///     Maps a property name to the property.
        /// </summary>
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        ///     Provides a locking mechanism when reading/writing properties from threads.
        /// </summary>
        private ReaderWriterLockSlim propertyAccessLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     The dispose method for this class. Just dispose of the lock.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;
                GC.SuppressFinalize(this);
                this.propertyAccessLock.Dispose();
            }
        }

        /// <summary>
        ///     Gets a reference to a property provider collection of properties.
        /// </summary>
        /// <returns>A read only reference to our private collection</returns>
        public ICollection<KeyValuePair<string, object>> GetCollection
        {
            // return a read only reference to our collection
            get { return this.properties; }
        }

        /// <summary>
        ///     Get an existing property value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>an object which contains the property. Null if the property wasn't found.</returns>
        /// <exception cref="UnknownPropertyException">
        ///     Thrown when the propertyName is not recognized.
        /// </exception>
        public object GetProperty(string propertyName)
        {
            this.propertyAccessLock.EnterReadLock();

            try
            {
                if (!this.properties.ContainsKey(propertyName))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("Cannot get value of unrecognized property: {0}", propertyName);
                    logger.Error(builder.ToString());
                    throw new UnknownPropertyException(builder.ToString());
                }

                object property = this.properties[propertyName];

                logger.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Getting property with key: {0}, value is: {1}",
                    propertyName,
                    property);

                return property;
            }
            finally
            {
                this.propertyAccessLock.ExitReadLock();
            }
        }

        /// <summary>
        ///     Set a property to a value. If the property doesn't exist, a new
        ///     property is created and set to the given value.
        ///     If the property does exist, it is set to the new value.
        ///     No checks are made on the value, i.e. you could write a string value to
        ///     something that was previously an int.
        /// </summary>
        /// <param name="propertyName">The property to set or create.</param>
        /// <param name="propertyValue">The new value for the property.</param>
        public void SetProperty(string propertyName, object propertyValue)
        {
            logger.DebugFormat(
                CultureInfo.InvariantCulture,
                "Setting property with key: {0}, value: {1}",
                propertyName,
                propertyValue);

            this.propertyAccessLock.EnterWriteLock();

            try
            {
                this.properties[propertyName] = propertyValue;
            }
            finally
            {
                this.propertyAccessLock.ExitWriteLock();
            }
        }
    }
}