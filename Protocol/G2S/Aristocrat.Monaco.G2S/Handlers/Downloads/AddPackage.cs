namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using Data.Model;

    /// <summary>
    ///     Handles the v21.addPackage G2S message
    /// </summary>
    [ProhibitWhenDisabled]
    public class AddPackage : ICommandHandler<download, addPackage>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;
        private readonly IPackageDownloadManager _packageDownloadManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddPackage" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="packageDownloadManager">Package Download Manager.</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public AddPackage(
            IG2SEgm egm,
            IPackageManager packageManager,
            IPackageDownloadManager packageDownloadManager,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _packageDownloadManager =
                packageDownloadManager ?? throw new ArgumentNullException(nameof(packageDownloadManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, addPackage> command)
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
        public async Task Handle(ClassCommand<download, addPackage> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                var location = command.Command.transferLocation.ToLower();
                if (!(location.StartsWith("ftp:")
                      || location.StartsWith("ftps:")
                      || location.StartsWith("http:")
                      || location.StartsWith("https:")
                      || location.StartsWith("sftp")))
                {
                    command.Error.Code = /*ErrorCode.*/"G2S_DLX019";
                    return;
                }

                if (_packageManager.HasPackage(command.Command.pkgId))
                {
                    if (!_packageManager.IsTransferring(command.Command.pkgId) &&
                        _packageManager.GetPackageEntity(command.Command.pkgId).State == PackageState.InUse
                        || CheckPackageReference(command))
                    {
                        command.Error.Code = ErrorCode.G2S_DLX003;
                        return;
                    }

                    command.Error.Code = ErrorCode.G2S_DLX002;
                    return;
                }

                if (!Path.GetFileNameWithoutExtension(command.Command.transferLocation)
                    .EndsWith($"{command.Command.pkgId}"))
                {
                    command.Error.Code = ErrorCode.G2S_DLX016;
                    return;
                }

                // TODO: Check storage size, error code G2S_DLX012
                // TODO: validate the transfer protocol, error code G2S_DLX019

                var transferEntity = new TransferEntity
                {
                    TransferId = command.Command.transferId,
                    Location = command.Command.transferLocation,
                    PackageId = command.Command.pkgId,
                    Parameters = command.Command.transferParameters,
                    ReasonCode = command.Command.reasonCode,
                    State = TransferState.Pending,
                    TransferType = TransferType.Download,
                    DeleteAfter = false,
                    TransferPaused = false,
                    TransferSize = 0,
                    Size = command.Command.pkgSize
                };

                _packageDownloadManager.PackageDownload(transferEntity, device);

                var status = command.GenerateResponse<packageStatus>().Command;
                status.pkgId = command.Command.pkgId;

                await _commandBuilder.Build(device, status);
            }

            await Task.CompletedTask;
        }

        private bool CheckPackageReference(ClassCommand<download, addPackage> command)
        {
            var referenced = false;

            foreach (var se in _packageManager.ScriptEntityList)
            {
                if (se.State == ScriptState.Canceled || se.State == ScriptState.Error ||
                    se.State == ScriptState.Completed)
                {
                    continue;
                }

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
            }

            return referenced;
        }
    }
}
