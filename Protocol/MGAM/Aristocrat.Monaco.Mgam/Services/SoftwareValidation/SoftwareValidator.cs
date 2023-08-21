namespace Aristocrat.Monaco.Mgam.Services.SoftwareValidation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Protocol;
    using Aristocrat.Mgam.Client.Services.Directory;
    using Common.Data.Models;
    using Common.Events;
    using CreditValidators;
    using FluentFTP;
    using Gaming.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Protocol.Common.Storage.Entity;
    using Security;
    using Device = Hardware.Contracts.SharedDevice.Device;

    public class SoftwareValidator : ISoftwareValidator, IDisposable
    {
        private const int LocateResponseTimeout = 30;
        private const int ValidateRetryTimeout = 30000;
        private const int ConnectionRetryTimeout = 60000;
        private const string DownloadsDirectoryPath = "/Downloads";
        private const string GdsProtocol = "GDS";
        private const string CertificatePackageId = "Certificate";
        private const string SoftwareExtension = ".iso";

        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEgm _egm;
        private readonly IXmlMessageSerializer _xmlMessageSerializer;
        private readonly Device _deviceConfiguration;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IPathMapper _pathMapper;
        private readonly IInstallerService _installerService;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IChecksumCalculator _checksumCalculator;
        private readonly ICertificateService _certificateService;
        private readonly IPrinter _printer;
        private readonly INoteAcceptor _noteAcceptor;
        private readonly IGameHistory _gameHistory;
        private readonly ICashOut _cashOutHandler;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private IDisposable _serviceListener;
        private Timer _relocateTimer;

        private bool _disposed;

        public SoftwareValidator(
            ILogger<SoftwareValidator> logger,
            IEventBus eventBus,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEgm egm,
            IIO ioService,
            IXmlMessageSerializer xmlMessageSerializer,
            IPathMapper pathMapper,
            IFileSystemProvider fileSystemProvider,
            IInstallerService installerService,
            IComponentRegistry componentRegistry,
            IChecksumCalculator checksumCalculator,
            ICertificateService certificateService,
            IGameHistory gameHistory,
            ICashOut cashOutHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceConfiguration = ioService?.DeviceConfiguration ?? throw new ArgumentNullException(nameof(ioService));
            _xmlMessageSerializer =
                xmlMessageSerializer ?? throw new ArgumentNullException(nameof(xmlMessageSerializer));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _checksumCalculator = checksumCalculator ?? throw new ArgumentNullException(nameof(checksumCalculator));
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            _relocateTimer = new Timer(_ => Validate(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <inheritdoc />
        ~SoftwareValidator()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async void Validate()
        {
            try
            {
                if (_egm.State >= EgmState.Stopping)
                {
                    return;
                }

                var directory = _egm.GetService<IDirectory>();

                string deviceName;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    deviceName = unitOfWork.Repository<Common.Data.Models.Device>().Queryable().Single().Name;
                }

                var manufacturer = _deviceConfiguration.Manufacturer;

                _serviceListener?.Dispose();
                _serviceListener = await directory.LocateXadf(
                        deviceName,
                        manufacturer,
                        async response =>
                        {
                            EnableRelocateTimer(false);

                            var discoveryComplete = false;

                            if (response.ResponseCode == ServerResponseCode.Ok)
                            {
                                try
                                {
                                    discoveryComplete = await DiscoverSoftware(response.Xadf, response.DownloadServer);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed software discovery with exception");
                                }
                            }

                            if (!discoveryComplete)
                            {
                                RetryValidation();
                            }
                            else
                            {
                                _eventBus.Publish(new ProtocolsInitializedEvent());
                            }
                        });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Locate software download service failure");
            }

            EnableRelocateTimer(true);
        }

        private async void RetryValidation()
        {
            _logger.LogError("Failed to complete software validation");

            await Task.Delay(ValidateRetryTimeout);

            Validate();
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                _autoResetEvent.Set();
                _autoResetEvent.Close();

                _eventBus?.UnsubscribeAll(this);

                if (_serviceListener != null)
                {
                    _serviceListener.Dispose();
                }

                if (_relocateTimer != null)
                {
                    _relocateTimer.Dispose();
                }
            }

            _serviceListener = null;
            _relocateTimer = null;
        }

        private async Task<bool> DiscoverSoftware(string file, string downloadServer)
        {
            if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(downloadServer))
            {
                var downloadConnectionInfo = downloadServer.Split(':', '@');
                if (downloadConnectionInfo.Length == 4)
                {
                    var user = downloadConnectionInfo[0];
                    var password = downloadConnectionInfo[1];
                    var host = downloadConnectionInfo[2];
                    var port = int.Parse(downloadConnectionInfo[3]);
                    return await Download(file, user, password, host, port);
                }
            }

            if(string.IsNullOrEmpty(file))
            {
                _logger.LogError($"Missing XADF file");
            }
            else
            {
                _logger.LogError($"Invalid FTP address received {downloadServer}");
            }

#if RETAIL
            return false;
#else
            // This is true to handle bypassing the Simulator check
            return true;
#endif
        }

        private async Task<bool> Download(string file, string userName, string password, string host, int port)
        {
            using (var ftpClient = new FtpClient())
            {
                var xadf = await DownloadXadf(file, userName, password, host, port, ftpClient);

                if (xadf == null)
                {
                    _logger.LogError("Unable to download XADF file from server");
                    return false;
                }

                if (!Guid.TryParseExact(xadf.Header.ApplicationGUID, "B", out var guid))
                {
                    _logger.LogError($"Unable to parse the application guid {xadf.Header.ApplicationGUID}");
                    return false;
                }

                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var repository = unitOfWork.Repository<Application>();

                    var application = repository.Queryable().FirstOrDefault();

                    if (application != null && application.ApplicationGuid.Equals(guid))
                    {
                        _logger.LogInfo($"Application software is current {guid}");

                        var installationRepository = unitOfWork.Repository<Installation>();

                        var installation = installationRepository.Queryable().FirstOrDefault();

                        if(installation != null && !application.ApplicationGuid.Equals(installation.InstallationGuid))
                        {
                            var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                            installation.Name = $"{fileVersion.ProductName} {fileVersion.FileVersion}";
                            installation.InstallationGuid = application.ApplicationGuid;

                            application.Name = fileVersion.ProductName;
                            application.Version = fileVersion.FileVersion;

                            repository.AddOrUpdate(application);
                            installationRepository.AddOrUpdate(installation);

                            unitOfWork.SaveChanges();
                        }

                        var missingComponent = false;
                        if (!_checksumCalculator.CheckSignature(xadf.Header.Signature))
                        {
                            // is there a software module that needs to be removed?
                            foreach (var resource in xadf.Resources)
                            {
                                if (CheckComponentName(resource.Name) &&
                                    _componentRegistry.Components.Any(c => c.ComponentId.Contains(resource.Name)))
                                {
                                    var component = _componentRegistry.Get($"{resource.Name}.iso");

                                    var components = _componentRegistry.Components.Where(
                                        c =>
                                            c.Type == ComponentType.Module &&
                                            c.Description.Equals(component.Description) &&
                                            !c.ComponentId.Contains(resource.Name)).ToList();

                                    Uninstall(components);
                                }
                                else
                                {
                                    missingComponent = true;
                                }
                            }
                        }

                        if (!missingComponent)
                        {
                            var resource = xadf.Resources.SingleOrDefault(r => r.Name == CertificatePackageId);
                            if (resource != null)
                            {
                                var downloadedPackages = DownloadPackages(
                                    new[] { resource },
                                    guid,
                                    _pathMapper.GetDirectory(DownloadsDirectoryPath),
                                    ftpClient);

                                InstallDownloadedPackages(downloadedPackages);
                            }

                            return _checksumCalculator.CheckSignature(xadf.Header.Signature);
                        }
                    }
                }

                GameRecoveryCheck();
                
                CreditCheck();
                
                _eventBus.Publish(new SoftwareUpgradeStartedEvent());

                if (xadf.Resources != null)
                {
                    var downloadedPackages = DownloadPackages(
                        xadf.Resources,
                        guid,
                        _pathMapper.GetDirectory(DownloadsDirectoryPath),
                        ftpClient);

                    var exitActions = InstallDownloadedPackages(downloadedPackages);

                    using (var unitOfWork = _unitOfWorkFactory.Create())
                    {
                        var repository = unitOfWork.Repository<Application>();

                        var application = repository.Queryable().FirstOrDefault();
                        if (application == null)
                        {
                            return false;
                        }

                        application.ApplicationGuid = guid;

                        repository.AddOrUpdate(application);

                        unitOfWork.SaveChanges();
                    }

                    foreach (var action in exitActions)
                    {
                        _eventBus.Publish(new ExitRequestedEvent(action));
                    }
                }

                return _checksumCalculator.CheckSignature(xadf.Header.Signature);
            }
        }

        private static bool CheckComponentName(string name)
        {
            return name.StartsWith(
                       GamingConstants.PlatformPackagePrefix,
                       StringComparison.InvariantCultureIgnoreCase) ||
                   name.StartsWith(
                       GamingConstants.JurisdictionPackagePrefix,
                       StringComparison.InvariantCultureIgnoreCase) ||
                   name.StartsWith(
                       GamingConstants.RuntimePackagePrefix,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private void GameRecoveryCheck()
        {
            if(_disposed)
            {
                return;
            }

            if(_gameHistory.IsRecoveryNeeded)
            {
                _logger.LogInfo("Game recovery must complete before software download");

                _autoResetEvent.Reset();
                _eventBus.Subscribe<GamePlayStateChangedEvent>(this,
                    evt =>
                    {
                        if(evt.CurrentState == PlayState.Idle)
                        {
                            _autoResetEvent.Set();
                            _eventBus.Unsubscribe<GamePlayStateChangedEvent>(this);
                        }
                    });

                _autoResetEvent.WaitOne();
            }
        }

        private void CreditCheck()
        {
            if (_disposed)
            {
                return;
            }

            if (_cashOutHandler.Balance > 0)
            {
                _logger.LogInfo("Player cashOutHandler must be $0 before software download");

                _cashOutHandler.CashOut();
                _autoResetEvent.Reset();
                _eventBus.Subscribe<VoucherIssuedEvent>(this,
                    evt =>
                    {
                        _autoResetEvent.Set();
                        _eventBus.Unsubscribe<VoucherIssuedEvent>(this);
                    });

                _autoResetEvent.WaitOne();
            }
        }

        private async Task<ApplicationPackageDescription> DownloadXadf(
            string file,
            string userName,
            string password,
            string host,
            int port,
            FtpClient ftpClient)
        {
            ftpClient.Host = host;
            ftpClient.Credentials = new NetworkCredential(userName, password);
            ftpClient.Port = port;
            do
            {
                await ftpClient.ConnectAsync();
                if (!ftpClient.IsConnected)
                {
                    await Task.Delay(ConnectionRetryTimeout);
                    continue;
                }

                using (var stream = new MemoryStream())
                {
                    if (await ftpClient.DownloadAsync(
                        stream,
                        $"\\{_deviceConfiguration.Manufacturer}\\{file}"))
                    {
                        if (_xmlMessageSerializer.TryDeserialize(
                            Encoding.UTF8.GetString(stream.ToArray()),
                            out var xadfMessage))
                        {
                            return xadfMessage as ApplicationPackageDescription;
                        }
                    }
                }
            } while (!ftpClient.IsConnected);

            return null;
        }

        private List<(string packageId, string fileName, string checksum, bool firmware)> DownloadPackages(
            ApplicationPackageDescriptionResource[] resources,
            Guid guid,
            DirectoryInfo temporaryDirectory,
            FtpClient ftpClient)
        {
            var downloadedPackages = new List<(string packageId, string fileName, string checksum, bool firmware)>();

            foreach (var resource in resources)
            {
                var firmware = false;
                if (resource.Attributes != null && resource.Attributes.Length > 0)
                {
                    firmware = _printer != null &&
                          _printer.DeviceConfiguration.Model.ToLower()
                              .Equals(resource.Attributes[0].Value.ToLower()) &&
                          _printer.DeviceConfiguration.Protocol.Equals(GdsProtocol) ||
                          _noteAcceptor != null &&
                          _noteAcceptor.DeviceConfiguration.Model.ToLower()
                              .Equals(resource.Attributes[0].Value.ToLower()) &&
                          _noteAcceptor.DeviceConfiguration.Protocol.Equals(GdsProtocol);

                    if (!firmware)
                    {
                        _logger.LogInfo(
                            $"Firmware update for {resource.Name} {resource.Attributes[0].Value} is not supported.");
                        continue;
                    }
                }

                if (!_componentRegistry.Components.Any(c => c.ComponentId.Contains(resource.Name)) ||
                    resource.Name == CertificatePackageId)
                {
                    var guidString = guid.ToString("D").ToUpper();
                    var location =
                        $"\\{_deviceConfiguration.Manufacturer}\\Package{guidString}\\{resource.FileLocation}";
                    var fileName = Path.Combine(temporaryDirectory.FullName, resource.FileLocation);
                    _logger.LogInfo($"Download {resource.Name} started");
                    using (var stream = _fileSystemProvider.GetFileWriteStream(fileName))
                    {
                        if (ftpClient.Download(
                            stream,
                            location))
                        {
                            downloadedPackages.Add((resource.Name, fileName, resource.Checksum, firmware));
                        }
                        else
                        {
                            _logger.LogError($"Failed to download {resource.Name} from {location}");
                        }
                    }
                }
            }

            return downloadedPackages;
        }

        private List<ExitAction> InstallDownloadedPackages(
            List<(string packageId, string fileName, string checksum, bool firmware)> downloadedPackages)
        {
            var exitActions = new List<ExitAction>();
            foreach (var package in downloadedPackages)
            {
                if (!_checksumCalculator.ValidateFile(package.fileName, unchecked((uint)int.Parse(package.checksum))))
                {
                    throw new Exception(
                        $"Invalid package downloaded {package.fileName} checksum {package.checksum}");
                }

                if (package.packageId == CertificatePackageId)
                {
                    _certificateService.ReplaceCaCertificate(package.fileName);
                    _fileSystemProvider.DeleteFile(package.fileName);
                    continue;
                }

                var result = _installerService.InstallPackage(package.packageId);

                if (!package.firmware && !CheckComponentName(package.packageId))
                {
                    var component = _componentRegistry.Get($"{package.packageId}.iso");

                    var components = _componentRegistry.Components.Where(
                        c =>
                            c.Type == ComponentType.Module &&
                            c.Description.Equals(component.Description) &&
                            !c.ComponentId.Contains(package.packageId)).ToList();

                    _eventBus.Publish(new TerminateGameProcessEvent());
                    Uninstall(components);

                    result.action = package.packageId.StartsWith(
                        GamingConstants.PlatformPackagePrefix,
                        StringComparison.InvariantCultureIgnoreCase) ? ExitAction.Reboot : ExitAction.Restart;
                }

                if (result.action.HasValue)
                {
                    exitActions.Add(result.action.Value);
                }

                _fileSystemProvider.DeleteFile(package.fileName);
            }

            return exitActions;
        }

        private void Uninstall(IList<Component> components)
        {
            foreach (var softwarePackage in components)
            {
                var packageId = softwarePackage.ComponentId.Replace(SoftwareExtension, string.Empty);

                _installerService.UninstallSoftwarePackage(packageId);
            }
        }

        private void EnableRelocateTimer(bool enable)
        {
            _relocateTimer.Change(
                enable ? TimeSpan.FromSeconds(LocateResponseTimeout) : Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }
    }
}