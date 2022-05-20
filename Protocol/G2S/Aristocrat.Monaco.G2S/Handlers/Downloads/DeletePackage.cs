namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Kernel.Contracts.Components;
    using Data.Model;

    /// <summary>
    ///     Handles the v21.deletePackage G2S message
    /// </summary>
    public class DeletePackage : ICommandHandler<download, deletePackage>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGatService _gatService;
        private readonly IIdProvider _idProvider;
        private readonly IPackageManager _packageManager;
        private readonly IScriptManager _scriptManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeletePackage" /> class.
        ///     Creates a new instance of the DeletePackage handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager.</param>
        /// <param name="scriptManager">Script Manager.</param>
        /// <param name="gatService">Gat Service.</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="idProvider">Id provider</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public DeletePackage(
            IG2SEgm egm,
            IPackageManager packageManager,
            IScriptManager scriptManager,
            IGatService gatService,
            IEventLift eventLift,
            IIdProvider idProvider,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, deletePackage> command)
        {
            var error = await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var device = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);

            return !device.DownloadEnabled ? new Error(ErrorCode.G2S_APX008) : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, deletePackage> command)
        {
            var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
            if (dl == null)
            {
                return;
            }

            var pe = _packageManager.GetPackageEntity(command.Command.pkgId);
            var te = _packageManager.GetTransferEntity(command.Command.pkgId);

            if (pe == null && te == null || pe != null && pe.State == PackageState.Deleted)
            {
                command.Error.Code = ErrorCode.G2S_DLX001;
                return;
            }

            if (pe?.State == PackageState.InUse || CheckPackageReference(command))
            {
                command.Error.Code = ErrorCode.G2S_DLX003;
                return;
            }

            /*TODO: If the package is being transferred, the transfer MAY abort before completing the transfer. The
      package will be marked for deletion (G2S_deletePending) if the transfer cannot abort. In either case,
      the package ultimately will be removed from the packageList following either the aborted transfer or
      the completion of the transfer.*/

            bool transferInProgress = false;

            var packageLog = _packageManager.GetPackageLogEntity(command.Command.pkgId).CreateActivityLog();
            packageLog.TransactionId = 0;
            packageLog.ActivityDateTime = DateTime.UtcNow;
            packageLog.ActivityType = PackageActivityType.Delete;
            packageLog.ReasonCode = command.Command.reasonCode;
            packageLog.State = PackageState.DeletePending;
            _packageManager.AddPackageLog(packageLog);
            
            if (_packageManager.PackageTaskAbortTokens.ContainsKey(command.Command.pkgId))
            {
                transferInProgress = true;
                _packageManager.PackageTaskAbortTokens[command.Command.pkgId].Cancel();
                _packageManager.PackageTaskAbortTokens.Remove(command.Command.pkgId);

                _packageManager.UpdateTransferEntity(te);
            }

            var response = command.GenerateResponse<packageStatus>();
            response.Command.pkgId = command.Command.pkgId;
            response.Command.Item = new packageActivityStatus();
            await _commandBuilder.Build(dl, response.Command);

            if (!transferInProgress)
            {
                DeleteAsync(command, packageLog, dl);
            }
        }

        private void DeleteAsync(ClassCommand<download, deletePackage> command, PackageLog pe, IDownloadDevice dl)
        {
            Task.Run(() => Delete(command, pe, dl));
        }

        private void Delete(ClassCommand<download, deletePackage> command, PackageLog pe, IDownloadDevice dl)
        {
            _packageManager.DeletePackage(
                new DeletePackageArgs(
                    pe.PackageId,
                    a =>
                    {
                        pe.State = PackageState.Deleted;

                        _gatService.DeleteComponent(pe.PackageId, ComponentType.Package);

                        pe.DeviceId = dl.Id;
                        pe.TransactionId = _idProvider.GetNextTransactionId();
                        _packageManager.UpdatePackage(pe);

                        var status = new packageStatus
                        {
                            pkgId = command.Command.pkgId,
                            Item = new packageActivityStatus()
                        };
                        _commandBuilder.Build(dl, status);
                        dl.SendStatus(status);

                        PostEvent(dl, EventCode.G2S_DLE140, command.Command.pkgId);
                    }));

            /*After deleting the package from the packageList, the EGM MUST also update the status of any
      script that referenced the package. Furthermore, a package that is deleted will remain in the package log with
      the deleted status, even though it is no longer in the package list.*/

            foreach (var se in _packageManager.ScriptEntityList)
            {
                if (se.State == ScriptState.Canceled || se.State == ScriptState.Error ||
                    se.State == ScriptState.Completed)
                {
                    continue;
                }

                if (CheckPackageReference(command, se))
                {
                    se.State = ScriptState.Canceled;
                    _scriptManager.UpdateScript(se);
                }
            }
        }

        private bool CheckPackageReference(ClassCommand<download, deletePackage> command)
        {
            foreach (var se in _packageManager.ScriptEntityList)
            {
                if (se.State == ScriptState.Canceled || se.State == ScriptState.Error ||
                    se.State == ScriptState.Completed)
                {
                    continue;
                }

                if (CheckPackageReference(command, se))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckPackageReference(ClassCommand<download, deletePackage> command, Script se)
        {
            var referenced = false;

            var commandItems = _packageManager.ParseXml<commandStatusList>(se.CommandData).Items;

            foreach (var commandStatus in commandItems)
            {
                if (commandStatus is packageCmdStatus pkgCmdStatus)
                {
                    if (pkgCmdStatus.pkgId == command.Command.pkgId)
                    {
                        referenced = true;
                        break;
                    }
                }
            }

            return referenced;
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
    }
}
