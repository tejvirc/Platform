namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.G2S.Client.Configuration;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Client.Security;
    using Aristocrat.G2S.Emdi;
    using Aristocrat.Monaco.G2S.Common;
    using Aristocrat.Monaco.G2S.Common.Data.Models;
    using Aristocrat.Monaco.G2S.Services;
    using Common.CertificateManager;
    using Data.Hosts;
    using Data.Profile;
    using DisableProvider;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Gaming.Progressives;
    using Handlers;
    using Handlers.CommConfig;
    using Handlers.OptionConfig;
    using Kernel;
    using log4net;
    using Meters;
    using Monaco.Common.Container;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Options;
    using PackageManifest;
    using PackageManifest.Ati;
    using PackageManifest.Models;
    using Protocol.Common.Installer;
    using Security;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using Constants = G2S.Constants;

    /// <summary>
    ///     Aristocrat.Monaco.G2S.CompositionRoot.Bootstrapper
    /// </summary>
    internal static class Bootstrapper
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Initializes the container.
        /// </summary>
        /// <returns>the container</returns>
        public static Container InitializeContainer()
        {
            return ConfigureContainer();
        }

        private static Container ConfigureContainer()
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Register<IEngine, G2SEngine>(Lifestyle.Singleton);
            container.Register<IG2SDisableProvider, G2SDisableProvider>(Lifestyle.Singleton);
            container.RegisterAuthenticationService(ConnectionString());
            container.RegisterPackageManager(ConnectionString());
            container.RegisterCertificateManager(ConnectionString());
            container.RegisterData(ConnectionString());
            container.AddDbContext();

            // Register the handlers
            container.ConfigureHandlers();

            // Register status changed handlers
            container.ConfigureStatusChangedHandlers();

            // Register the device descriptors
            container.ConfigureDeviceDescriptors();

            // Register the command builders
            container.ConfigureCommandBuilders();

            // Register the meter providers
            container.ConfigureMeterProviders();

            // Register the meter aggregator(s)
            container.ConfigureMeterAggregators();

            // Register the consumers
            container.ConfigureConsumers();

            // Register the instances handled by the service manager, so we won't need to use the service manager
            container.RegisterExternalServices();

            // Register the handlers services
            container.ConfigureServices();

            // Register the G2S Client
            container.ConfigureCommunications();

            // Register EMDI
            container.ConfigureEmdi();

            return container;
        }

        private static void ConfigureServices(this Container @this)
        {
            @this.Register<IDisableConditionSaga, DisableConditionSaga>(Lifestyle.Singleton);
            @this.Register<IVoucherDataService, VoucherDataService>(Lifestyle.Singleton);
            @this.Register<IMasterResetService, MasterResetService>(Lifestyle.Singleton);
            @this.Register<IVoucherValidator, VoucherValidator>(Lifestyle.Singleton);

            @this.Register<ITransactionReferenceProvider, TransactionReferenceProvider>(Lifestyle.Singleton);

            @this.Register<IHandpayService, HandpayService>(Lifestyle.Singleton);
            @this.Register<IHandpayProperties, HandpayService>(Lifestyle.Singleton);
            @this.Register<IHandpayValidator, HandpayService>(Lifestyle.Singleton);

            @this.Register<IdReaderService>(Lifestyle.Singleton);
            @this.Register<IInformedPlayerService, InformedPlayerService>(Lifestyle.Singleton);
            @this.Register<IIdReaderValidator, IdReaderValidator>(Lifestyle.Singleton);

            @this.Register<ICommChangeLogValidationService, CommChangeLogValidationService>(Lifestyle.Singleton);
            @this.Register<IApplyOptionConfigService, ApplyOptionConfigToDevicesService>(Lifestyle.Singleton);
            @this.Register<IOptionChangeLogValidationService, OptionChangeLogValidationService>(Lifestyle.Singleton);
            @this.Register<ITaskScheduler, TaskScheduler>(Lifestyle.Singleton);
            @this.Register<ISetOptionChangeValidateService, SetOptionChangeValidateService>(Lifestyle.Singleton);
            @this.Register<IPrintLog, PrintLogService>(Lifestyle.Singleton);
            @this.Register<IPackageLog, PackageLogService>(Lifestyle.Singleton);
            @this.Register<PlayerSessionService>(Lifestyle.Singleton);
            @this.Register<BonusService>(Lifestyle.Singleton);
            @this.Register<ICentralService, CentralService>(Lifestyle.Singleton);
            @this.Register<IHostStatusHandlerFactory, HostStatusHandlerFactory>(Lifestyle.Singleton);
            @this.Register<IDeviceDescriptorFactory, DeviceDescriptorFactory>(Lifestyle.Singleton);
            @this.Register<IFileSystemProvider, FileSystemProvider>(Lifestyle.Singleton);
            @this.RegisterManyAsCollection(typeof(IDeviceOptions), Assembly.GetExecutingAssembly());
            @this.RegisterManyAsCollection(typeof(IDeviceOptionsBuilder), Assembly.GetExecutingAssembly());

            @this.RegisterConditional<IConfigurationService, CommunicationsConfigurationService>(
                Lifestyle.Singleton,
                x => x.Consumer?.ImplementationType.Namespace == typeof(GetCommHostList).Namespace);

            @this.RegisterConditional<IConfigurationService, OptionConfigurationService>(
                Lifestyle.Singleton,
                x => x.Consumer?.ImplementationType.Namespace == typeof(GetOptionList).Namespace);

            @this.Register<IInstallerService, InstallerService>(Lifestyle.Singleton);
            @this.Register<IZipArchive, ZipArchive>(Lifestyle.Singleton);
            @this.Register<ITarArchive, TarArchive>(Lifestyle.Singleton);
            @this.Register<IPackageService, PackageService>(Lifestyle.Singleton);
            @this.Register<IManifest<Image>, ImageManifest>(Lifestyle.Singleton);
            @this.Register<IProgressiveDeviceManager, ProgressiveDeviceManager>(Lifestyle.Singleton);
            @this.Register<IProgressiveLevelManager, ProgressiveLevelManager>(Lifestyle.Singleton);
            @this.Register<IProgressiveService, ProgressiveService>(Lifestyle.Singleton);
            @this.Register<IAnalyticsService, AnalyticsService>(Lifestyle.Singleton);
        }

        private static void ConfigureMeterProviders(this Container @this)
        {
            @this.Register<IG2SMeterProvider, G2SMeterProvider>(Lifestyle.Singleton);
        }

        private static void ConfigureCommunications(this Container @this)
        {
            var egm = EgmFactory.Create(
                e =>
                {
                    e.UsesNamespace("Aristocrat.G2S.Protocol.v21");
                    e.ListenOn(ConfigureBinding);
                    e.WithEgmId(
                        ServiceManager.GetInstance().GetService<IPropertiesManager>()
                            .GetValue<string>(Constants.EgmId, null));
                });
            
            @this.Register<IDeviceFactory, DeviceFactory>(Lifestyle.Singleton);
            @this.Register<IGatComponentFactory, GatComponentFactory>(Lifestyle.Singleton);
            @this.Register<IHostFactory, HostFactory>(Lifestyle.Singleton);
            @this.Register<IEventLift, EventLift>(Lifestyle.Singleton);
            @this.Register<ITransportStateObserver, TransportStateObserver>(Lifestyle.Singleton);
            @this.Register<ICommunicationsStateObserver, CommunicationsStateObserver>(Lifestyle.Singleton);
            @this.Register<IDeviceObserver, DeviceObserver>(Lifestyle.Singleton);
            @this.Register<IProgressiveDeviceObserver, ProgressiveDeviceObserver>(Lifestyle.Singleton);
            @this.Register<IEgmStateObserver, EgmStateObserver>(Lifestyle.Singleton);
            @this.Register<IEgmStateManager, EgmStateManager>(Lifestyle.Singleton);
            @this.Register<IProfileService, ProfileService>(Lifestyle.Singleton);
            @this.Register<IHostService, HostService>(Lifestyle.Singleton);
            @this.Register<IEventPersistenceManager, EventPersistenceManager>(Lifestyle.Singleton);
            @this.Register<IScriptManager, ScriptManager>(Lifestyle.Singleton);
            @this.Register<IPackageDownloadManager, PackageDownloadManager>(Lifestyle.Singleton);
            @this.Register<IMetersSubscriptionManager, MetersSubscriptionManager>(Lifestyle.Singleton);
            @this.Register<ISelfTest, SelfTest>(Lifestyle.Singleton);

            @this.RegisterInstance(typeof(IG2SEgm), egm);
        }

        private static void ConfigureHandlers(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(ICommandHandler<,>),
                false,
                Assembly.GetExecutingAssembly());
        }

        private static void ConfigureStatusChangedHandlers(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(IStatusChangedHandler<>),
                false,
                Assembly.GetExecutingAssembly());
        }

        private static void ConfigureDeviceDescriptors(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(IDeviceDescriptor<>),
                false,
                Assembly.GetExecutingAssembly());
        }

        private static void ConfigureCommandBuilders(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(ICommandBuilder<,>),
                false,
                Assembly.GetExecutingAssembly());

            @this.Register<ICommHostListCommandBuilder, CommHostListCommandBuilder>(Lifestyle.Singleton);
            @this.Register<IOptionListCommandBuilder, OptionListCommandBuilder>(Lifestyle.Singleton);
        }

        private static void ConfigureMeterAggregators(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(IMeterAggregator<>),
                false,
                Assembly.GetExecutingAssembly());
        }

        private static void ConfigureConsumers(this Container @this)
        {
            @this.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
        }

        private static string ConnectionString()
        {
            var dir = ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(Constants.DataPath);
            var path = Path.GetFullPath(dir.FullName);

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Path.Combine(path, Constants.DatabaseFileName)
            };

            return sqlBuilder.ConnectionString;
        }

        private static void ConfigureBinding(IBindingInfo bindingInfo)
        {
            X509Certificate2 certificate = null;

            // Adding only Ssl3 suddenly stopped working with a Win10 update, so we're going to forcibly set all required types
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                    SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var port = properties.GetValue(Constants.Port, Constants.DefaultPort);
            var bypassCertificateValidation = properties.GetValue(Constants.BypassCertificateValidation, false);

            Firewall.AddRule(Constants.FirewallRuleName, (ushort)port);

            var hosts = properties.GetValues<IHost>(Constants.RegisteredHosts);
            if (hosts.Any(h => h.Registered && h.Address.Scheme == Uri.UriSchemeHttps))
            {
                var certificateService =
                    ServiceManager.GetInstance().GetService<ICertificateFactory>().GetCertificateService();

                var validation = new CertificateValidation(certificateService);

                if (bypassCertificateValidation)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, x509Certificate, chain, errors) => true;
                }
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback += validation.OnValidateCertificate;
                }

                var certificates = certificateService.GetCertificates();

                foreach (var cert in certificates)
                {
                    var x509Cert = cert.ToX509Certificate2();

                    if (cert.Default && x509Cert.HasPrivateKey)
                    {
                        certificate = CertificateStore.GetOrAdd(
                            StoreLocation.LocalMachine,
                            X509FindType.FindByThumbprint,
                            cert.Thumbprint,
                            () => x509Cert);

                        Logger.Debug($"Added default cert to the store: {x509Cert.Thumbprint}");
                    }
                    else
                    {
                        CertificateStore.Add(StoreName.Root, StoreLocation.LocalMachine, x509Cert);

                        Logger.Debug($"Added non-default cert to the store: {x509Cert.Thumbprint}");
                    }
                }

                if (certificate == null)
                {
                    certificate = CertificateStore.GetOrAdd(
                        StoreLocation.LocalMachine,
                        X509FindType.FindByThumbprint,
                        Constants.DefaultCertificateThumbprint,
                        () =>
                            new X509Certificate2(
                                Constants.DefaultCertificate,
                                Constants.CertificatePassword,
                                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet));

                    Logger.Debug($"Added self-signed cert to the store: {certificate.Thumbprint}");
                }

                var endpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);

                ClientBinding.AddOrUpdate(
                    endpoint,
                    certificate.Thumbprint,
                    () => certificate != null
                        ? new CertificateBinding(
                            certificate.Thumbprint,
                            StoreName.My,
                            endpoint,
                            Constants.ApplicationId,
                            new BindingOptions
                            {
                                VerifyCertificateRevocation = false,
                                UsageCheck = false,
                                NegotiateClientCertificate = true
                            })
                        : null);

                var clientAddress = new UriBuilder(
                    Uri.UriSchemeHttps,
                    GetHostName(),
                    port,
                    Constants.ResourcePath);

                bindingInfo.Address = clientAddress.Uri;
                bindingInfo.Certificate = certificate;
                bindingInfo.BypassCertificateValidation = bypassCertificateValidation;
                bindingInfo.Validator = new ClientCertificateValidator(certificateService);

                Logger.Debug($"Created endpoint at {clientAddress.Uri} using certificate {certificate.Thumbprint}");
            }
            else
            {
                var clientAddress = new UriBuilder(
                    Uri.UriSchemeHttp,
                    GetHostName(),
                    port,
                    Constants.ResourcePath);

                bindingInfo.Address = clientAddress.Uri;

                Logger.Debug($"Created endpoint at {clientAddress.Uri}");
            }
        }

        private static string GetHostName()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            if (!string.IsNullOrEmpty(ipProperties.DomainName))
            {
                return $"{ipProperties.HostName}.{ipProperties.DomainName}";
            }

            return NetworkInterfaceInfo.DefaultIpAddress?.ToString() ?? Aristocrat.G2S.Client.Constants.DefaultUrl;
        }
    }
}