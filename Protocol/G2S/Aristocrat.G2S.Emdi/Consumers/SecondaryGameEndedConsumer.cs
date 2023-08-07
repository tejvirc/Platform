namespace Aristocrat.G2S.Emdi.Consumers
{
    using Events;
    using Extensions;
    using Host;
    using log4net;
    using Monaco.Gaming.Contracts;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="SecondaryGameEndedEvent"/> event.
    /// </summary>
    public class SecondaryGameEndedConsumer : Consumes<SecondaryGameEndedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryGameEndedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public SecondaryGameEndedConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(SecondaryGameEndedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received SecondaryGameEndedEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SSecondaryGameEnded ? theEvent.Log.ToRecallLog() : null,
                        EventCodes.SecondaryGameEnded, EventCodes.G2SSecondaryGameEnded);

                await _reporter.ReportAsync(events, EventCodes.SecondaryGameEnded, EventCodes.G2SSecondaryGameEnded);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending secondary game ended event report", ex);
            }
        }
    }
}
