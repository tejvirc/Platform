namespace Aristocrat.G2S.Emdi.Consumers
{
    using Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="MediaPlayerEmdiMessageFromHostEvent"/> event
    /// </summary>
    public class HostToContentConsumer : Consumes<MediaPlayerEmdiMessageFromHostEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IHostQueue _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostToContentConsumer"/> class.
        /// </summary>
        /// <param name="server"></param>
        public HostToContentConsumer(
            IHostQueue server)
        {
            _server = server;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(MediaPlayerEmdiMessageFromHostEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received MediaPlayerEmdiMessageFromHostEvent event");

                if (string.IsNullOrEmpty(theEvent.Base64Message))
                {
                    throw new ArgumentException("Message cannot be null or empty");
                }

                await _server[theEvent.MediaPlayer.Port].SendCommandAsync<mdCabinet, hostToContentMessage, hostToContentMessageAck>(
                    new hostToContentMessage
                    {
                        instructionData = new instructionData
                        {
                            Value = Convert.FromBase64String(theEvent.Base64Message)
                        }
                    });
            }
            catch (MessageException ex)
            {
                Logger.Error($"EMDI: Error ({ex.ErrorCode}) sending message from host to content on port {theEvent.MediaPlayer.Port}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error sending message from host to content on port {theEvent.MediaPlayer.Port}: {ex.Message}", ex);
            }
        }
    }
}
