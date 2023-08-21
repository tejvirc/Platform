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
    ///     Handles the v21.pausePackageTransfer G2S message
    /// </summary>
    public class PausePackageTransfer : ICommandHandler<download, pausePackageTransfer>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PausePackageTransfer" /> class.
        ///     Creates a new instance of the PausePackageTransfer handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        public PausePackageTransfer(IG2SEgm egm, IPackageManager packageManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, pausePackageTransfer> command)
        {
            return await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, pausePackageTransfer> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                /*
                 *         /// TODO: Not Implemented in current release
        /// The pausePackageTransfer command is used by a host to direct the EGM to pause the specified download
        /// or upload operation currently in progress with the Software Download Distribution Point or any other
        /// transferLocation.The EGM generates a packageStatus command in response to the
        /// pausePackageTransfer command.
        /// When the host sends the pausePackageTransfer command to the EGM while the EGM is performing the
        /// download or upload operation with the Software Download Distribution Point or any other
        /// transferLocation, the EGM MUST, if possible, stop performing the download or upload operation and set
        /// the transferPaused attribute to true, and generate event GTK_DLE401 Package Transfer Paused.
        /// Upon receipt of the pausePackageTransfer command, if the transferState is not G2S_pending or
        /// G2S_inProgress, the EGM MUST reject the pausePackageTransfer command, and respond with error code
        /// GTK_DLX002 Unable to Pause Transfer. In addition, if for any other reason the EGM cannot pause the
        /// transfer—for example, the transfer protocol does not support pausing—the EGM MUST reject the
        /// pausePackageTransfer command, and respond with error code GTK_DLX002 Unable to Pause Transfer,
        /// continuing the transfer until completion.
        If the package does not exist in the packageList, error code G2S_DLX001 Command References A NonExistent
        Package MUST be included in the response to the host indicating that a non-existent package is
        specified.
        
        The EGM MUST NOT consider a pausePackageTransfer command logically equivalent to any previous
        pausePackageTransfer command. Each pausePackageTransfer command MUST be processed as if it were
        a unique command. Two or more pausePackageTransfer commands containing the same pkgId may not be
        logically equivalent.
        
        If the pkgId included in the pausePackageTransfer command references a package transfer that is not
        associated with the device identified in the class-level element of the command, the EGM MUST include error
        code G2S_DLX001 Command References A Non-Existent Package in its response.
        *
        *  */

                //// TODO: GTK_DLX003 Transfer Is Not Paused
                //// TODO: event 10.49.45 GTK_DLE401 Package Transfer Paused
                command.Error.Code = _packageManager.HasPackage(command.Command.pkgId) ? ErrorCode.G2S_DLX002 : ErrorCode.G2S_DLX001;
            }

            await Task.CompletedTask;
        }
    }
}