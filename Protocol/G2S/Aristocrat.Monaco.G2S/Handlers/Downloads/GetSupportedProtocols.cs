namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getSupportedProtocols G2S message
    /// </summary>
    public class GetSupportedProtocols : ICommandHandler<download, getSupportedProtocols>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetSupportedProtocols" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetSupportedProtocols(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getSupportedProtocols> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getSupportedProtocols> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var response = command.GenerateResponse<supportedProtocolList>();

                response.Command.supportedProtocol = new[]
                {
                    new supportedProtocol
                    {
                        transferProtocol = "G2S_http",
                        downloadSupported = true,
                        uploadSupported = true
                    },
                    new supportedProtocol
                    {
                        transferProtocol = "G2S_https",
                        downloadSupported = true,
                        uploadSupported = true
                    },
                    new supportedProtocol
                    {
                        transferProtocol = "G2S_ftp",
                        downloadSupported = true,
                        uploadSupported = true
                    },
                    new supportedProtocol
                    {
                        transferProtocol = "G2S_ftps",
                        downloadSupported = true,
                        uploadSupported = true
                    }
                };
            }

            await Task.CompletedTask;
        }
    }
}