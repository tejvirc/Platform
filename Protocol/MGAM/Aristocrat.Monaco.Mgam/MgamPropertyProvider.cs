namespace Aristocrat.Monaco.Mgam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Common;
    using Kernel;

    /// <summary>
    /// MGAM Properties provider.
    /// </summary>
    public class MgamPropertyProvider : IPropertyProvider
    {
        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(Log4NetLoggerFactory.CreateLogger<MgamPropertyProvider>);

        private readonly Dictionary<string, (object Value, bool IsPersistent)> _properties;

        /// <summary>
        /// Initialized a new instance of the <see cref="MgamPropertyProvider"/> class.
        /// </summary>
        public MgamPropertyProvider()
        {
            _properties = new Dictionary<string, (object, bool)>
            {
                { PropertyNames.EgmId, (BuildEgmId(), false) },
                { PropertyNames.KnownRegistration, (false, false) },
                { PropertyNames.DirectoryPort, (MgamConstants.DefaultDirectoryPort, false) },
                { PropertyNames.ServiceName, (MgamConstants.DefaultServiceName, false) },
                { PropertyNames.CompressionThreshold, (MgamConstants.DefaultCompressionThreshold, false) }
            };
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Value)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var property))
            {
                return property.Value;
            }

            var errorMessage = "Unknown game property: " + propertyName;
            Logger.Value.LogError(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Value.LogError(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            _properties[propertyName] = (propertyValue, value.IsPersistent);
        }

        private static string BuildEgmId() =>
            $"{ProtocolConstants.ManufacturerPrefix}_{NetworkInterfaceInfo.DefaultPhysicalAddress}";
    }
}
