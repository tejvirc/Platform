namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     Property provider for the G2S protocol
    /// </summary>
    public class G2SPropertyProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SDataFactory _dataFactory;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, object> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SPropertyProvider" /> class.
        /// </summary>
        public G2SPropertyProvider()
        {
            _dataFactory = ServiceManager.GetInstance().GetService<IG2SDataFactory>();

            _persistentStorageAccessor = ServiceManager.GetInstance().GetService<IPersistentStorageManager>()
                .GetAccessor(PersistenceLevel.Critical, GetType().ToString());

            var port = (int)InitFromStorage(Constants.Port);

            _properties = new Dictionary<string, object>
            {
                { Constants.EgmId, BuildEgmId() },
                { Constants.RegisteredHosts, InitHostsFromStorage() },
                { Constants.StartupContext, InitStartupContext() },
                { Constants.Port, port != 0 ? port : Constants.DefaultPort }
            };
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            => new List<KeyValuePair<string, object>>(_properties);

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            var errorMessage = "Unknown G2S property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (_properties.ContainsKey(propertyName))
            {
                Logger.Debug(
                    $"Setting G2S property {propertyName} to {propertyValue}. Type is {propertyValue?.GetType()}");
                _properties[propertyName] = propertyValue;
            }
            else
            {
                var errorMessage = "Cannot set unknown G2S property: " + propertyName;
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            Logger.Info($"[CONFIG] Setting the G2S property {propertyName} with {propertyValue}");

            // NOTE:  Not all properties are persisted
            switch (propertyName)
            {
                case Constants.RegisteredHosts:
                    var hostService = _dataFactory.GetHostService();

                    if (propertyValue is IEnumerable hosts)
                    {
                        var hostArray = hosts.Cast<IHost>().ToList();

                        hostService.Save(
                            hostArray.Select(
                                h => new Data.Model.Host
                                {
                                    Id = h.Index,
                                    HostId = h.Id,
                                    Address = h.Address.ToString(),
                                    Registered = h.Registered,
                                    RequiredForPlay = h.RequiredForPlay
                                }));
                    }

                    break;

                case Constants.StartupContext:
                    if (propertyValue == null)
                    {
                        _persistentStorageAccessor[Constants.StartupContext] = null;
                    }
                    else if (propertyValue is StartupContext)
                    {
                        _persistentStorageAccessor[Constants.StartupContext] =
                            JsonConvert.SerializeObject(propertyValue, Formatting.None);
                    }

                    break;

                case Constants.Port:
                    _persistentStorageAccessor[Constants.Port] = propertyValue;
                    break;
            }

            _properties[propertyName] = propertyValue;
        }

        private static string BuildEgmId()
        {
            return
                $"{Aristocrat.G2S.Client.Constants.ManufacturerPrefix}_{NetworkInterfaceInfo.DefaultPhysicalAddress}";
        }

        private IEnumerable<IHost> InitHostsFromStorage()
        {
            var hosts = _dataFactory.GetHostService().GetAll();

            return hosts.Select(
                h =>
                    new Host
                    {
                        Index = (int)h.Id,
                        Id = h.HostId,
                        Address = string.IsNullOrEmpty(h.Address)
                            ? null
                            : new Uri(h.Address),
                        Registered = h.Registered,
                        RequiredForPlay = h.RequiredForPlay
                    });
        }

        private StartupContext InitStartupContext()
        {
            var context = (string)_persistentStorageAccessor[Constants.StartupContext];

            return string.IsNullOrEmpty(context) ? null : JsonConvert.DeserializeObject<StartupContext>(context);
        }

        private object InitFromStorage(string propertyName)
        {
            return _persistentStorageAccessor[propertyName];
        }
    }
}