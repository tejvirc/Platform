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
    ///     Handles the v21.resumePackageTransfer G2S message
    /// </summary>
    public class ResumePackageTransfer : ICommandHandler<download, resumePackageTransfer>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResumePackageTransfer" /> class.
        ///     Creates a new instance of the ResumePackageTransfer handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        public ResumePackageTransfer(IG2SEgm egm, IPackageManager packageManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, resumePackageTransfer> command)
        {
            return await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, resumePackageTransfer> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                /*    /// <summary>
               /// TODO: Not supported in current release!
               /// The resumePackageTransfer command is used by a host to direct the EGM to resume the specified
               /// download or upload package transfer operation which is currently paused with the Software Download
               /// Distribution Point or any other transferLocation.The EGM generates a packageStatus command in
               /// response to the resumePackageTransfer command.
               /// Upon receipt of the resumePackageTransfer command, if the transferPaused attribute is not set to true,
               /// the EGM MUST reject the resumePackageTransfer command, and respond with error code GTK_DLX003
               /// Transfer Is Not Paused, otherwise, the EGM MUST, if possible, resume performing the download or
               /// upload operation and set the transferPaused attribute to false and generate event G2S_DLE105 Package
               /// Download in Progress or event G2S_DLE123 Package Upload in Progress, as appropriate.
               /// If the EGM is not able to resume the download or upload, the EGM MUST respond with error code
               /// GTK_DLX004 Unable to Resume Transfer and generate event G2S_DLE107 Package Download Aborted or
               /// event G2S_DLE125 Package Upload Aborted, as appropriate.
               /// If the package does not exist in the packageList, error code G2S_DLX001 Command References A NonExistent
               /// Package MUST be included in the response to the host indicating that a non-existent package is
               /// specified.
               /// The EGM MUST NOT consider a resumePackageTransfer command logically equivalent to any previous
               /// resumePackageTransfer command.Each resumePackageTransfer command MUST be processed as if it
               /// were a unique command.Two or more resumePackageTransfer commands containing the same pkgId may
               /// not be logically equivalent.
               /// If the pkgId included in the resumePackageTransfer command references a package transfer that is not
               /// associated with the device identified in the class-level element of the command, the EGM MUST include error
               /// code G2S_DLX001 Command References A Non-Existent Package in its response
               /// </summary>
               /// <param name="command">The command.</param>
               /// <returns>An error code.</returns>*/

                command.Error.Code = _packageManager.HasPackage(command.Command.pkgId) ? ErrorCode.G2S_DLX004 : ErrorCode.G2S_DLX001;
            }

            await Task.CompletedTask;
        }
    }
}