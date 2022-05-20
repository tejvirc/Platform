namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getDownloadStatus G2S message
    /// </summary>
    public class GetDownloadStatus : ICommandHandler<download, getDownloadStatus>
    {
        private readonly ICommandBuilder<IDownloadDevice, downloadStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetDownloadStatus" /> class.
        ///     Creates a new instance of the GetDownloadStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="command">Command builder.</param>
        public GetDownloadStatus(IG2SEgm egm, ICommandBuilder<IDownloadDevice, downloadStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getDownloadStatus> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getDownloadStatus> command)
        {
            var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
            if (dl == null)
            {
                return;
            }

            var response = command.GenerateResponse<downloadStatus>();
            var status = response.Command;
            await _command.Build(dl, status);
        }
    }
}