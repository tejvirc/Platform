namespace Aristocrat.G2S.Emdi.Consumers
{
    using Events;
    using Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="ReleaseContentMediaPlayerEvent"/> event.
    /// </summary>
    public class ReleaseContentMediaPlayerConsumer : Consumes<ReleaseContentMediaPlayerEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMediaProvider _media;
        private readonly IHostQueue _server;
        private readonly IEventSubscriptions _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseContentMediaPlayerConsumer"/> class.
        /// </summary>
        /// <param name="media"></param>
        /// <param name="server"></param>
        /// <param name="events"></param>
        public ReleaseContentMediaPlayerConsumer(
            IMediaProvider media,
            IHostQueue server,
            IEventSubscriptions events)
        {
            _media = media;
            _server = server;
            _events = events;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(ReleaseContentMediaPlayerEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received ReleaseContentMediaPlayerEvent event");

                var activeContent = new activeContent
                {
                    mediaDisplayId = theEvent.MediaPlayer.Id,
                    contentId = theEvent.Media.Id
                };

                var subscribers = (await _events.GetSubscribersAsync(EventCodes.G2SDisplayInterfaceClosed))
                    .Where(sub => sub.Port != theEvent.MediaPlayer.Port);

                foreach (var sub in subscribers)
                {
                    var player = _media.GetMediaPlayers().First(x => x.Port == sub.Port);

                    if (player.ActiveMedia?.MdContentToken == theEvent.Media.MdContentToken)
                    {
                        await SendEventAsync(player.Port, activeContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending interface closed event report", ex);
            }
        }

        private async Task SendEventAsync(int port, object activeContent)
        {
            try
            {
                Logger.Debug($"EMDI: Sending events (G2S_MDE101) to port {port}");

                await _server[port].SendCommandAsync<mdEventHandler, eventReport, eventAck>(
                    new eventReport
                    {
                        eventItem = new[]
                        {
                            new c_eventReportEventItem
                            {
                                eventCode = EventCodes.G2SDisplayInterfaceClosed,
                                Item = activeContent
                            }
                        }
                    });
            }
            catch (MessageException ex)
            {
                Logger.Error($"EMDI: Error ({ex.ErrorCode}) sending events (G2S_MDE101) to {port}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error sending events (G2S_MDE101) to port {port}", ex);
            }
        }
    }
}
