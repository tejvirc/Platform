namespace Aristocrat.Monaco.Mgam.CompositionRoot
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Options;
    using Aristocrat.Monaco.Protocol.Common.Installer;
    using Commands;
    using Common;
    using Common.Configuration;
    using Common.Data;
    using Common.Data.Models;
    using Consumers;
    using Kernel;
    using log4net;
    using Mappings;
    using Middleware;
    using Monaco.Common.Container;
    using Monaco.Common.Storage;
    using PackageManifest;
    using PackageManifest.Ati;
    using PackageManifest.Models;
    using Services.Attributes;
    using Services.Communications;
    using Services.CreditValidators;
    using Services.Devices;
    using Services.DropMode;
    using Services.Event;
    using Services.GameConfiguration;
    using Services.GamePlay;
    using Services.Identification;
    using Services.Lockup;
    using Services.Meters;
    using Services.Notification;
    using Services.PlayerTracking;
    using Services.Registration;
    using Services.Security;
    using Services.SoftwareValidation;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    ///     Initialize and configure container.
    /// </summary>
    public static class Bootstrapper
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
            container.AddResolveUnregisteredType(typeof(Bootstrapper).FullName, Logger);

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.RegisterExternalServices();

            container.RegisterClient(ConfigureOptions,
                c =>
                {
#if !RETAIL
                    c.MinimumLevel = LogLevel.Debug;
#else
                    c.MinimumLevel = LogLevel.Debug;
#endif
                }, Assembly.GetExecutingAssembly());

            container.UseMiddleware<SessionMiddleware>();

            container.AddDbContext();

            container.AddMappings();

            container.AddConsumers();

            container.AddCommands();

            return container.ConfigureServices();
        }

        private static Container ConfigureServices(this Container container)
        {
            container.RegisterSingleton<IEngine, MgamEngine>();

            container.RegisterSingleton<IEventDispatcher, EventDispatcher>();

            container.RegisterSingleton<IVoucherValidator, VoucherValidator>();
            container.RegisterSingleton<IHandpayValidator, HandpayValidator>();
            container.RegisterSingleton<ICurrencyValidator, CurrencyValidator>();
            container.RegisterSingleton<ITransactionRetryHandler, TransactionRetryHandler>();
            container.RegisterSingleton<IChecksumCalculator, ChecksumCalculator>();
            container.RegisterSingleton<IIdentificationValidator, IdentificationValidator>();
            container.RegisterSingleton<IPlayerTracking, PlayerTracking>();
            container.RegisterSingleton<ICashOut, CashOutHandler>();
            container.RegisterSingleton<CentralHandler>();

            container.RegisterSingleton<IDropMode, DropMode>();
            container.RegisterSingleton<ILockup, Lockup>();
            container.RegisterSingleton<IMeterMonitor, MeterMonitor>();

            container.RegisterSingleton<NoteAcceptorService>();
            container.RegisterSingleton<IRegistrationState, RegistrationState>();

            container.RegisterSingleton<ICommsObserver, CommsObserverService>();

            container.RegisterSingleton<IVltServiceLocator, VltServiceLocator>();

            container.RegisterSingleton<ISoftwareValidator, SoftwareValidator>();
            container.RegisterSingleton<IAttributeManager, AttributeManager>();
            container.RegisterSingleton<IInstallerService, InstallerService>();
            container.RegisterSingleton<IManifest<Image>, ImageManifest>();
            container.RegisterSingleton<IFileSystemProvider, FileSystemProvider>();
            container.RegisterSingleton<IZipArchive, ZipArchive>();
            container.RegisterSingleton<ITarArchive, TarArchive>();
            container.RegisterSingleton<IPackageService, PackageService>();

            container.RegisterSingleton<IProgressiveController, ProgressiveController>();

            container.RegisterSingleton<ICertificateService, CertificateService>();

            container.RegisterSingleton<IAutoPlay, AutoPlay>();

            container.RegisterSingleton<IGameConfigurator, GameConfigurator>();

            container.RegisterSingleton<IHostTranscripts, HostTranscripts>();

            container.RegisterSingleton<INotificationLift, NotificationLift>();
            container.RegisterSingleton<INotificationQueue, NotificationQueue>();

            return container;
        }

        private static void AddMappings(this Container container)
        {
            container.Register<MapperProvider>();
            container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper());
        }

        private static void AddConsumers(this Container container)
        {
            container.RegisterSingleton<ISharedConsumer, SharedConsumerContext>();
            container.Collection.Register(typeof(IConsumer<>), Assembly.GetExecutingAssembly());
        }

        private static void AddCommands(this Container container)
        {
            container.Register(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());
            container.RegisterSingleton<ICommandHandlerFactory, CommandHandlerFactory>();
        }

        private static void ConfigureOptions(ProtocolOptionsBuilder options)
        {
            int directoryPort;
            bool useBroadcast;
            string directoryIpAddress;

            using (var context = new MgamContext(
                new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>())))
            {
                var host = context.Hosts.SingleOrDefault();

                if (host == null)
                {
                    throw new InvalidOperationException("Host has not been configured");
                }

                directoryPort = host.DirectoryPort;
                useBroadcast = host.UseUdpBroadcasting;
                directoryIpAddress = host.DirectoryIpAddress;
            }

            var networkAddress = NetworkInterfaceInfo.DefaultIpAddress;

            IPAddress directoryAddress;

            if (!useBroadcast && !string.IsNullOrWhiteSpace(directoryIpAddress))
            {
                directoryAddress = IPAddress.Parse(directoryIpAddress);
            }
            else
            {
                directoryAddress = IPAddress.Broadcast;
            }

            options
                .BroadcastOn(new IPEndPoint(directoryAddress, directoryPort))
                .UseNetworkAddress(networkAddress);

            var configuration = ConfigurationUtilities.GetConfiguration<MgamConfiguration>(
                MgamConstants.ConfigurationExtensionPath,
                () => throw new InvalidOperationException(
                    $"MGAM configuration is not defined in Jurisdiction configuration, {MgamConstants.ConfigurationExtensionPath}"));

            if (configuration.ValidateServerCertificate)
            {
                options.EnableServerCertificateValidation();
            }
        }
    }
}
