namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="hostToContentMessage" /> message.  There is no response.
    /// </summary>
    public class HostToContentMessage : ICommandHandler<mediaDisplay, hostToContentMessage>
    {
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="HostToContentMessage" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public HostToContentMessage(
            IG2SEgm egm,
            IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, hostToContentMessage> command)
        {
            return await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public Task Handle(ClassCommand<mediaDisplay, hostToContentMessage> command)
        {
            // Re-encode the bytes back into Base64, because that's what the application is going to want.
            var encodeStr = Convert.ToBase64String(command.Command.instructionData.Value);

            _mediaProvider.SendEmdiFromHostToContent(command.Class.deviceId, encodeStr);

            return Task.CompletedTask;
        }
    }
}