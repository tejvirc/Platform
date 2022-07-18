namespace Aristocrat.Monaco.Sas.PropertiesProvider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Protocol;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;
    using Storage;
    using Storage.Models;

    /// <summary>Definition of the SasStaticPropertiesProvider class.</summary>
    public class SasStaticPropertiesProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string SasDefaultConfigurationExtensionPath = "/SAS/DefaultConfiguration";
        private Dictionary<string, object> _properties;

        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;
        private readonly ISasDataFactory _sasDataFactory;

        /// <summary>
        ///     Default constructor used by Mono Addins
        /// </summary>
        public SasStaticPropertiesProvider() :
            this(ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                 ServiceManager.GetInstance().GetService<IEventBus>(),
                 ServiceManager.GetInstance().TryGetService<IMultiProtocolConfigurationProvider>(),
                 ServiceManager.GetInstance().GetService<ISasDataFactory>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the SasStaticPropertiesProvider class.
        ///     This constructor is used by unit tests to allow mocking the parameters.
        /// </summary>
        /// <param name="propertiesManager">Reference to properties manager</param>
        /// <param name="eventBus">Reference to the event bus</param>
        /// <param name="multiProtocolConfigurationProvider">Reference to the Multi Protocol Configuration Provider</param>
        /// <param name="sasDataFactory"></param>
        public SasStaticPropertiesProvider(
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            ISasDataFactory sasDataFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _sasDataFactory = sasDataFactory ?? throw new ArgumentNullException(nameof(sasDataFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            SetDefaultProperties();

            // if this is the first boot and we haven't selected a protocol yet, watch for
            // when we finish selecting the jurisdiction/protocol configuration and update the
            // SAS configuration values for the jurisdiction
            if (multiProtocolConfigurationProvider == null || !multiProtocolConfigurationProvider.MultiProtocolConfiguration.Any())
            {
                _eventBus.Subscribe<PreConfigBootCompleteEvent>(this, HandlePreConfigComplete);
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => _properties.ToList();

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (!_properties.TryGetValue(propertyName, out var returnObject))
            {
                throw new UnknownPropertyException(propertyName);
            }

            Logger.Info($"[CONFIG] The property {propertyName} reads as {returnObject}");
            return returnObject;
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                throw new UnknownPropertyException(propertyName);
            }

            _properties[propertyName] = propertyValue;
            Logger.Info($"[CONFIG] Setting the property {propertyName} to {propertyValue}");
            switch (propertyName)
            {
                case SasProperties.SasPortAssignments when propertyValue is PortAssignment assignment:
                    _sasDataFactory.GetConfigurationService().SavePortAssignment(assignment);
                    break;
                case SasProperties.SasHosts when propertyValue is IEnumerable hosts:
                    _sasDataFactory.GetConfigurationService().SasHosts(hosts.Cast<Host>());
                    break;
                case SasProperties.SasFeatureSettings when propertyValue is SasFeatures settings:
                    _sasDataFactory.GetConfigurationService().SaveSasFeatures(settings);
                    break;
            }
        }

        private void HandlePreConfigComplete(PreConfigBootCompleteEvent obj)
        {
            if (_propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None) == ImportMachineSettings.None)
            {
                UpdateConfigurationSettings();
            }

            _eventBus.Unsubscribe<PreConfigBootCompleteEvent>(this);
        }

        private void UpdateConfigurationSettings()
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                SasDefaultConfigurationExtensionPath,
                () => new SASDefaultConfiguration
                {
                    SASHostPage = new SASDefaultConfigurationSASHostPage
                    {
                        EGMDisabledOnPowerUp = new SASDefaultConfigurationSASHostPageEGMDisabledOnPowerUp
                        {
                            Enabled = false
                        },
                        EGMDisabledOnHostOffline = new SASDefaultConfigurationSASHostPageEGMDisabledOnHostOffline
                        {
                            Enabled = false,
                            Configurable = false
                        },
                        TransferLimit = new SASDefaultConfigurationSASHostPageTransferLimit
                        {
                            Default = SasConstants.MaxAftTransferLimitDefaultAmount,
                            MaxAllowed = SasConstants.MaxAftTransferAmount
                        },
                    }
                });

            var ports = (PortAssignment)GetProperty(SasProperties.SasPortAssignments);
            ports.IsDualHost = configuration.SASHostPage?.MustHaveDualHost?.Enabled ?? false;
            ports.FundTransferPort = configuration.SASHostPage?.FundTransferHost?.HostId ?? HostId.Host1;
            ports.ValidationPort = configuration.SASHostPage?.Validation?.HostId ?? HostId.Host1;
            ports.GameStartEndHosts = configuration.SASHostPage?.GameStartEnd?.HostId ?? GameStartEndHost.Host1;
            SetProperty(SasProperties.SasPortAssignments, ports);

            var sasSettings = (SasFeatures)GetProperty(SasProperties.SasFeatureSettings);
            sasSettings.OverflowBehavior = configuration.SASHostPage?.ExceptionOverflow?.Behaviour ??
                                           ExceptionOverflowBehavior.DiscardOldExceptions;
            sasSettings.ConfigNotification =
                configuration.SASHostPage?.ConfigurationChangeNotification?.NotificationType ??
                ConfigNotificationTypes.Always;
            sasSettings.DisableOnDisconnect = configuration.SASHostPage?.EGMDisabledOnHostOffline?.Enabled ?? false;
            sasSettings.TransferLimit = (long)(configuration.SASHostPage?.TransferLimit?.Default ??
                                               SasConstants.MaxAftTransferLimitDefaultAmount);
            sasSettings.DisabledOnPowerUp = configuration.SASHostPage?.EGMDisabledOnPowerUp?.Enabled ?? false;
            sasSettings.MaxAllowedTransferLimits = (long)(configuration.SASHostPage?.TransferLimit?.MaxAllowed ??
                                                          SasConstants.MaxAftTransferAmount);
            sasSettings.DisableOnDisconnectConfigurable =
                configuration.SASHostPage?.EGMDisabledOnHostOffline?.Configurable ?? false;
            sasSettings.GeneralControlEditable = configuration.SASHostPage?.GeneralControl?.Editable ?? true;
            sasSettings.AddressConfigurableOnlyOnce =
                configuration.SASHostPage?.AddressConfigurationOnceOnly?.Enabled ?? false;
            sasSettings.BonusTransferStatusEditable = configuration.SASHostPage?.BonusTransferStatus?.Editable ?? true;
            SetProperty(SasProperties.SasFeatureSettings, sasSettings);
        }

        private void SetDefaultProperties()
        {
            var configurationService = _sasDataFactory.GetConfigurationService();
            _properties = new Dictionary<string, object>
            {
                { SasProperties.SasPortAssignments, configurationService.GetPortAssignment() },
                { SasProperties.SasFeatureSettings, configurationService.GetSasFeatures() },
                { SasProperties.SasHosts, _sasDataFactory.GetConfigurationService().GetHosts().ToList() }
            };
        }
    }
}
