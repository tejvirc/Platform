namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Command builder for <see cref="mediaDisplayStatus" /> response
    /// </summary>
    public class MediaDisplayStatusCommandBuilder : ICommandBuilder<IMediaDisplay, mediaDisplayStatus>
    {
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="MediaDisplayStatusCommandBuilder" />
        /// </summary>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public MediaDisplayStatusCommandBuilder(IMediaProvider mediaProvider)
        {
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task Build(IMediaDisplay device, mediaDisplayStatus command)
        {
            var player = _mediaProvider.GetMediaPlayer(device.Id);

            command.configurationId = device.ConfigurationId;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.hostLocked = device.HostLocked;
            if (player != null)
            {
                if (player.ActiveMedia?.State != MediaState.Error)
                {
                    command.transactionId = player.ActiveMedia?.TransactionId ?? 0;
                    command.contentId = player.ActiveMedia?.Id ?? 0;
                    command.deviceVisibleState =
                        player.Visible ? t_deviceVisibleStates.IGT_shown : t_deviceVisibleStates.IGT_hidden;
                }
                else
                {
                    command.transactionId = 0;
                    command.contentId = 0;
                    command.deviceVisibleState = t_deviceVisibleStates.IGT_hidden;
                }
            }

            await Task.CompletedTask;
        }
    }
}
