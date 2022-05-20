namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;

    /// <summary>
    ///     Handles the <see cref="MediaPlayerEmdiMessageToHostEvent" /> which sends EMDI data to the G2S host.
    /// </summary>
    public class MediaPlayerEmdiMessageToHostConsumer : Consumes<MediaPlayerEmdiMessageToHostEvent>
    {
        private readonly ICommandBuilder<IMediaDisplay, contentToHostMessage> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="MediaPlayerEmdiMessageToHostConsumer" />
        /// </summary>
        /// <param name="egm">A G2S EGM</param>
        public MediaPlayerEmdiMessageToHostConsumer(IG2SEgm egm)
        {
            _commandBuilder = new ContentToHostMessageCommandBuilder();
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(MediaPlayerEmdiMessageToHostEvent theEvent)
        {
            var device = _egm.GetDevice<IMediaDisplay>(theEvent.MediaPlayer.Id);

            var message = new contentToHostMessage
            {
                instructionData = new instructionData
                {
                    Value = Convert.FromBase64String(theEvent.Base64Message)
                }
            };

            _commandBuilder.Build(device, message);

            var request = device.MediaDisplayClassInstance;
            request.Item = message;

            device.Queue.SendRequest(request);
        }

        /// <summary>
        ///     Simple command builder for contentToHostMessage
        /// </summary>
        private class ContentToHostMessageCommandBuilder : ICommandBuilder<IMediaDisplay, contentToHostMessage>
        {
            /// <inheritdoc />
            public async Task Build(IMediaDisplay device, contentToHostMessage command)
            {
                await Task.CompletedTask;
            }
        }
    }
}
