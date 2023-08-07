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
    /// Consumes the <see cref="ShowMediaPlayerEvent"/> event.
    /// </summary>
    public class ShowMediaPlayerConsumer : Consumes<ShowMediaPlayerEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMediaPlayerConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public ShowMediaPlayerConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }


        /// <inheritdoc />
        protected override async Task ConsumeAsync(ShowMediaPlayerEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received ShowMediaPlayerEvent event");

                await _reporter.ReportAsync(
                    new c_eventReportEventItem { eventCode = EventCodes.G2SDisplayDeviceShown },
                    EventCodes.G2SDisplayDeviceShown);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending display interface open/display device shown event report", ex);
            }
        }
    }
}
