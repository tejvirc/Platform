namespace Aristocrat.G2S.Emdi.Consumers
{
    using Events;
    using Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="HideMediaPlayerEvent"/> event.
    /// </summary>
    public class HideMediaPlayerConsumer : Consumes<HideMediaPlayerEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HideMediaPlayerConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public HideMediaPlayerConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(HideMediaPlayerEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received HideMediaPlayerEvent event");

                await _reporter.ReportAsync(
                    new c_eventReportEventItem { eventCode = EventCodes.G2SDisplayDeviceHidden },
                    EventCodes.G2SDisplayDeviceHidden);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending display interface closed event report", ex);
            }
        }
    }
}
