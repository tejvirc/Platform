namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;

    /// <summary>
    ///     Handles the v21.getPackageList G2S message
    /// </summary>
    public class GetPackageList : ICommandHandler<download, getPackageList>
    {
        private readonly ICommandBuilder<IDownloadDevice, packageStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackageList" /> class.
        ///     Creates a new instance of the GetPackageList handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="commandBuilder">Package status command builder.</param>
        public GetPackageList(
            IG2SEgm egm,
            IPackageManager packageManager,
            ICommandBuilder<IDownloadDevice, packageStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getPackageList> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getPackageList> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var res = command.GenerateResponse<packageList>();

                var list = new List<packageStatus>();

                foreach (var pe in _packageManager.PackageEntityList)
                {
                    var status = new packageStatus
                    {
                        pkgId = pe.PackageId
                    };

                    await _commandBuilder.Build(dl, status);
                    list.Add(status);
                }

                res.Command.packageStatus = list.ToArray();
            }

            await Task.CompletedTask;
        }
    }
}