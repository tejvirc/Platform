namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Command builder for <see cref="mediaDisplayAck" /> response
    /// </summary>
    public class MediaDisplayAckCommandBuilder : ICommandBuilder<IMediaDisplay, mediaDisplayAck>
    {
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="MediaDisplayAckCommandBuilder" />
        /// </summary>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public MediaDisplayAckCommandBuilder(IMediaProvider mediaProvider)
        {
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task Build(IMediaDisplay device, mediaDisplayAck command)
        {
            var player = _mediaProvider.GetMediaPlayer(device.Id);

            if (null != player)
            {
                command.transactionId = player.ActiveMedia?.TransactionId ?? 0;
                command.contentId = player.ActiveMedia?.Id ?? 0;
                command.deviceVisibleState =
                    player.Visible ? t_deviceVisibleStates.IGT_shown : t_deviceVisibleStates.IGT_hidden;
            }

            await Task.CompletedTask;
        }
    }
}