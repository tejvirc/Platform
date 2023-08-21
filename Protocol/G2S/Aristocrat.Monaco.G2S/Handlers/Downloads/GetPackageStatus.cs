namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;

    /// <summary>
    ///     Handles the v21.getPackageStatus G2S message
    /// </summary>
    public class GetPackageStatus : ICommandHandler<download, getPackageStatus>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackageStatus" /> class.
        ///     Creates a new instance of the GetPackageStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public GetPackageStatus(
            IG2SEgm egm,
            IPackageManager packageManager,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getPackageStatus> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getPackageStatus> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                if (_packageManager.HasPackage(command.Command.pkgId))
                {
                    var response = command.GenerateResponse<packageStatus>();
                    response.Command.pkgId = command.Command.pkgId;
                    await _commandBuilder.Build(dl, response.Command);
                }
                else
                {
                    command.Error.Code = ErrorCode.G2S_DLX001;
                }
            }

            await Task.CompletedTask;
        }
    }
}