namespace Aristocrat.Monaco.Mgam.UI.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Aristocrat.Monaco.UI.Common;
    using Common;
    using Common.Configuration;
    using Common.Data;
    using Common.Data.Models;
    using Common.Events;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     The view model for the host configuration page.
    /// </summary>
    public class HostConfigurationViewModel : ConfigWizardViewModelBase
    {
        private IPathMapper PathMapper => _pathMapper ?? (_pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>());

        private int _directoryPort;
        private string _serviceName;
        private string _macAddress;
        private string _deviceName;
        private Guid _deviceId;
        private string _directoryIpAddress = string.Empty;
        private string _previousIpAddress = string.Empty;
        private bool _useUdpBroadcasting = true;
        private IPathMapper _pathMapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationViewModel"/> class.
        /// </summary>
        public HostConfigurationViewModel(bool isWizardPage)
            : base(isWizardPage)
        {
        }

        /// <summary>
        ///     Gets or sets the directory service port.
        /// </summary>
        public int DirectoryPort
        {
            get => _directoryPort;
            set
            {
                if (_directoryPort != value)
                {
                    ClearErrors(nameof(DirectoryPort));

                    if (value <= 0 || value > ushort.MaxValue)
                    {
                        SetError(nameof(DirectoryPort), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port_MustBeInRange));
                    }
                    _directoryPort = value;
                    OnPropertyChanged(nameof(DirectoryPort));
                    SetupNavigation();
                }
            }
        }

        /// <summary>
        /// Gets or Sets UseUdpBroadcasting
        /// </summary>
        public bool UseUdpBroadcasting
        {
            get => _useUdpBroadcasting;

            set
            {
                if (value != _useUdpBroadcasting)
                {
                    _useUdpBroadcasting = value;
                    if (value)
                    {
                        _previousIpAddress = DirectoryIpAddress;
                        DirectoryIpAddress = string.Empty;
                    }
                    else
                    {
                        DirectoryIpAddress = _previousIpAddress;
                    }
                    OnPropertyChanged(nameof(UseUdpBroadcasting));
                }
            }
        }

        /// <summary>
        /// Gets or Sets DirectoryIpAddress
        /// </summary>
        public string DirectoryIpAddress
        {
            get => _directoryIpAddress;

            set
            {
                var trimmed = value.Trim();
                if (trimmed != _directoryIpAddress)
                {
                    _directoryIpAddress = trimmed;
                    OnPropertyChanged(nameof(DirectoryIpAddress));
                }
                ValidateNetworkAddress(trimmed);
                SetupNavigation();
            }
        }

        /// <summary>
        ///     Gets or sets the VLT service name to locate.
        /// </summary>
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (_serviceName != value)
                {
                    ClearErrors(nameof(ServiceName));

                    if (string.IsNullOrEmpty(value))
                    {
                        SetError(nameof(ServiceName), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage));
                    }
                    _serviceName = value;
                    OnPropertyChanged(nameof(ServiceName));
                    SetupNavigation();
                }
            }
        }

        /// <summary>
        ///     Gets mac address
        /// </summary>
        public string MacAddress
        {
            get => _macAddress;
            set => SetProperty(ref _macAddress, value);
        }

        /// <summary>
        ///     Gets device name
        /// </summary>
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        /// <summary>
        ///     Gets device id
        /// </summary>
        public Guid DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value, nameof(DeviceId));
        }

        /// <inheritdoc />
        protected override void Loaded()
        {
            if (IsWizardPage)
            {
                DirectoryPort = PropertiesManager.GetValue(PropertyNames.DirectoryPort, 0);
            }

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            ServiceName = PropertiesManager.GetValue(PropertyNames.ServiceName, string.Empty);
            MacAddress = NetworkInterfaceInfo.DefaultPhysicalAddress;
            DeviceName = properties.GetValue(ApplicationConstants.CalculatedDeviceName, string.Empty);

            using (var context = new MgamContext(new DefaultConnectionStringResolver(PathMapper)))
            {
                // get existing device GUID; else generate
                DeviceId = context.Devices.Any()
                    ? context.Devices.First().DeviceGuid
                    : GuidUtility.Create(GuidUtility.DnsNamespace, DeviceName);

                if (!IsWizardPage)
                {
                    DirectoryPort = context.Hosts.First().DirectoryPort;
                    ServiceName = context.Hosts.First().ServiceName;
                    UseUdpBroadcasting = context.Hosts.First().UseUdpBroadcasting;
                    DirectoryIpAddress = context.Hosts.First().DirectoryIpAddress;
                }
            }

        }

        /// <inheritdoc />
        protected override void SaveChanges()
        {
            if (HasErrors) return;

            PropertiesManager.SetProperty(PropertyNames.DirectoryPort, DirectoryPort);
            PropertiesManager.SetProperty(PropertyNames.ServiceName, ServiceName);

            EventBus.Publish(new OperatorMenuSettingsChangedEvent());

            using (var context = new MgamContext(new DefaultConnectionStringResolver(PathMapper)))
            {
                if (IsWizardPage)
                {
                    CreateDatabase(context);
                    context.SaveChanges();
                }
                else
                {
                    context.Hosts.First().DirectoryPort = DirectoryPort;
                    context.Hosts.First().ServiceName = ServiceName;
                    context.Hosts.First().UseUdpBroadcasting = UseUdpBroadcasting;
                    context.Hosts.First().DirectoryIpAddress = DirectoryIpAddress;

                    if (context.ChangeTracker.HasChanges())
                    {
                        context.SaveChanges();
                        EventBus.Publish(new HostConfigurationChangedEvent(ServiceName, DirectoryIpAddress, DirectoryPort, UseUdpBroadcasting));
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void SetupNavigation()
        {
            if (!IsWizardPage) return;

            WizardNavigator.CanNavigateBackward = true;
            WizardNavigator.CanNavigateForward = !HasErrors;
        }

        private void CreateDatabase(MgamContext context)
        {
            Logger.Info("Create database started...");

            if (context.Hosts.SingleOrDefault() == null)
            {
                Logger.Info("Creating Host table...");

                context.Hosts.Add(
                        new Host
                        {
                            ServiceName = ServiceName,
                            DirectoryPort = DirectoryPort,
                            IcdVersion = 2,
                            UseUdpBroadcasting = UseUdpBroadcasting,
                            DirectoryIpAddress = DirectoryIpAddress
                        });
            }

            var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            if (context.Devices.SingleOrDefault() == null)
            {
                Logger.Info("Creating Device table...");

                context.Devices.Add(
                    new Device
                    {
                        ManufacturerName = MgamConstants.ManufacturerName,
                        DeviceGuid = DeviceId,
                        Name = DeviceName
                    });
            }

            if (context.Installations.SingleOrDefault() == null)
            {
                Logger.Info("Creating Installation table...");

                var name = $"{fileVersion.ProductName} {fileVersion.FileVersion}";

                context.Installations.Add(
                    new Installation
                    {
                        InstallationGuid = GuidUtility.Create(GuidUtility.DnsNamespace, name),
                        Name = name
                    });
            }

            if (context.Applications.SingleOrDefault() == null)
            {
                Logger.Debug("Creating Application table...");

                context.Applications.Add(
                    new Application
                    {
                        ApplicationGuid = GuidUtility.Create(GuidUtility.DnsNamespace, $"{fileVersion.ProductName} {fileVersion.FileVersion}"),
                        Name = fileVersion.ProductName,
                        Version = fileVersion.FileVersion
                    });
            }

            if (context.Certificates.SingleOrDefault() == null)
            {
                Logger.Debug("Creating Certificate table...");

                var configuration = ConfigurationUtilities.GetConfiguration<MgamConfiguration>(
                    MgamConstants.ConfigurationExtensionPath,
                    () => throw new InvalidOperationException(
                        $"MGAM configuration is not defined in Jurisdiction configuration, {MgamConstants.ConfigurationExtensionPath}"));

                var certPath = Path.GetFullPath(configuration.CertificateAuthority.FilePath);

                context.Certificates.Add(CertificateExtensions.LoadCertificate(certPath));
            }

            Logger.Info("Create database complete.");
        }

        private void ValidateNetworkAddress(string address)
        {
            ClearErrors(nameof(DirectoryIpAddress));

            if (!UseUdpBroadcasting && !IpValidation.IsIpV4AddressValid(address))
            {
                SetError(nameof(DirectoryIpAddress), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid));
            }
        }

        /// <inheritdoc />
        protected override void LoadAutoConfiguration()
        {
            string stringValue = null;
            var boolValue = false;

            if (AutoConfigurator.GetValue("MgamUseUdpBroadcasting", ref boolValue))
            {
                UseUdpBroadcasting = boolValue;
            }

            // If Use UDP Broadcasting is true, ignore the Directory IP
            if (!UseUdpBroadcasting && AutoConfigurator.GetValue("MgamDirectoryIpAddress", ref stringValue))
            {
                DirectoryIpAddress = stringValue;
            }

            if (string.IsNullOrEmpty(DirectoryIpAddress))
            {
                // If no valid IP is used then default to use UDP broadcasting
                UseUdpBroadcasting = true;
            }

            base.LoadAutoConfiguration();
        }
    }
}
