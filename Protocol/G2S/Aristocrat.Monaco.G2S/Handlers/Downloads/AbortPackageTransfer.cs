namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Data.Model;

    /// <summary>
    ///     Handles the v21.abortPackageTransfer G2S message
    /// </summary>
    public class AbortPackageTransfer : ICommandHandler<download, abortPackageTransfer>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbortPackageTransfer" /> class.
        ///     Creates a new instance of the AbortPackageTransfer handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public AbortPackageTransfer(
            IG2SEgm egm,
            IPackageManager packageManager,
            IEventLift eventLift,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, abortPackageTransfer> command)
        {
            return await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, abortPackageTransfer> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                if (!_packageManager.HasPackage(command.Command.pkgId))
                {
                    command.Error.Code = ErrorCode.G2S_DLX001;
                    return;
                }

                //// TODO:
                /*if the EGM is unable to abort the
        download or upload before it is complete, then the EGM will execute the current operation to completion and
        respond with error code GTK_DLX001 Unable to Abort Transfer, leaving the package intact until directed by
        the host to delete it.*/

                if (_packageManager.IsTransferring(command.Command.pkgId))
                {
                    var te = _packageManager.GetTransferEntity(command.Command.pkgId);

                    _packageManager.PackageTaskAbortTokens?[command.Command.pkgId].Cancel();

                    if (te.TransferType == TransferType.Download)
                    {
                        _packageManager.DeletePackage(
                            new DeletePackageArgs(
                                command.Command.pkgId,
                                a => PostEvent(device, EventCode.G2S_DLE107, command.Command.pkgId)));
                    }
                    else
                    {
                        PostEvent(device, EventCode.G2S_DLE125, command.Command.pkgId);
                    }

                    te.State = TransferState.Failed;

                    _packageManager.UpdateTransferEntity(te);

                    // TODO: update Transfer and package entity!!!
                    _packageManager.PackageTaskAbortTokens?.Remove(command.Command.pkgId);

                    var response = command.GenerateResponse<packageStatus>();
                    response.Command.pkgId = command.Command.pkgId;
                    await _commandBuilder.Build(device, response.Command);
                }
                else
                {
                    command.Error.Code = /*TODO: ErrorCode.*/ "GTK_DLX001";
                }
            }

            await Task.CompletedTask;
        }

        private void PostEvent(IDownloadDevice dl, string code, string packageId)
        {
            var log = _packageManager.GetPackageLogEntity(packageId);
            var packageLog = log.ToPackageLog();

            var info = new transactionInfo
            {
                deviceId = dl.Id, deviceClass = dl.PrefixedDeviceClass(), Item = packageLog
            };

            var transList = new transactionList { transactionInfo = new[] { info } };

            _eventLift.Report(
                dl,
                code,
                packageLog.transactionId,
                transList);
        }
    }
}