namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     A command builder for <see cref="contentStatus" /> response
    /// </summary>
    public class ContentStatusCommandBuilder : ICommandBuilder<IMediaDisplay, contentStatus>
    {
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="ContentStatusCommandBuilder" />
        /// </summary>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public ContentStatusCommandBuilder(IMediaProvider mediaProvider)
        {
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task Build(IMediaDisplay device, contentStatus command)
        {
            var media = _mediaProvider.GetMedia(device.Id, command.contentId, command.transactionId);

            if (null != media)
            {
                command.contentState = media.State.ToProtocolType();
                command.contentException = media.ExceptionCode.ToProtocolInt();
            }

            await Task.CompletedTask;
        }
    }
}