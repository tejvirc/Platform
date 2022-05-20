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
    using Data.Model;

    /// <summary>
    ///     Handles the v21.createPackage G2S message
    /// </summary>
    [ProhibitWhenDisabled]
    public class CreatePackage : ICommandHandler<download, createPackage>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IIdProvider _idProvider;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreatePackage" /> class.
        ///     Creates a new instance of the CreatePackage handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="idProvider">Id provider</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public CreatePackage(
            IG2SEgm egm,
            IPackageManager packageManager,
            IEventLift eventLift,
            IIdProvider idProvider,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, createPackage> command)
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
        public async Task Handle(ClassCommand<download, createPackage> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                if (_packageManager.HasPackage(command.Command.pkgId) && !command.Command.overwrite)
                {
                    command.Error.Code = ErrorCode.G2S_DLX002;
                    return;
                }

                Module module = null;
                foreach (var ms in command.Command.moduleSelect)
                {
                    if ((module = _packageManager.GetModuleEntity(ms.modId)) == null)
                    {
                        command.Error.Code = ErrorCode.G2S_DLX009;
                        return;
                    }
                }

                var pe = new PackageLog
                {
                    PackageId = command.Command.pkgId,
                    State = PackageState.Pending,
                    TransactionId = _idProvider.GetNextTransactionId(),
                    ActivityDateTime = DateTime.UtcNow,
                    ActivityType = PackageActivityType.Create,
                    Overwrite = command.Command.overwrite,
                    ReasonCode = command.Command.reasonCode
                };
                
                _packageManager.AddPackageLog(pe);
                if (_packageManager.CreatePackage(
                        pe,
                        module,
                        command.Command.overwrite,
                        command.Command.pkgFormat == t_pkgFormats.G2S_tar ? ".tar" : ".zip") ==
                    CreatePackageState.Created)
                {
                    PostEvent(dl, EventCode.G2S_DLE110, command.Command.pkgId);
                }

                var response = command.GenerateResponse<packageStatus>();
                response.Command.pkgId = pe.PackageId;
                response.Command.Item = new packageActivityStatus();
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
    }
}