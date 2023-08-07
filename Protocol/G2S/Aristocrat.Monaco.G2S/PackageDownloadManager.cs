namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Protocol.Common.Installer;
    using Common.Events;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using ExpressMapper;
    using Handlers;
    using Handlers.Downloads;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>
    ///     Package download manager implementation used to control and track G2S add package commands and states.
    /// </summary>
    public class PackageDownloadManager : IPackageDownloadManager, IDisposable
    {
        private const int DownloadDelayMilliseconds = 420000;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGatService _gatService;
        private readonly IPackageManager _packageManager;
        private readonly IIdProvider _idProvider;
        private readonly IInstallerService _installerService;
        private readonly CancellationTokenSource _cancelDownloading;
        private readonly ActionBlock<TransferEntity> _downloadProcessor;
        private readonly object _downloadLock = new object();

        private bool _disposed;
        private bool _onLine;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageDownloadManager" /> class.
        ///     Creates Instance of the Package Download Manager.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="eventBus">Event bus.</param>
        /// <param name="gatService">GAT service.</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        /// <param name="idProvider">ID provider.</param>
        public PackageDownloadManager(
            IG2SEgm egm,
            IPackageManager packageManager,
            IEventBus eventBus,
            IGatService gatService,
            IEventLift eventLift,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder,
            IIdProvider idProvider,
            IInstallerService installerService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));

            _cancelDownloading = new CancellationTokenSource();

            _downloadProcessor = new ActionBlock<TransferEntity>(
                async request =>
                {
                    var waitForDisconnect = false;
                    var transferEntity = _packageManager.GetTransferEntity(request.PackageId);
                    if (transferEntity.State == TransferState.Pending || transferEntity.State == TransferState.InProgress)
                    {
                        var packageLogEntity = _packageManager.GetPackageLogEntity(transferEntity.PackageId);

                        if (!_packageManager.PackageTaskAbortTokens.ContainsKey(transferEntity.PackageId))
                        {
                            var device = _egm.GetDevice<IDownloadDevice>();

                            if (device == null)
                            {
                                return;
                            }

                            waitForDisconnect = true;

                            var ct = new CancellationTokenSource();
                            _packageManager.PackageTaskAbortTokens[transferEntity.PackageId] = ct;
                            _packageManager.DownloadPackage(
                                transferEntity.PackageId,
                                transferEntity,
                                packageTransfer => UpdatePackageStatus(packageTransfer, device),
                                ct.Token,
                                packageLogEntity,
                                device.Id);
                        }
                    }

                    _cancelDownloading.Token.ThrowIfCancellationRequested();
                    if (waitForDisconnect)
                    {
                        await Task.Delay(DownloadDelayMilliseconds, _cancelDownloading.Token);
                    }
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Start()
        {
            _onLine = false;

            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, HandleEvent);
        }

        /// <inheritdoc />
        public void PackageDownload(TransferEntity transferEntity, IDownloadDevice device)
        {
            lock (_downloadLock)
            {
                var packageLogEntity = new PackageLog
                {
                    State = PackageState.Pending,
                    PackageId = transferEntity.PackageId,
                    Size = 0,
                    ReasonCode = transferEntity.ReasonCode,
                    Exception = 0,
                    TransactionId = _idProvider.GetNextTransactionId(),
                    DeviceId = device.Id
                };

                _packageManager.AddPackageLog(packageLogEntity);

                _packageManager.UpdateTransferEntity(transferEntity);

                _downloadProcessor.Post(transferEntity);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                _cancelDownloading.Cancel(false);
                _downloadProcessor.Complete();

                try
                {
                    _downloadProcessor.Completion.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle(ex => {
                        Logger.Error(ex);
                        return true;
                    });
                }

                _cancelDownloading.Dispose();
            }
        }

        private void PostEvent(IDownloadDevice dl, string code, string packageId)
        {
            var log = _packageManager.GetPackageLogEntity(packageId);
            var packageLog = log.ToPackageLog();

            var info = new transactionInfo
            {
                deviceId = dl.Id, deviceClass = dl.PrefixedDeviceClass(), Item = packageLog
            };

            _eventLift.Report(
                dl,
                code,
                packageLog.transactionId,
                new transactionList { transactionInfo = new[] { info } });
        }

        private void UpdatePackageStatus(PackageTransferEventArgs packageTransferArgs, IDownloadDevice dl)
        {
            if (_packageManager.HasPackage(packageTransferArgs.PackageId))
            {
                var packageStatus = new packageStatus
                {
                    pkgId = packageTransferArgs.PackageId
                };
                var transferEntity = _packageManager.GetTransferEntity(packageTransferArgs.PackageId);
                transferEntity.State = packageTransferArgs.TransferState;
                _packageManager.UpdateTransferEntity(transferEntity);
                var package = _packageManager.GetPackageLogEntity(packageTransferArgs.PackageId);

                switch (packageTransferArgs.TransferState)
                {
                    case TransferState.Completed:
                        transferEntity.TransferCompletedDateTime = DateTime.UtcNow;

                        if (transferEntity.TransferType == TransferType.Download)
                        {
                            _eventBus.Publish(new PackageDownloadCompleteEvent());
                        }

                        _packageManager.UpdatePackage(package);
                        _packageManager.PackageTaskAbortTokens.Remove(packageStatus.pkgId);

                        _packageManager.UpdateTransferEntity(transferEntity);

                        ////TODO: The following packageStatus broke the IGT 4.2 host.  We need to make this configurable (per jurisdiction?)
                        ////     The configuration should simply allow us to configure behavior for host compatibility issues
                        ////SendStatus(dl, ps, te);

                        PostEvent(dl, EventCode.G2S_DLE106, packageTransferArgs.PackageId);

                        Validate(dl, package, transferEntity, packageStatus, packageTransferArgs);
                        break;
                    case TransferState.Failed:
                        var toDelete = true;
                        if (package.State == PackageState.DeletePending)
                        {
                            toDelete = false;
                            DeletePackage();
                        }
                        else
                        {
                            package.State = PackageState.Deleted;
                        }

                        package.Exception = transferEntity.Exception == 2 ? transferEntity.Exception :
                            transferEntity.Exception == 6 ? 3 : 4;

                        _packageManager.UpdatePackage(package);
                        SendStatus(dl, packageStatus, transferEntity);
                        _packageManager.PackageTaskAbortTokens.Remove(packageStatus.pkgId);

                        var eventCode = transferEntity.Exception == 2 ? EventCode.G2S_DLE103 :
                            transferEntity.Exception == 6 ? EventCode.G2S_DLE104 : EventCode.G2S_DLE107;

                        PostEvent(dl, eventCode, packageTransferArgs.PackageId);
                        if (toDelete)
                        {
                            DeletePackage();
                        }

                        break;
                    case TransferState.InProgress:
                        PostEvent(dl, EventCode.G2S_DLE102, packageTransferArgs.PackageId);
                        PostEvent(dl, EventCode.G2S_DLE105, packageTransferArgs.PackageId);

                        break;
                    case TransferState.Pending:
                        PostEvent(dl, EventCode.G2S_DLE101, packageTransferArgs.PackageId);
                        break;
                }
            }

            void DeletePackage()
            {
                _packageManager.DeletePackage(
                    new DeletePackageArgs(
                        packageTransferArgs.PackageId,
                        args => { PackageDeleted(args.PackageId); }));
            }

            void PackageDeleted(string packageId)
            {
                var device = _egm.GetDevice<IDownloadDevice>();

                var package = _packageManager.GetPackageLogEntity(packageId);
                package.State = PackageState.Deleted;
                _packageManager.UpdatePackage(package);

                var packageStatus = new packageStatus
                {
                    pkgId = packageId
                };

                PostEvent(
                    device,
                    EventCode.G2S_DLE140,
                    packageId);
                SendStatus(device, packageStatus);
            }
        }

        private void Validate(
            IDownloadDevice device,
            PackageLog package,
            TransferEntity transferEntity,
            packageStatus status,
            PackageTransferEventArgs args)
        {
            if (_packageManager.ValidatePackage(args.PackageFilePath))
            {
                var component = new Component
                {
                    ComponentId = args.PackageId,
                    Description = $"{args.PackageId} Download",
                    FileSystemType = FileSystemType.File,
                    Path = args.PackageFilePath,
                    Size = args.Size,
                    Type = ComponentType.Package
                };

                _gatService.SaveComponent(component);

                transferEntity.PackageValidateDateTime = DateTime.UtcNow;
                package.State = PackageState.Available;
                _packageManager.UpdatePackage(package);
                transferEntity.State = TransferState.Validated;
                _packageManager.UpdateTransferEntity(transferEntity);

                SendStatus(device, status, transferEntity);

                PostEvent(device, EventCode.G2S_DLE108, args.PackageId);
            }
            else
            {
                package.State = PackageState.Error;
                package.Exception = 9;
                _packageManager.UpdatePackage(package);
                transferEntity.State = TransferState.Failed;
                transferEntity.Exception = 9;
                _packageManager.UpdateTransferEntity(transferEntity);

                SendStatus(device, status, transferEntity);

                PostEvent(device, EventCode.G2S_DLE109, args.PackageId);
            }
        }

        private void SendStatus(IDownloadDevice dl, packageStatus ps, TransferEntity te = null)
        {
            if (te != null)
            {
                ps.Item = Mapper.Map<TransferEntity, packageTransferStatus>(te);
            }

            _commandBuilder.Build(dl, ps);
            dl.SendStatus(ps);
        }

        private void HandleEvent(CommunicationsStateChangedEvent evt)
        {
            if (!evt.Online || _onLine)
            {
                return;
            }

            var downloadDevice = _egm?.GetDevice<IDownloadDevice>();
            if (downloadDevice?.Owner == evt.HostId)
            {
                _onLine = true;

                Logger.Info("checking for unfinished downloads...");

                foreach (var entity in _packageManager.TransferEntityList)
                {
                    if (entity.State is not (TransferState.InProgress or TransferState.Pending
                            or TransferState.Completed) ||
                        _packageManager.PackageTaskAbortTokens.ContainsKey(entity.PackageId))
                    {
                        continue;
                    }

                    _installerService.DeleteSoftwarePackage(entity.PackageId);
                    if (entity.State == TransferState.Completed)
                    {
                        entity.State = TransferState.Pending;
                        _packageManager.UpdateTransferEntity(entity);
                    }
                    PackageDownload(entity, downloadDevice);
                }
            }
        }
    }
}