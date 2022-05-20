namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using Kernel;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     Handles the v21.uploadPackage G2S message
    /// </summary>
    [ProhibitWhenDisabled]
    public class UploadPackage : ICommandHandler<download, uploadPackage>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGatService _gatService;
        private readonly IIdProvider _idProvider;
        private readonly IPackageManager _packageManager;
        private IDownloadDevice _downloadDevice;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UploadPackage" /> class.
        ///     Creates a new instance of the UploadPackage handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="eventBus">Event Bus.</param>
        /// <param name="gatService">Gat service.</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="idProvider">Id provider</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public UploadPackage(
            IG2SEgm egm,
            IPackageManager packageManager,
            IEventBus eventBus,
            IGatService gatService,
            IEventLift eventLift,
            IIdProvider idProvider,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, uploadPackage> command)
        {
            var error = await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);

            if (error != null)
            {
                return error;
            }

            var dl = _egm.GetDevice<IDownloadDevice>(command.HostId);
            if (dl == null)
            {
                return new Error(ErrorCode.G2S_APX999);
            }

            if (!dl.UploadEnabled)
            {
                return new Error(ErrorCode.G2S_DLX013);
            }

            if (!_packageManager.HasPackage(command.Command.pkgId))
            {
                return new Error(ErrorCode.G2S_DLX001);
            }

            // TODO: • If the EGM does not support the specified transferType, the error code G2S_DLX013 will be included
            /*in the response to the System Management Point indicating that the EGM does not support the
            transfer type.
            */

            var te = _packageManager.GetTransferEntity(command.Command.pkgId);
            if (te?.State == TransferState.InProgress || te?.State == TransferState.Pending)
            {
                return new Error(ErrorCode.G2S_DLX010);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, uploadPackage> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.HostId);
                if (dl == null)
                {
                    return;
                }

                _downloadDevice = dl;

                var package = _packageManager.GetPackageEntity(command.Command.pkgId);
                if (package.State == PackageState.Available)
                {
                    var ct = new CancellationTokenSource();
                    _packageManager.PackageTaskAbortTokens[command.Command.pkgId] = ct;

                    var transferEntity = new TransferEntity
                    {
                        TransferId = command.Command.transferId,
                        Location = command.Command.transferLocation,
                        PackageId = command.Command.pkgId,
                        Parameters = command.Command.transferParameters,
                        ReasonCode = command.Command.reasonCode,
                        State = TransferState.Pending,
                        TransferType = TransferType.Upload,
                        DeleteAfter = command.Command.deleteAfter
                    };

                    _packageManager.UpdateTransferEntity(transferEntity);

                    var packageLogEntity = new PackageLog
                    {
                        State = PackageState.InUse,
                        PackageId = package.PackageId,
                        Size = package.Size,
                        ReasonCode = command.Command.reasonCode,
                        Exception = 0,
                        TransactionId = _idProvider.GetNextTransactionId(),
                        DeviceId = dl.Id,
                    };

                    _packageManager.AddPackageLog(packageLogEntity);

                    PostEvent(_downloadDevice, EventCode.G2S_DLE120, command.Command.pkgId);
                    _packageManager.UploadPackage(
                        command.Command.pkgId,
                        transferEntity,
                        UpdatePackageStatus,
                        ct.Token,
                        packageLogEntity);
                }

                var response = command.GenerateResponse<packageStatus>();
                response.Command.pkgId = package.PackageId;

                await _commandBuilder.Build(dl, response.Command);
            }

            await Task.CompletedTask;
        }

        private void PostEvent(IDownloadDevice dl, string code, string packageId)
        {
            var log = _packageManager.GetPackageLogEntity(packageId);
            var packageLog = log.ToPackageLog();
            var info = new transactionInfo { deviceId = dl.Id, deviceClass = dl.PrefixedDeviceClass(), Item = packageLog };

            var transList = new transactionList { transactionInfo = new[] { info } };

            _eventLift.Report(
                dl,
                code,
                packageLog.transactionId,
                transList);
        }

        private void UpdatePackageStatus(PackageTransferEventArgs a)
        {
            if (_packageManager.HasPackage(a.PackageId))
            {
                var ps = new packageStatus { pkgId = a.PackageId};
                var package = _packageManager.GetPackageLogEntity(a.PackageId);
                var transfer = _packageManager.GetTransferEntity(a.PackageId);
                switch (a.TransferState)
                {
                    case TransferState.Completed:
                        PostEvent(_downloadDevice, EventCode.G2S_DLE124, a.PackageId);

                        if (transfer.TransferType == TransferType.Upload)
                        {
                            _eventBus.Publish(new PackageDownloadCompleteEvent());
                        }

                        if (transfer.DeleteAfter)
                        {
                            package.State = PackageState.DeletePending;
                        }

                        _packageManager.UpdatePackage(package);
                        SendStatus(_downloadDevice, ps);
                        _packageManager.PackageTaskAbortTokens.Remove(ps.pkgId);
                        break;
                    case TransferState.Failed:

                        if (package.State == PackageState.DeletePending)
                        {
                            DeletePackage(package.PackageId);
                        }
                        else
                        {
                            package.State = PackageState.Error;
                            package.Exception = transfer.Exception;
                        }

                        _packageManager.UpdatePackage(package);
                        SendStatus(_downloadDevice, ps);
                        _packageManager.PackageTaskAbortTokens.Remove(ps.pkgId);

                        //// TODO: verify this is the correct event and except (Update to 2 (two), transfer refused by host)
                        PostEvent(_downloadDevice, EventCode.G2S_DLE122, a.PackageId);
                        break;
                    case TransferState.InProgress:
                        PostEvent(_downloadDevice, EventCode.G2S_DLE121, a.PackageId);
                        PostEvent(_downloadDevice, EventCode.G2S_DLE123, a.PackageId);
                        break;
                    case TransferState.Pending:
                        package.TransferId = transfer.TransferId;
                        _packageManager.UpdatePackage(package);
                        break;
                }
            }
        }

        private void SendStatus(IDownloadDevice dl, packageStatus ps)
        {
            _commandBuilder.Build(dl, ps);
            dl.SendStatus(ps);
        }

        private void DeletePackage(string packageId)
        {
            var pe = _packageManager.GetPackageEntity(packageId);
            if (pe != null)
            {
                _packageManager.DeletePackage(
                    new DeletePackageArgs(
                        packageId,
                        a =>
                        {
                            PostEvent(_downloadDevice, EventCode.G2S_DLE140, packageId);
                            _gatService.DeleteComponent(packageId, ComponentType.Package);
                        }));
            }
        }
    }
}
