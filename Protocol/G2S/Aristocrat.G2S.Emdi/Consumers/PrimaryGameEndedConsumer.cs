namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Reflection;
    using Events;
    using Extensions;
    using Host;
    using Monaco.Gaming.Contracts;
    using System.Threading.Tasks;
    using log4net;

    /// <summary>
    /// Consumes the <see cref="PrimaryGameEndedEvent"/> event
    /// </summary>
    public class PrimaryGameEndedConsumer : Consumes<PrimaryGameEndedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryGameEndedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public PrimaryGameEndedConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(PrimaryGameEndedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received PrimaryGameEndedEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SPrimaryGameEnded ? theEvent.Log.ToRecallLog() : null,
                    EventCodes.PrimaryGameEnded, EventCodes.G2SPrimaryGameEnded);

                await _reporter.ReportAsync(events, EventCodes.PrimaryGameEnded, EventCodes.G2SPrimaryGameEnded);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending primary game ended event report", ex);
            }
        }
    }
}
