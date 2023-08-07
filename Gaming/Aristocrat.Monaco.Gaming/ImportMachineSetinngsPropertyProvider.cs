namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A <see cref="IPropertyProvider" /> implementation for the gaming layer used to pre-load the
    ///     the <see cref="PropertyProvider" /> class when importing machine settings.
    /// </summary>
    public class ImportMachineSettingsPropertyProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportMachineSettingsPropertyProvider" /> class.
        /// </summary>
        public ImportMachineSettingsPropertyProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportMachineSettingsPropertyProvider" /> class.
        /// </summary>
        /// <param name="storageManager">the storage manager</param>
        public ImportMachineSettingsPropertyProvider(IPersistentStorageManager storageManager)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            var propertyProvider = new PropertyProvider(storageManager);
            if (propertyProvider == null)
            {
                throw new ArgumentNullException(nameof(propertyProvider));
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            => new List<KeyValuePair<string, object>>();

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            var errorMessage = "GetProperty not implemented, unable to get property: " + propertyName;
            Logger.Error(errorMessage);
            throw new NotImplementedException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            var errorMessage = "SetProperty not implemented, unable to set property: " + propertyName;
            Logger.Error(errorMessage);
            throw new NotImplementedException(errorMessage);
        }
    }
}
